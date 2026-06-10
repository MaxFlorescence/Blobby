public class LavaBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Firey;

    public override BlobSoundDataStruct SoundData => SQUISH_SOUNDS;
    
    public override AtomParticleBehaviorStruct ParticleBehavior => DROPLET_BEHAVIOR;

    public LavaBlobMaterial() : base(
        bodyMaterial:"LavaJelly",
        particleMaterial: "Flame",
        particleDirectory: FileUtilities.OBJECT_MATERIALS,
        particleMesh: "icosahedron"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (
                BlobMaterialProperties.Cold_Transition | BlobMaterialProperties.Wet_Transition,
                BlobMaterial.Rock
            )
        };
    }
}