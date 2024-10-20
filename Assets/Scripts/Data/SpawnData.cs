using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct SpawnData : ISharedComponentData
{
    public Entity Prefab1;
    public Entity Prefab2;
    public NativeArray<float3> SpawnDirections;
}
