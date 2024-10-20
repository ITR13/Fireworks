using Unity.Entities;
using Unity.Mathematics;

namespace Data
{
    public struct ParticleGravity : IComponentData
    {
        public float3 Gravity;
    }
}