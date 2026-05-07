using System;

/// <summary>
///     A struct for holding game settings.
/// </summary>
[Serializable]
public struct GameOptionsStruct
{
    // public float mouseSensitivity;
    // public float environmentLightIntensity;
    // public float gamma;
    // public float gain;
    public OptionStruct<float>[] SliderOptions;
    public OptionStruct<bool>[] ToggleOptions;
    public OptionStruct<int>[] DropdownOptions;

    public GameOptionsStruct(OptionStruct<float>[] sliderOptions,
                             OptionStruct<bool>[] toggleOptions,
                             OptionStruct<int>[] dropdownOptions)
    {
        SliderOptions = sliderOptions;
        ToggleOptions = toggleOptions;
        DropdownOptions = dropdownOptions;
    }

    public readonly float GetFloat(OptionName optionName) {
        return SliderOptions[(int)optionName].value;
    }
    public readonly bool GetBool(OptionName optionName) {
        return ToggleOptions[(int)optionName].value;
    }
    public readonly int GetInt(OptionName optionName) {
        return DropdownOptions[(int)optionName].value;
    }
}