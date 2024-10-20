using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Data
{
    [MaterialProperty("_Color")]
    public struct MaterialColor : IComponentData
    {
        public float4 Value;
    }
}