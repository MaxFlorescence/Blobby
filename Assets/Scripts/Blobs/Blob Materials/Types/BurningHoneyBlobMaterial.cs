public class BurningHoneyBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Slimy
        | BlobMaterialProperties.Firey;

    public override BlobSoundFamiliesStruct SoundFamilies => new(
        collision: SQUISH_FAMILY,
        background: BLAZE_SOUND,
        toTransition: IGNITE_SOUND,
        fromTransition: EXTINGUISH_SOUND
    );

    public override AtomParticleBehaviorStruct ParticleBehavior => FLAME_BEHAVIOR;

    public BurningHoneyBlobMaterial() : base(
        bodyMaterial:"HoneyJelly",
        particleMaterial: "Flame",
        particleDirectory: FileUtilities.OBJECT_MATERIALS,
        particleMesh: "soft_cube"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (
                BlobMaterialProperties.Cold_Transition | BlobMaterialProperties.Wet_Transition,
                BlobMaterial.Honey
            )
        };
    }
}