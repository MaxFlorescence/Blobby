public class FerrofluidBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Slippery
        | BlobMaterialProperties.Magnetic | BlobMaterialProperties.Conductive;

    public override BlobSoundFamiliesStruct SoundFamilies => new(collision: SQUISH_FAMILY);
    
    public override AtomParticleBehaviorStruct ParticleBehavior => SPARKLE_BEHAVIOR;

    public FerrofluidBlobMaterial() : base(
        bodyMaterial: "OilJelly",
        particleMaterial: "Spike"
    ) {}
}