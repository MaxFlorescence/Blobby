using UnityEngine;

/// <summary>
///     A struct for holding corridor asset data.
/// </summary>
public readonly struct CorridorAssetsStruct
{
    /// <summary>
    ///     The gameobject prefab for this corridor.
    /// </summary>
    public readonly GameObject prefab;
    /// <summary>
    ///     The sprite that displays on the minimap for this corridor.
    /// </summary>
    public readonly Sprite minimapIcon;

    public CorridorAssetsStruct(GameObject prefab, Sprite minimapIcon)
    {
        this.prefab = prefab;
        this.minimapIcon = minimapIcon;
    }
}