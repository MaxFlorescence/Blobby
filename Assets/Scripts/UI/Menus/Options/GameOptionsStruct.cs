using System;

/// <summary>
///     A struct for holding game settings.
/// </summary>
[Serializable]
public struct GameOptionsStruct
{
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

    /// <param name="optionName">
    ///     The name of the slider option to get.
    /// </param>
    /// <returns>
    ///     The value of the slider option corresponding to the given option name.
    /// </returns>
    public readonly float GetFloat(OptionName optionName) {
        return SliderOptions[(int)optionName].value;
    }
    
    /// <param name="optionName">
    ///     The name of the toggle option to get.
    /// </param>
    /// <returns>
    ///     The value of the toggle option corresponding to the given option name.
    /// </returns>
    public readonly bool GetBool(OptionName optionName) {
        return ToggleOptions[(int)optionName].value;
    }

    /// <param name="optionName">
    ///     The name of the dropdown option to get.
    /// </param>
    /// <returns>
    ///     The value of the dropdown option corresponding to the given option name.
    /// </returns>
    public readonly int GetInt(OptionName optionName) {
        return DropdownOptions[(int)optionName].value;
    }
}