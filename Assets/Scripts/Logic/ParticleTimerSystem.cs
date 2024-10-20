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

            foreach (var (spawnData, localTransform, spawner) in SystemAPI.Query<SpawnData, ParticlePosition>().WithDisabled<ParticleTimer>().WithEntityAccess())
            {
                if (spawnData.Prefab1 == Entity.Null)
                {
                    ecb.DestroyEntity(spawner);
                    continue;
                }

                var entities = new NativeArray<Entity>(spawnData.SpawnDirections.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                ecb.Instantiate(spawnData.Prefab1, entities.GetSubArray(0, entities.Length / 2));
                ecb.Instantiate(spawnData.Prefab2, entities.GetSubArray(entities.Length / 2, entities.Length - entities.Length / 2));
                foreach (Entity entity in entities)
                {
                    ecb.SetComponent(entity, localTransform);
                }

                var spawnDirections = spawnData.SpawnDirections;
                for (var i = 0; i < entities.Length; i++)
                {
                    var index = (i & 1) * (entities.Length / 2) + i / 2;
                    ecb.SetComponent(entities[index], new ParticleDirection {Direction = spawnDirections[i]});
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