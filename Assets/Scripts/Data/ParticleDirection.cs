using Unity.Entities;
using Unity.Mathematics;

namespace Data
{
    public struct ParticleDirection : IComponentData
    {
        public float3 Direction;
    }
}