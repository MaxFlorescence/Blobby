using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     The available game options.
/// </summary>
public enum OptionName
{
    // Slider Options
    MouseSensitivity = 0,
    EnvironmentLightIntensity = 1,
    Gamma = 2,
    Gain = 3,
    MusicVolume = 4,
    SoundEffectsVolume = 5
    // Toggle Options
    // Dropdown Options
}

/// <summary>
///     A class for controlling an in-game option's UI components.
/// </summary>
/// <typeparam name="S">
///     The type of component that changes this options value. One of
///     <tt>Slider</tt>, <tt>Toggle</tt>, or <tt>Dropdown</tt>.
/// </typeparam>
/// <typeparam name="T">
///     The type of this option's value. One of
///     <tt>float</tt>, <tt>bool</tt>, or <tt>int</tt>.
/// </typeparam>
public abstract class OptionController<S, T> : MonoBehaviour where S : Selectable
{
    /// <summary>
    ///     The struct holding the data for this option.
    /// </summary>
    public OptionStruct<T> option;
    /// <summary>
    ///     The name that corresponds to this option.
    /// </summary>
    public OptionName optionName;
    /// <summary>
    ///     The component that the player uses to change this option's value. One of
    ///     <tt>Slider</tt>, <tt>Toggle</tt>, or <tt>Dropdown</tt>.
    /// </summary>
    public S selectable;
    /// <summary>
    ///     The text box for displaying this option's name and value.
    /// </summary>
    public TextMeshProUGUI infoText;
    /// <summary>
    ///     The button for resetting edits to this option's value.
    /// </summary>
    public Button resetButton;
    /// <summary>
    ///     The button for loading the default value of this option.
    /// </summary>
    public Button loadDefaultButton;
    /// <summary>
    ///     The value that the reset button reverts this option to.
    /// </summary>
    private T storedValue;
    /// <summary>
    ///     The value that the reset button reverts this option to.
    /// </summary>
    private T defaultValue;

    void Start()
    {
        defaultValue = GetDefaultValue();
    }

    /// <summary>
    ///     The selectable UI component that the player uses to change this option's value.
    /// </summary>
    public void ChangeValueSelectable() {
        loadDefaultButton.interactable = true;
        SetOption(reset: false, TransformValue(GetSelectableValue()));
    }

    /// <summary>
    ///     The button that the player presses to reset their changes to the selectable UI component.
    /// </summary>
    public void ResetValueButton()
    {
        SetSelectableValue(InverseTransformValue(storedValue));
        SetOption(reset: true, storedValue);
    }

    /// <summary>
    ///     The button that the player presses to load the default value for this option to the
    ///     selectable UI component.
    /// </summary>
    public void LoadDefaultButton()
    {
        loadDefaultButton.interactable = false;
        SetSelectableValue(InverseTransformValue(defaultValue));
        SetOption(reset: false, defaultValue);
    }

    /// <summary>
    ///     Update the option's data and label, and enable/disable the reset button.
    /// </summary>
    /// <param name="reset">
    ///     Iff <tt>true</tt>, treat this change to the option's value as a reversible edit.
    /// </param>
    /// <param name="newValue">
    ///     The new value for this option.
    /// </param>
    protected void SetOption(bool reset, T newValue)
    {
        option.unsaved = !reset;
        option.value = newValue;

        resetButton.interactable = !reset;
        UpdateInfoText();
    }

    /// <summary>
    ///     Update the option's information display for the player.
    /// </summary>
    private void UpdateInfoText()
    {
        string valueString = FormattedValue();
        if (valueString.Length > 0) valueString = ": " + valueString;

        infoText.SetText($"{option.name}{valueString}");
    }

    /// <summary>
    ///     Initialize this option with the values of the given base option.
    /// </summary>
    public void InitializeOption(OptionStruct<T> baseOption)
    {
        storedValue = baseOption.value;
        option.name = baseOption.name;
        option.percentage = baseOption.percentage;

        ResetValueButton();
        loadDefaultButton.interactable = !storedValue.Equals(GetDefaultValue());
    }

    protected virtual void OnInitializeOption(OptionStruct<T> baseOption) {}

    /// <returns>
    ///     The default value of this option.
    /// </returns>
    public abstract T GetDefaultValue();
    
    /// <returns>
    ///     The value currently set in the selectable.
    /// </returns>
    protected abstract T GetSelectableValue();

    /// <summary>
    ///     Updates the value currently set in the selectable.
    /// </summary>
    protected abstract void SetSelectableValue(T value);

    /// <summary>
    ///     A method that applies a transformation to values obtained from this option's selectable
    ///     component before storing them as this option's value.
    /// </summary>
    /// <param name="value">
    ///     The value that the selectable component of this object is set to.
    /// </param>
    /// <returns>
    ///     A transformed version of the value, ready to store as this option's value.
    /// </returns>
    protected virtual T TransformValue(T value) {
        return value;
    }

    /// <summary>
    ///     A method that applies a transformation to this option's value before setting it to this
    ///     option's selectable component.
    /// </summary>
    /// <param name="value">
    ///     The value of this option.
    /// </param>
    /// <returns>
    ///     A transformed version of the value, ready to set to this option's selectable component.
    /// </returns>
    protected virtual T InverseTransformValue(T value) {
        return value;
    }

    /// <returns>
    ///     The value of this option formatted to be displayed to the player.
    /// </returns>
    protected virtual string FormattedValue()
    {
        return "";
    }
}