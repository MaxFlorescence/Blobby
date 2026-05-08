using UnityEngine.UI;

/// <summary>
///     A class for controlling an in-game dropdown option's UI components.
/// </summary>
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