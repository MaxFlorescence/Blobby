public class CrystalHoneyBlobMaterial : BlobMaterialDataClass
{
    public override BlobMaterialProperties Properties => BlobMaterialProperties.Slimy
        | BlobMaterialProperties.Sweet | BlobMaterialProperties.Solid
        | BlobMaterialProperties.Heat_Transition;

    public override BlobSoundDataStruct SoundData => SQUISH_SOUNDS;

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