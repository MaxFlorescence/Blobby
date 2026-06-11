public class HoneyBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Slimy
        | BlobMaterialProperties.Sweet | BlobMaterialProperties.Cold_Transition
        | BlobMaterialProperties.Can_Extinguish | BlobMaterialProperties.Heat_Transition;

    public override BlobSoundFamiliesStruct SoundFamilies => new(collision: DEEP_SQUISH_FAMILY);
    
    public override AtomParticleBehaviorStruct ParticleBehavior => DROPLET_BEHAVIOR;

    public HoneyBlobMaterial() : base(
        bodyMaterial:"HoneyJelly",
        particleMesh: "icosahedron"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Heat_Transition, BlobMaterial.Burning_Honey),
            (BlobMaterialProperties.Cold_Transition, BlobMaterial.Crystal_Honey),
        };
    }
}