using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
///    This class maintains and updates light objects associated with blobs.
/// </summary>
public class BlobLightController : MonoBehaviour
{
    public BlobLightStruct[] blobLightData;
    private readonly int[] mapping = new int[EnumUtilities.CountNames<BlobLight>()];

    void Awake()
    {
        for (int i = 0; i < blobLightData.Length; i++)
        {
            mapping[(int)blobLightData[i].blobLight] = i;
        }
    }

    private BlobLightStruct GetData(BlobLight id)
    {
        return blobLightData[mapping[(int)id]];
    }

    private void SetData(BlobLight id, BlobLightStruct lightData)
    {
        blobLightData[mapping[(int)id]] = lightData;
    }

    /// <summary>
    ///     Sets the state of the given blob light, optionally saving it as the light's default.
    /// </summary>
    /// <param name="blobLight">
    ///     Which blob light to modify the state of.
    /// </param>
    /// <param name="enable">
    ///     <tt>True</tt>/<tt>false</tt> enables/disables the light, respectively. <tt>null</tt> sets
    ///     the light's state to be the opposite of its default.
    /// </param>
    /// <param name="save">
    ///     <tt>True</tt> sets the light's default state to that determined by the enable parameter.
    /// </param>
    public void SetLight(BlobLight blobLight, bool? enable = null, bool save = false)
    {
        BlobLightStruct lightData = GetData(blobLight);
        enable ??= !lightData.defaultState;

        lightData.light.enabled = (bool)enable;

        if (!save) return;

        lightData.defaultState = (bool)enable;
        SetData(blobLight, lightData);
    }

    /// <summary>
    ///     Sets the state of the given blob light back to its default.
    /// </summary>
    /// <param name="blobLight">
    ///     Which blob light to modify the state of.
    /// </param>
    public void ResetLight(BlobLight blobLight)
    {
        BlobLightStruct lightData = GetData(blobLight);
        if (lightData.light.enabled == lightData.defaultState) return;

        lightData.light.enabled = lightData.defaultState;
        SetData(blobLight, lightData);
    }
}