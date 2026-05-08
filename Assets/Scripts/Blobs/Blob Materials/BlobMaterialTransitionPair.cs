using UnityEngine.Assertions;

/// <summary>
///     A struct defining a temperature relationship between two blob materials.
/// </summary>
public readonly struct BlobMaterialTransitionPair
{
    /// <summary>
    ///     The blob material that the colder material transitions to when exposed to heat.
    /// </summary>
    public readonly BlobMaterial fromMaterial;
    /// <summary>
    ///     The blob material that the warmer material transitions to when exposed to cold.
    /// </summary>
    public readonly BlobMaterial toMaterial;
    public readonly BlobMaterialProperties transitionProperties;

    public BlobMaterialTransitionPair(BlobMaterial fromMaterial,
                                      BlobMaterialProperties transitionProperties,
                                      BlobMaterial toMaterial)
    {
        Assert.IsTrue(fromMaterial.Has(transitionProperties), $"{fromMaterial} does not have all of [{transitionProperties}]!");
        this.fromMaterial = fromMaterial;
        this.toMaterial = toMaterial;
        this.transitionProperties = transitionProperties;
    }
}