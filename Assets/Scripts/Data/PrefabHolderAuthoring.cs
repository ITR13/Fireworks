using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PrefabHolderAuthoring : MonoBehaviour
{
    public GameObject BasicParticle;

    public class PrefabHolderBaker : Baker<PrefabHolderAuthoring>
    {
        public override void Bake(PrefabHolderAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var basicParticle = GetEntity(authoring.BasicParticle, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PrefabHolder {BasicParticle = basicParticle});
        }
    }
}