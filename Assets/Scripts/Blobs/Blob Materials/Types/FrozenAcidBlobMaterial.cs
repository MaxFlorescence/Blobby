public class FrozenAcidBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Icy;

    public override BlobSoundFamiliesStruct SoundFamilies => new(
        collision: CLINK_CRUNCH_FAMILY
    );
    
    public override AtomParticleBehaviorStruct ParticleBehavior => FLUTTER_BEHAVIOR;

    public FrozenAcidBlobMaterial() : base(
        bodyMaterial:"FrozenAcidJelly",
        particleMaterial: "Snowflake"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Heat_Transition, BlobMaterial.Acid)
        };
    }
}