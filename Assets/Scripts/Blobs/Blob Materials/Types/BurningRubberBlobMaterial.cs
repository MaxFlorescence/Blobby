public class BurningRubberBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Solid
        | BlobMaterialProperties.Bouncy | BlobMaterialProperties.Non_Stick
        | BlobMaterialProperties.Firey;

    public override BlobSoundFamiliesStruct SoundFamilies => new(
        collision: DODGEBALL_SOUND,
        background: BLAZE_SOUND,
        toTransition: IGNITE_SOUND,
        fromTransition: EXTINGUISH_SOUND
    );
    
    public override AtomParticleBehaviorStruct ParticleBehavior => FLAME_BEHAVIOR;

    public BurningRubberBlobMaterial() : base(
        bodyMaterial:"RubberJelly",
        particleMaterial: "Flame",
        particleDirectory: FileUtilities.OBJECT_MATERIALS,
        particleMesh: "soft_cube"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (
                BlobMaterialProperties.Cold_Transition | BlobMaterialProperties.Wet_Transition,
                BlobMaterial.Rubber
            )
        };
    }
}