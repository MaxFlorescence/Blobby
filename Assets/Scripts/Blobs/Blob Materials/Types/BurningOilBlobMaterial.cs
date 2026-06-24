public class BurningOilBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Cold_Transition
        | BlobMaterialProperties.Slippery | BlobMaterialProperties.Glowing
        | BlobMaterialProperties.Can_Ignite;

    public override BlobSoundFamiliesStruct SoundFamilies => new(
        collision: DEEP_SQUISH_FAMILY,
        background: BLAZE_SOUND,
        toTransition: IGNITE_SOUND,
        fromTransition: EXTINGUISH_SOUND
    );
    
    public override AtomParticleBehaviorStruct ParticleBehavior => FLAME_BEHAVIOR;

    public BurningOilBlobMaterial() : base(
        bodyMaterial: "OilJelly",
        particleMaterial: "Flame",
        particleDirectory: FileUtilities.OBJECT_MATERIALS,
        particleMesh: "soft_cube"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Cold_Transition, BlobMaterial.Rubber)
        };
    }
}