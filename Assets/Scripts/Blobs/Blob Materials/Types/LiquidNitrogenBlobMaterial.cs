public class LiquidNitrogenBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Light
        | BlobMaterialProperties.Can_Extinguish | BlobMaterialProperties.Can_Freeze;

    public override BlobSoundFamiliesStruct SoundFamilies => new(
        collision: HIGH_SQUISH_FAMILY,
        background: WIND_SOUND
    );
    
    public override AtomParticleBehaviorStruct ParticleBehavior => FLUTTER_BEHAVIOR;

    public LiquidNitrogenBlobMaterial() : base(
        bodyMaterial:"LiquidNitrogenJelly",
        particleMaterial: "Snowflake"
    ) {}
}