using Unity.Entities;

namespace Data
{
    public struct ParticleTimer : IComponentData, IEnableableComponent
    {
        public float RemainingTime;
    }
}