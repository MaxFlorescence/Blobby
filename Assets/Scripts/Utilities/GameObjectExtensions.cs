using UnityEngine;

/// <summary>
///     A class defining extensions for <tt>GameObject</tt>s.
/// </summary>
public static class GameObjectExtensions
{
    /// <summary>
    ///     The layer mask for the "Default" layer.
    /// </summary>
    public static readonly int DEFAULT_LAYER = LayerMask.NameToLayer("Default");
    /// <summary>
    ///     The layer mask for the "Invisible" layer.
    /// </summary>
    public static readonly int INVISIBLE_LAYER = LayerMask.NameToLayer("Invisible");
    /// <summary>
    ///     The layer mask for the "InventoryUI" layer.
    /// </summary>
    public static readonly int INVENTORY_UI_LAYER = LayerMask.NameToLayer("InventoryUI");
    /// <summary>
    ///     The layer mask for the "Ignore Camera" layer.
    /// </summary>
    public static readonly int IGNORE_CAMERA_LAYER = LayerMask.NameToLayer("Ignore Camera");

    /// <summary>
    ///     Sets the layer of the given game object and all of its children to be the given layer.
    /// </summary>
    public static void SetLayer(this GameObject obj, LayerMask layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = layer;
        }
    }
}