using UnityEngine.Assertions;

/// <summary>
///     A struct defining a temperature relationship between two blob materials.
/// </summary>
public readonly struct BlobMaterialTemperaturePair
{
    /// <summary>
    ///     The blob material that the colder material transitions to when exposed to heat.
    /// </summary>
    public readonly BlobMaterial warmerMaterial;
    /// <summary>
    ///     The blob material that the warmer material transitions to when exposed to cold.
    /// </summary>
    public readonly BlobMaterial colderMaterial;

    public BlobMaterialTemperaturePair(BlobMaterial warmerMaterial, BlobMaterial colderMaterial)
    {
        Assert.IsTrue(warmerMaterial.HasProperty(BlobMaterialProperties.Transitions_With_Cold));
        this.warmerMaterial = warmerMaterial;
        
        Assert.IsTrue(colderMaterial.HasProperty(BlobMaterialProperties.Transitions_With_Heat));
        this.colderMaterial = colderMaterial;
    }
}