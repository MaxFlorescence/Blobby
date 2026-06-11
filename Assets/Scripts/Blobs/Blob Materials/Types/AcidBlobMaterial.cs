public class AcidBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Watery
        | BlobMaterialProperties.Can_Dissolve;

    public override BlobSoundFamiliesStruct SoundFamilies => new(
        collision: SQUISH_FAMILY,
        background: BUBBLES_FIZZING_SOUND
    );
    
    public override AtomParticleBehaviorStruct ParticleBehavior => DROPLET_BEHAVIOR;

    public AcidBlobMaterial() : base(
        bodyMaterial:"AcidJelly",
        particleMesh: "icosahedron"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Cold_Transition, BlobMaterial.Frozen_Acid)
        };
    }
}