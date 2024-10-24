using Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Logic
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
    public partial struct ParticleTimerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            state.Dependency = new DecreaseParticleTimerJob
            {
                DeltaTime = deltaTime,
            }.ScheduleParallel(state.Dependency);

            state.CompleteDependency();
            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (spawnData, particlePosition, localTransform, spawner) in SystemAPI.Query<SpawnData, ParticlePosition, LocalTransform>().WithDisabled<ParticleTimer>().WithEntityAccess())
            {
                if (spawnData.Prefab1 == Entity.Null)
                {
                    ecb.DestroyEntity(spawner);
                    continue;
                }

                var speed = SystemAPI.GetComponent<ParticleSpeed>(spawnData.Prefab1).Speed;
                var lifetime = SystemAPI.GetComponent<ParticleTimer>(spawnData.Prefab1).RemainingTime;

                var speed2 = SystemAPI.GetComponent<ParticleSpeed>(spawnData.Prefab2).Speed;
                var lifetime2 = SystemAPI.GetComponent<ParticleTimer>(spawnData.Prefab2).RemainingTime;

                var entities = new NativeArray<Entity>(spawnData.SpawnDirections.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                ecb.Instantiate(spawnData.Prefab1, entities.GetSubArray(0, entities.Length / 2));
                ecb.Instantiate(spawnData.Prefab2, entities.GetSubArray(entities.Length / 2, entities.Length - entities.Length / 2));
                foreach (var entity in entities)
                {
                    ecb.SetComponent(entity, particlePosition);
                }

                foreach (var entity in entities)
                {
                    ecb.SetComponent(entity, localTransform);
                }

                var indexes = new NativeArray<int>(entities.Length, Allocator.Temp);
                for (var i = 0; i < indexes.Length; i++)
                {
                    indexes[i] = (i & 1) * (entities.Length / 2) + i / 2;
                }

                var spawnDirections = spawnData.SpawnDirections;
                var speedMultiplier = spawnData.SpeedMultiplier;
                var lifetimeMultiplier = spawnData.LifetimeMultiplier;
                for (var i = 0; i < entities.Length; i++)
                {
                    ecb.SetComponent(entities[indexes[i]], new ParticleDirection {Direction = spawnDirections[i]});
                }

                if (speedMultiplier.IsCreated)
                {
                    for (var i = 0; i < entities.Length; i++)
                    {
                        ecb.SetComponent(entities[indexes[i]], new ParticleSpeed {Speed = speedMultiplier[i] * (i % 2 == 0 ? speed : speed2)});
                    }
                }

                if (lifetimeMultiplier.IsCreated)
                {
                    for (var i = 0; i < entities.Length; i++)
                    {
                        var orgLifetime = (i % 2 == 0 ? lifetime : lifetime2);
                        var newLifetime = lifetimeMultiplier[i] * orgLifetime;
                        ecb.SetComponent(entities[indexes[i]], new ParticleTimer {RemainingTime = newLifetime});
                    }
                }

                ecb.DestroyEntity(spawner);
            }
        }

        [BurstCompile]
        [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
        public partial struct DecreaseParticleTimerJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(ref ParticleTimer timer, EnabledRefRW<ParticleTimer> isAlive)
            {
                timer.RemainingTime -= DeltaTime;
                isAlive.ValueRW = timer.RemainingTime > 0;
            }
        }
    }
}