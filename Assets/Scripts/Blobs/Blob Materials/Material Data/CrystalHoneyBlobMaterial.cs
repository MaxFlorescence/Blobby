public class CrystalHoneyBlobMaterial : BlobMaterialDataClass
{
    public override AtomParticleBehaviorStruct ParticleBehavior => NONE_BEHAVIOR;
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Slimy
        | BlobMaterialProperties.Sweet | BlobMaterialProperties.Solid
        | BlobMaterialProperties.Heat_Transition;

    public CrystalHoneyBlobMaterial() : base(
        bodyMaterial: "HoneyJelly"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Heat_Transition, BlobMaterial.Honey)
        };
    }
}