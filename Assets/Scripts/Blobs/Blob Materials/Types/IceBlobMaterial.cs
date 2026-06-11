public class IceBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Icy;

    public override BlobSoundFamiliesStruct SoundFamilies => new(collision: CLINK_CRUNCH_FAMILY);
    
    public override AtomParticleBehaviorStruct ParticleBehavior => SPARKLE_BEHAVIOR;

    public IceBlobMaterial() : base(
        bodyMaterial: "IceJelly",
        particleMaterial: "Snowflake"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Heat_Transition, BlobMaterial.Water)
        };
    }
}