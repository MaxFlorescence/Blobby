public class RockBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Solid
        | BlobMaterialProperties.Heavy | BlobMaterialProperties.Non_Stick
        | BlobMaterialProperties.Heat_Transition;

    public override BlobSoundDataStruct SoundData => SQUISH_SOUNDS;
        
    public override AtomParticleBehaviorStruct ParticleBehavior => DUST_BEHAVIOR;

    public RockBlobMaterial() : base(
        bodyMaterial: "Dungeon Stone",
        bodyDirectory: FileUtilities.DUNGEON_MATERIALS,
        particleMaterial: "Dust"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Heat_Transition, BlobMaterial.Lava)
        };
    }
}