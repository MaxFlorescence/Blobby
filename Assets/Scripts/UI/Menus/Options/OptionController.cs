using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class OptionController<S, T> : MonoBehaviour where S : Selectable
{
    public OptionStruct<T> option;
    public OptionName optionName;

    public S selectable;
    public TextMeshProUGUI infoText;
    public Button resetButton;
    public Button loadDefaultButton;

    private T storedValue;

    // selectable ui component
    public void ChangeValueSelectable() {
        loadDefaultButton.interactable = true;
        SetOption(reset: false, TransformValue(GetSelectableValue()));
    }

    // reset button ui component
    public void ResetValueButton()
    {
        SetSelectableValue(InverseTransformValue(storedValue));
        SetOption(reset: true, storedValue);
    }

    protected void SetOption(bool reset, T newValue)
    {
        option.unsaved = !reset;
        resetButton.interactable = !reset;
        option.value = newValue;
        UpdateInfoText();
    }

    private void UpdateInfoText()
    {
        infoText.SetText($"{option.name}: {FormattedValue()}");
    }

    // initialize original option value for resetting
    public void InitializeOption(OptionStruct<T> newOption)
    {
        storedValue = newOption.value;
        option.name = newOption.name;
        option.format = newOption.format;

        ResetValueButton();
        if (storedValue.Equals(GetDefaultValue())) loadDefaultButton.interactable = false;
    }

    // reset value to default
    public void LoadDefaultButton()
    {
        loadDefaultButton.interactable = false;
        T defaultValue = GetDefaultValue();
        SetSelectableValue(InverseTransformValue(defaultValue));
        SetOption(reset: false, defaultValue);
    }

    public abstract T GetDefaultValue();

    protected abstract T GetSelectableValue();

    protected abstract void SetSelectableValue(T value);

    protected virtual T TransformValue(T value) {
        return value;
    }

    protected virtual T InverseTransformValue(T value) {
        return value;
    }

    protected virtual string FormattedValue() {
        string formatString = $"{{0:{option.format}}}";
        return string.Format(formatString, option.value);
    }
}