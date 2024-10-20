using System.Windows.Input;
using Unity.Entities;
using Unity.Mathematics;

namespace Data
{
    public struct ParticlePosition : IComponentData
    {
        public float PreviousTime, Time;
        public float3 PreviousPosition, Position;
    }
}