public class OilBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Heat_Transition
        | BlobMaterialProperties.Cold_Transition | BlobMaterialProperties.Slippery;

    public override BlobSoundFamiliesStruct SoundFamilies => new(collision: DEEP_SQUISH_FAMILY);
    
    public override AtomParticleBehaviorStruct ParticleBehavior => DROPLET_BEHAVIOR;

    public OilBlobMaterial() : base(
        bodyMaterial:"OilJelly",
        particleMesh: "icosahedron"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Heat_Transition, BlobMaterial.Burning_Oil),
            (BlobMaterialProperties.Cold_Transition, BlobMaterial.Rubber)
        };
    }
}