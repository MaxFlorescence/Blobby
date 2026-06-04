using UnityEngine;

/// <summary>
///    This class maintains and updates light objects associated with blobs.
/// </summary>
public class BlobLightCollection : MonoBehaviour
{
    /// <summary>
    ///     The list of blob lights in this collection.
    /// </summary>
    public BlobLight[] blobLights;

    /// <summary>
    ///     The mapping of <tt>BlobLightType<\tt>s to array indices.
    /// </summary>
    private readonly int[] mapping = new int[EnumUtilities.CountNames<BlobLightType>()];

    public BlobLight this[BlobLightType lightType]
    {
        get => blobLights[mapping[(int)lightType]];
        private set => blobLights[mapping[(int)lightType]] = value;
    }

    void Awake()
    {
        for (int i = 0; i < blobLights.Length; i++)
        {
            mapping[(int)blobLights[i].blobLightType] = i;
            blobLights[i].Reset();
        }
    }
}