using UnityEngine;

/// <summary>
///     The different types of blob lights that exist.
/// </summary>
public enum BlobLight
{
    /// <summary>
    ///     A light that illuminates inventory objects.
    /// </summary>
    Inventory_Icon = 0,
    /// <summary>
    ///     A light that corresponds to a blob material.
    /// </summary>
    Material_Glow = 1
}

/// <summary>
///    This class maintains and updates light objects associated with blobs.
/// </summary>
public class BlobLightController
{
    // ---------------------------------------------------------------------------------------------
    // DATA
    // ---------------------------------------------------------------------------------------------
    private Light[] lights;
    /// <summary>
    ///     The default state of each light type.
    /// </summary>
    private bool[] defaultStates;

    public BlobLightController()
    {
        int typeCount = EnumUtilities.CountNames<BlobLight>();
        defaultStates = new bool[typeCount];
        lights = new Light[typeCount];
    }

    /// <summary>
    ///     Add an entry for the given type of blob light.
    /// </summary>
    public void AddLight(BlobLight blobLight, Light light, bool defaultState)
    {
        defaultStates[(int)blobLight] = defaultState;
        lights[(int)blobLight] = light;
    }

    /// <summary>
    ///     Sets the state of the given blob light, optionally saving it as the light's default.
    /// </summary>
    /// <param name="blobLight">
    ///     Which blob light to modify the state of.
    /// </param>
    /// <param name="enable">
    ///     <tt>True<\tt>/<tt>false</tt> enable/disable the light, respectively. <tt>null</tt> sets
    ///     the light's state to be the opposite of its default.
    /// </param>
    /// <param name="save">
    ///     <tt>True</tt> sets the light's default state to that determined by the enable parameter.
    /// </param>
    public void SetLight(BlobLight blobLight, bool? enable, bool save = false)
    {
        int index = (int)blobLight;
        enable ??= !defaultStates[index];

        lights[index].enabled = (bool)enable;

        if (save) defaultStates[index] = (bool)enable;
    }

    /// <summary>
    ///     Sets the state of the given blob light back to its default.
    /// </summary>
    /// <param name="blobLight">
    ///     Which blob light to modify the state of.
    /// </param>
    public void ResetLight(BlobLight blobLight)
    {
        lights[(int)blobLight].enabled = defaultStates[(int)blobLight];
    }
}