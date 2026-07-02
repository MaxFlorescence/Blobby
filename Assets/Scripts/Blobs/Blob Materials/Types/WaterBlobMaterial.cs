public class WaterBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Watery;

    public override BlobSoundFamiliesStruct SoundFamilies => new(
        collision: SQUISH_FAMILY,
        damage:    WET_DAMAGE_SOUND,
        death:     WET_SPLAT_SOUND
    );
    
    public override AtomParticleBehaviorStruct ParticleBehavior => DROPLET_BEHAVIOR;

    public WaterBlobMaterial() : base(
        bodyMaterial: "WaterJelly",
        particleMaterial: "Water Drop",
        particleMesh: "icosahedron"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Cold_Transition, BlobMaterial.Ice)
        };
    }
}