using Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Logic
{
    public partial struct FireworkSystem : ISystem
    {
        private NativeList<NativeArray<float3>> _arrays;
        private float Delay;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabHolder>();
            _arrays = new NativeList<NativeArray<float3>>(Allocator.Persistent);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.CompleteDependency();
            if (!SystemAPI.TryGetSingletonEntity<FireworkInstruction>(out var instructionEntity)) return;
            var instructions = SystemAPI.GetBuffer<FireworkInstruction>(instructionEntity).ToNativeArray(Allocator.Temp);
            if (instructions.Length <= 0) return;
            var deltaTime = SystemAPI.Time.DeltaTime;
            if ((Delay -= deltaTime) > 0) return;

            var prefabHolder = SystemAPI.GetSingleton<PrefabHolder>();

            var startParticle = CreateStartParticle(ref state, prefabHolder.BasicParticle);
            var particleQueue = new NativeQueue<(Entity, float)>(Allocator.Temp);
            particleQueue.Enqueue((startParticle, 3f));

            var maxTime = 3f;
            var instructionIndex = 0;

            while (particleQueue.TryDequeue(out var pair))
            {
                var coreInstruction = NextInstruction(instructions, ref instructionIndex);
                if (coreInstruction == 0) continue;

                var (parentParticle, parentTime) = pair;
                var currentParticle = CreateBaseParticle(ref state, prefabHolder.BasicParticle);
                var otherParticle = CreateBaseParticle(ref state, prefabHolder.BasicParticle);

                var lifetime = math.remap(0, 0xFF, 0.5f, 3f, coreInstruction & 0xFF);
                var doubleParticle = coreInstruction % 4 == 3;
                coreInstruction >>= 8;
                state.EntityManager.SetComponentData(currentParticle, new ParticleTimer {RemainingTime = lifetime});
                state.EntityManager.SetComponentData(otherParticle, new ParticleTimer {RemainingTime = lifetime});

                var endTime = parentTime + lifetime;
                maxTime = math.max(maxTime, endTime);

                var keepSpawningCheck = math.remap(0, 0xFFFF, 10, 20f, (coreInstruction & 0xFFFF));
                coreInstruction >>= 16;

                var isEndStep = maxTime > keepSpawningCheck;
                if (!isEndStep)
                {
                    particleQueue.Enqueue((currentParticle, endTime));
                    if (doubleParticle)
                    {
                        particleQueue.Enqueue((otherParticle, endTime));
                    }
                }

                var chanceOfBigPattern = isEndStep ? 255 - 25 : 25;
                var willSpawnBigPattern = (coreInstruction & 0xFF) < chanceOfBigPattern;

                var minParticles = willSpawnBigPattern ? 25 : 2;
                var maxParticles = willSpawnBigPattern ? 128 : 16;

                var particleCountInstruction = NextInstruction(instructions, ref instructionIndex);
                var particleLerp = ToLowerFloat(particleCountInstruction);
                var particleCount = (int)math.lerp(minParticles, maxParticles, particleLerp);


                var is3d = particleCountInstruction % 8 == 0;
                if (isEndStep)
                {
                    particleCount *= is3d ? 10 : 2;
                }

                var patternOffset = NextInstruction(instructions, ref instructionIndex) / (float)0xFFFFFFFF;

                state.EntityManager.SetSharedComponent(
                    parentParticle,
                    new SpawnData
                    {
                        Prefab1 = currentParticle,
                        Prefab2 = doubleParticle ? otherParticle : currentParticle,
                        SpawnDirections = is3d ? Create3dPattern(particleCount, patternOffset) : Create2dPattern(particleCount, patternOffset),
                    }
                );

                instructionIndex = SetupParticle(ref state, instructions, ref instructionIndex, lifetime, currentParticle);
                instructionIndex = SetupParticle(ref state, instructions, ref instructionIndex, lifetime, otherParticle);
            }

            state.EntityManager.Instantiate(startParticle);

            var multiplyTime = NextInstruction(instructions, ref instructionIndex);
            if (multiplyTime > 0xFFFF0000)
            {
                multiplyTime -= 0xFFFF0000;
                maxTime *= multiplyTime / (float)0x0000FFFF;
            }

            if (maxTime > 2f)
            {
                maxTime -= 1f;
            }

            var instructionBuffer = SystemAPI.GetBuffer<FireworkInstruction>(instructionEntity);
            instructionBuffer.RemoveRangeSwapBack(0, instructionIndex);
            Delay += maxTime;
        }

        private int SetupParticle(ref SystemState state, NativeArray<FireworkInstruction> instructions, ref int instructionIndex, float lifetime, Entity currentParticle)
        {
            var speedInstruction = NextInstruction(instructions, ref instructionIndex);
            var withGravity = (speedInstruction % 4) == 0;
            speedInstruction >>= 2;
            var withDrag = (speedInstruction % 4) == 0;
            speedInstruction >>= 2;
            var withBuoyancy = (speedInstruction % 4) == 3;
            speedInstruction >>= 2;

            var maxSpeed = 8 / lifetime;
            var minSpeed = 3 / lifetime;
            var speed = math.lerp(minSpeed, maxSpeed, (speedInstruction & 0xFFFF) / (float)0xFFFF);

            speedInstruction >>= 16;
            if (withGravity)
            {
                var gravity = math.lerp(4f, 16f, ToLowerFloat(NextInstruction(instructions, ref instructionIndex)));
                speed += gravity * (1 - (speedInstruction & 0xF) / (30f));
                speedInstruction >>= 4;

                var randomDir = (speedInstruction & 0xF) == 0xF;
                var dir = randomDir ? RandomDir(instructions, ref instructionIndex) : new float3(0, -1, 0);
                state.EntityManager.AddComponent<ParticleGravity>(currentParticle);
                state.EntityManager.SetComponentData<ParticleGravity>(currentParticle, new() {Gravity = dir * gravity});
            }

            if (withDrag)
            {
                speed += 1;
                var drag = math.lerp(0.99f, 0.85f, ToLowerFloat(NextInstruction(instructions, ref instructionIndex)));
                state.EntityManager.AddComponent<ParticleDrag>(currentParticle);
                state.EntityManager.SetComponentData<ParticleDrag>(currentParticle, new() {Drag = drag});
            }

            if (withBuoyancy)
            {
                var buoyancy = math.lerp(8, 90, ToLowerFloat(NextInstruction(instructions, ref instructionIndex)));
                state.EntityManager.AddComponent<ParticleBuoyancy>(currentParticle);
                state.EntityManager.SetComponentData(currentParticle, new ParticleBuoyancy {Buoyancy = 100 * buoyancy});
            }

            state.EntityManager.SetComponentData(currentParticle, new ParticleSpeed {Speed = speed});


            var hue = NextInstruction(instructions, ref instructionIndex) / (float)0xFFFFFFFF;
            var saturation = NextInstruction(instructions, ref instructionIndex) / (float)0xFFFFFFFF;
            var color = (float4)(Vector4)Color.HSVToRGB(hue, saturation, 1);

            if (saturation > 0.15f)
            {
                state.EntityManager.AddComponent<MaterialColor>(currentParticle);
                state.EntityManager.SetComponentData(currentParticle, new MaterialColor {Value = color});
            }

            return instructionIndex;
        }

        private static float ToLowerFloat(uint value)
        {
            return (float)(value & 0xFF) * ((value >> 8) & 0xFF) * ((value >> 16) & 0xFFFF) / (255f * 255f * 255f * 255f);
        }

        private uint NextInstruction(NativeArray<FireworkInstruction> instructions, ref int instructionIndex)
        {
            if (instructionIndex >= instructions.Length) return 0;
            return instructions[instructionIndex++].Data;
        }

        private float3 RandomDir(NativeArray<FireworkInstruction> instructions, ref int instructionIndex)
        {
            var instruction = NextInstruction(instructions, ref instructionIndex);
            var r = math.PI * 2 * ((float)instruction) / 0xFFFFFFFF;
            return new float3(
                math.sin(r),
                math.cos(r),
                0
            );
        }

        private Entity CreateStartParticle(ref SystemState state, Entity prefab)
        {
            var baseParticle = CreateBaseParticle(ref state, prefab);
            state.EntityManager.SetComponentData(
                baseParticle,
                new ParticlePosition
                {
                    PreviousPosition = new float3(0, -50f, 0),
                    Position = new float3(0, -50f, 0),
                    PreviousTime = -1000,
                    Time = (float)SystemAPI.Time.ElapsedTime,
                }
            );
            state.EntityManager.SetComponentData(
                baseParticle,
                new LocalTransform
                {
                    Position = new float3(0, -50f, 0),
                    Rotation = quaternion.identity,
                    Scale = 1f,
                }
            );

            state.EntityManager.AddComponent<ParticleGravity>(baseParticle);

            state.EntityManager.SetComponentData(baseParticle, new ParticleDirection {Direction = new float3(0, 1f, 0f)});
            state.EntityManager.SetComponentData(baseParticle, new ParticleGravity {Gravity = new float3(0, -11.111f, 0f)});
            state.EntityManager.SetComponentData(baseParticle, new ParticleSpeed {Speed = 33.333f});
            state.EntityManager.SetComponentData(baseParticle, new ParticleTimer {RemainingTime = 3f});

            return baseParticle;
        }

        private Entity CreateBaseParticle(ref SystemState state, Entity prefab)
        {
            var particle = state.EntityManager.Instantiate(prefab);

            state.EntityManager.AddComponent<Prefab>(particle);
            state.EntityManager.AddComponent<ParticlePosition>(particle);
            state.EntityManager.AddComponent<ParticleTimer>(particle);
            state.EntityManager.AddComponent<ParticleDirection>(particle);
            state.EntityManager.AddComponent<ParticleSpeed>(particle);
            state.EntityManager.AddSharedComponent(particle, new SpawnData {Prefab1 = Entity.Null, Prefab2 = Entity.Null, SpawnDirections = default});
            return particle;
        }


        private NativeArray<float3> Create2dPattern(int count, float offset)
        {
            var array = new NativeArray<float3>(count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < count; i++)
            {
                var x = math.cos((i + offset) * Mathf.PI * 2f / count);
                var y = math.sin((i + offset) * Mathf.PI * 2f / count);
                array[i] = new float3(x, y, 0);
            }

            _arrays.Add(array);
            return array;
        }

        private NativeArray<float3> Create3dPattern(int count, float offset)
        {
            var array = new NativeArray<float3>(count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            var phi = math.PI * (3f - math.sqrt(5));
            for (var i = 0; i < count; i++)
            {
                var y = 1 - (i / (float)(count - 1)) * 2;
                var radius = math.sqrt(1 - y * y);
                var theta = phi * i + offset;

                var x = math.cos(theta) * radius;
                var z = math.sin(theta) * radius;
                array[i] = new float3(x, y, z);
            }

            _arrays.Add(array);
            return array;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            for (var i = 0; i < _arrays.Length; i++)
            {
                _arrays[i].Dispose();
            }

            _arrays.Dispose();
        }
    }
}