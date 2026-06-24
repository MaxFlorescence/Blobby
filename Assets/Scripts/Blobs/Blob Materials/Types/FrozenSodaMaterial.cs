public class FrozenSodaBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Icy
        | BlobMaterialProperties.Light;

    public override BlobSoundFamiliesStruct SoundFamilies => new(
        collision: CLINK_CRUNCH_FAMILY
    );
    
    public override AtomParticleBehaviorStruct ParticleBehavior => FLUTTER_BEHAVIOR;

    public FrozenSodaBlobMaterial() : base(
        bodyMaterial: "FrozenSodaJelly",
        particleMaterial: "Snowflake"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Heat_Transition, BlobMaterial.Soda)
        };
    }
}