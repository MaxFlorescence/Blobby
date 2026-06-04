using System;
using UnityEngine;

/// <summary>
///     The different types of blob lights that exist.
/// </summary>
public enum BlobLightType
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
public class BlobLight : IOverridable<bool>
{
    public BlobLightType blobLightType;
    public Light light;
    public bool defaultState;
    private bool overrideActive = false;
    public bool IsOverridden => overrideActive;

    public BlobLight(BlobLightType blobLightType, Light light, bool defaultState)
    {
        this.blobLightType = blobLightType;
        this.light = light;
        this.defaultState = defaultState;
    }

    public void Reset()
    {
        SetValue(defaultState);
    }

    public void ClearOverride()
    {
        light.enabled = defaultState;
        overrideActive = false;
    }

    public void SetOverride(bool newOverride)
    {
        light.enabled = newOverride;
        overrideActive = true;
    }

    public void SetValue(bool newValue)
    {
        defaultState = newValue;
        if (!overrideActive) light.enabled = defaultState;
    }
}