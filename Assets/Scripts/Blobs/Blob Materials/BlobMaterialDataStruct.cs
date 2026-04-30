using UnityEngine;

/// <summary>
///     A struct defining the data for a blob material.
/// </summary>
public readonly struct BlobMaterialDataStruct
{
    /// <summary>
    ///     The material that is applied to the blob's body.
    /// </summary>
    public readonly Material bodyMaterial;
    /// <summary>
    ///     The material that is applied to the blob's droplets.
    /// </summary>
    public readonly Material dropletMaterial;
    /// <summary>
    ///     The properties of this material.
    /// </summary>
    public readonly BlobMaterialProperties properties;

    public BlobMaterialDataStruct(BlobMaterialProperties properties, string bodyName, string dropName = null)
    {
        bodyMaterial = Resources.Load<Material>(bodyName);
        dropletMaterial = (dropName == null) ? bodyMaterial : Resources.Load<Material>(dropName);
        this.properties = properties;
    }
}