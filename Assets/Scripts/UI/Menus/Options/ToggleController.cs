using UnityEngine.UI;

/// <summary>
///     A class for controlling an in-game toggle option's UI components.
/// </summary>
public class ToggleController : OptionController<Toggle, bool>
{
    public override bool GetDefaultValue()
    {
        return GameInfo.DefaultOptions.GetBool(optionName);
    }

    protected override bool GetSelectableValue()
    {
        return selectable.isOn;
    }

    protected override void SetSelectableValue(bool value)
    {
        selectable.isOn = value;
    }
}