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
    public readonly AtomParticleDataStruct particles;
    /// <summary>
    ///     The properties of this material.
    /// </summary>
    public readonly BlobMaterialProperties properties;

    public BlobMaterialDataStruct(BlobMaterialProperties properties, string bodyName, string dropName = null)
    {
        bodyMaterial = Resources.Load<Material>(bodyName);
        this.properties = properties;
        
        particles = (dropName ?? bodyName) switch
        {
            "Materials\\Blob Materials\\WaterJelly" => AtomParticleController.Water,
            "Materials\\Blob Materials\\LavaJelly" => AtomParticleController.Lava,
            _ => AtomParticleController.Missing,
        };
    }
}