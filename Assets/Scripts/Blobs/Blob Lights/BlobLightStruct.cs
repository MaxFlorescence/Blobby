using System;
using UnityEngine;

/// <summary>
///     The different types of blob lights that exist.
/// </summary>
public enum BlobLight
{
    /// <summary>
    ///     A light that illuminates inventory objects.
    /// </summary>
    Inventory_Icon,
    /// <summary>
    ///     A light that corresponds to a blob material.
    /// </summary>
    Material_Glow
}

[Serializable]
public struct BlobLightStruct
{
    public BlobLight blobLight;
    public Light light;
    public bool defaultState;

    public BlobLightStruct(BlobLight blobLight, Light light, bool defaultState)
    {
        this.blobLight = blobLight;
        this.light = light;
        this.defaultState = defaultState;
    }
}