using UnityEngine;

/// <summary>
///    This class maintains and updates light objects associated with blobs.
/// </summary>
public class BlobLightCollection : MonoBehaviour
{
    public BlobLight[] blobLights;
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

    // private BlobLight GetData(BlobLightType id)
    // {
    //     return blobLights[mapping[(int)id]];
    // }

    // private void SetData(BlobLightType id, BlobLight lightData)
    // {
    //     blobLights[mapping[(int)id]] = lightData;
    // }

    // /// <summary>
    // ///     Sets the state of the given blob light, optionally saving it as the light's default.
    // /// </summary>
    // /// <param name="blobLight">
    // ///     Which blob light to modify the state of.
    // /// </param>
    // /// <param name="enable">
    // ///     <tt>True</tt>/<tt>false</tt> enables/disables the light, respectively. <tt>null</tt> sets
    // ///     the light's state to be the opposite of its default.
    // /// </param>
    // /// <param name="save">
    // ///     <tt>True</tt> sets the light's default state to that determined by the enable parameter.
    // /// </param>
    // public void SetLight(BlobLightType blobLight, bool? enable = null, bool save = false)
    // {
    //     BlobLight lightData = this[blobLight];
    //     enable ??= !lightData.defaultState;

    //     lightData.light.enabled = (bool)enable;

    //     if (!save) return;

    //     lightData.defaultState = (bool)enable;
    //     // SetData(blobLight, lightData);
    // }

    // /// <summary>
    // ///     Sets the state of the given blob light back to its default.
    // /// </summary>
    // /// <param name="blobLight">
    // ///     Which blob light to modify the state of.
    // /// </param>
    // public void ResetLight(BlobLightType blobLight)
    // {
    //     BlobLight lightData = this[blobLight];
    //     if (lightData.light.enabled == lightData.defaultState) return;

    //     lightData.light.enabled = lightData.defaultState;
    //     // SetData(blobLight, lightData);
    // }
}