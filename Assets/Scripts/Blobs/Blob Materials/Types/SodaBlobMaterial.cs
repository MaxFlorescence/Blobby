public class SodaBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Watery
        | BlobMaterialProperties.Sweet | BlobMaterialProperties.Light;

    public override BlobSoundFamiliesStruct SoundFamilies => new(
        collision: HIGH_SQUISH_FAMILY,
        background: FIZZING_SOUND
    );
    
    public override AtomParticleBehaviorStruct ParticleBehavior => SPARKLE_BEHAVIOR;

    public SodaBlobMaterial() : base(
        bodyMaterial: "SodaJelly",
        particleMaterial: "Bubble"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Cold_Transition, BlobMaterial.Frozen_Soda)
        };
    }
}