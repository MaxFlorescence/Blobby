public class RubberBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Solid
        | BlobMaterialProperties.Bouncy | BlobMaterialProperties.Non_Stick
        | BlobMaterialProperties.Heat_Transition;

    public override BlobSoundFamiliesStruct SoundFamilies => new(collision: DODGEBALL_SOUND);
    
    public override AtomParticleBehaviorStruct ParticleBehavior => NONE_BEHAVIOR;

    public RubberBlobMaterial() : base(
        bodyMaterial:"RubberJelly"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Heat_Transition, BlobMaterial.Burning_Rubber)
        };
    }
}