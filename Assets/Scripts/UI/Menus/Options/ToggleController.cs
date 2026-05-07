using UnityEngine.UI;

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