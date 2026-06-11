public class AerogelBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Solid
        | BlobMaterialProperties.Light | BlobMaterialProperties.Non_Stick
        | BlobMaterialProperties.Wet_Transition;

    public override BlobSoundFamiliesStruct SoundFamilies => new(collision: DRY_CRUNCH_FAMILY);
    
    public override AtomParticleBehaviorStruct ParticleBehavior => NONE_BEHAVIOR;

    public AerogelBlobMaterial() : base(
        bodyMaterial: "AerogelJelly"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Wet_Transition, BlobMaterial.Water)
        };
    }
}