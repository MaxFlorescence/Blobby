using System;

/// <summary>
///     A struct for holding game settings.
/// </summary>
[Serializable]
public struct OptionsStruct
{
    public float mouseSensitivity;
    public float environmentLightIntensity;
    public float gamma;
    public float gain;
}