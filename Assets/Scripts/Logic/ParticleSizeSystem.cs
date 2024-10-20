using Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Logic
{
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
    public partial struct ParticleSizeSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ScaleParticleJob().ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct ScaleParticleJob : IJobEntity
        {
            public void Execute(in ParticleTimer particleTimer, ref LocalTransform localTransform)
            {
                var scale = math.clamp((particleTimer.RemainingTime - 1 / 60f) * 5f, 0, 1);
                localTransform.Scale = scale;
            }
        }
    }
}