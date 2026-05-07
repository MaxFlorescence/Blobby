using UnityEngine.UI;

public class ToggleController : OptionController<Toggle, bool>
{
    protected override bool GetSelectableValue()
    {
        return selectable.isOn;
    }
    protected override void SetSelectableValue(bool value)
    {
        selectable.isOn = value;
    }
}