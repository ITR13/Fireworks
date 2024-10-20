using Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Logic
{
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
    public partial struct ParticlePositionSystem : ISystem, ISystemStartStop
    {
        private float _fixedDeltaTime;


        public void OnStartRunning(ref SystemState state)
        {
            var fixedSimulationSystemGroup = state.World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
            if (fixedSimulationSystemGroup.RateManager is RateUtils.FixedRateCatchUpManager rateManager)
            {
                _fixedDeltaTime = rateManager.Timestep;
            }
            else
            {
                _fixedDeltaTime = 1 / 60f;
                Debug.LogError("Failed to get rate manager");
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var sysTime = SystemAPI.Time;
            state.Dependency = new MoveParticleJob
            {
                Time = (float)sysTime.ElapsedTime - _fixedDeltaTime,
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct MoveParticleJob : IJobEntity
        {
            public float Time;

            public void Execute(in ParticlePosition particlePosition, ref LocalTransform localTransform)
            {
                localTransform.Position = math.lerp(
                    particlePosition.PreviousPosition,
                    particlePosition.Position,
                    math.unlerp(particlePosition.PreviousTime, particlePosition.Time, Time)
                );
            }
        }
    }
}