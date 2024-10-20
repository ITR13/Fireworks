using Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Logic
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct MoveParticleSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var sysTime = SystemAPI.Time;
            var elapsed = sysTime.ElapsedTime;
            var deltaTime = sysTime.DeltaTime;
            state.Dependency = new GravityJob
            {
                DeltaTime = deltaTime,
            }.ScheduleParallel(state.Dependency);
            state.Dependency = new DragJob
            {
            }.ScheduleParallel(state.Dependency);
            state.Dependency = new BuoyancyJob()
            {
                DeltaTime = deltaTime,
            }.ScheduleParallel(state.Dependency);
            state.Dependency = new MoveParticleSystemJob
            {
                DeltaTime = deltaTime,
                Time = (float)elapsed,
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct MoveParticleSystemJob : IJobEntity
        {
            public float DeltaTime;
            public float Time;

            public void Execute(in ParticleDirection particleDirection, in ParticleSpeed particleSpeed, ref ParticlePosition localTransform)
            {
                localTransform.PreviousPosition = localTransform.Position;
                localTransform.PreviousTime = localTransform.Time;
                localTransform.Position += particleDirection.Direction * particleSpeed.Speed * DeltaTime;
                localTransform.Time = Time;
            }
        }

        [BurstCompile]
        public partial struct DragJob : IJobEntity
        {
            public void Execute(in ParticleDrag particleDrag, ref ParticleSpeed particleSpeed)
            {
                particleSpeed.Speed *= particleDrag.Drag;
            }
        }

        [BurstCompile]
        public partial struct BuoyancyJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(in ParticleBuoyancy particleDrag, in ParticlePosition localTransform, ref ParticleDirection particleDirection, ref ParticleSpeed particleSpeed)
            {
                var y = math.clamp(localTransform.Position.y, -500, 0);
                var velocity = particleDirection.Direction * particleSpeed.Speed;
                velocity.y += particleDrag.Buoyancy * y / -500 * DeltaTime;
                var speed = math.length(velocity);
                particleSpeed.Speed = speed;
                particleDirection.Direction = speed > 0 ? velocity / speed : float3.zero;
            }
        }

        [BurstCompile]
        public partial struct GravityJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(in ParticleGravity particleGravity, ref ParticleDirection particleDirection, ref ParticleSpeed particleSpeed)
            {
                var velocity = particleDirection.Direction * particleSpeed.Speed;
                velocity += particleGravity.Gravity * DeltaTime;
                var speed = math.length(velocity);
                particleSpeed.Speed = speed;
                particleDirection.Direction = speed > 0 ? velocity / speed : float3.zero;
            }
        }
    }
}