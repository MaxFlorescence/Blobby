public class CrystalHoneyBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Slimy
        | BlobMaterialProperties.Sweet | BlobMaterialProperties.Solid
        | BlobMaterialProperties.Heat_Transition;
        
    public override BlobSoundFamiliesStruct SoundFamilies => new(collision: WET_CRUNCH_FAMILY);

    public CrystalHoneyBlobMaterial() : base(
        bodyMaterial: "HoneyJelly"
    )
    {
        Transitions = new (BlobMaterialProperties, BlobMaterial)[]
        {
            (BlobMaterialProperties.Heat_Transition, BlobMaterial.Honey)
        };
    }
}