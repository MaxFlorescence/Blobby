public class RockBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Solid
        | BlobMaterialProperties.Heavy | BlobMaterialProperties.Non_Stick
        | BlobMaterialProperties.Heat_Transition;

    public override BlobSoundFamiliesStruct SoundFamilies => new(collision: RUMBLE_FAMILY);

    public override AtomParticleBehaviorStruct ParticleBehavior => DUST_BEHAVIOR;

    public RockBlobMaterial() : base(
        bodyMaterial: "StoneJelly",
        particleMaterial: "Dust"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Heat_Transition, BlobMaterial.Lava)
        };
    }
}