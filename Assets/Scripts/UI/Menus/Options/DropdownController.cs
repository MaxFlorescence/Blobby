using UnityEngine.UI;

public class DropdownController : OptionController<Dropdown, int>
{
    public override int GetDefaultValue()
    {
        return GameInfo.DefaultOptions.GetInt(optionName);
    }

    protected override int GetSelectableValue()
    {
        return selectable.value;
    }

    protected override void SetSelectableValue(int value)
    {
        selectable.value = value;
    }
}