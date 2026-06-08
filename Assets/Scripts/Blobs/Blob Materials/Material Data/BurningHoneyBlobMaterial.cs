public class BurningHoneyBlobMaterial : BlobMaterialDataClass
{
    public override AtomParticleBehaviorStruct ParticleBehavior => FLAME_BEHAVIOR;

    public override BlobMaterialProperties Properties => BlobMaterialProperties.Slimy
        | BlobMaterialProperties.Firey;

    public BurningHoneyBlobMaterial() : base(
        bodyMaterial:"HoneyJelly",
        particleMaterial: "Flame",
        particleDirectory: FileUtilities.OBJECT_MATERIALS,
        particleMesh: "soft_cube"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (
                BlobMaterialProperties.Cold_Transition | BlobMaterialProperties.Wet_Transition,
                BlobMaterial.Honey
            )
        };
    }
}