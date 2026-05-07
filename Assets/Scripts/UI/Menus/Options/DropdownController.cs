using UnityEngine.UI;

public class DropdownController : OptionController<Dropdown, int>
{
    protected override int GetSelectableValue()
    {
        return selectable.value;
    }
    protected override void SetSelectableValue(int value)
    {
        selectable.value = value;
    }
}