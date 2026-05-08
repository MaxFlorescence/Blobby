using UnityEngine.UI;

/// <summary>
///     A class for controlling an in-game slider option's UI components.
/// </summary>
public class SliderController : OptionController<Slider, float>
{
    public override float GetDefaultValue()
    {
        return GameInfo.DefaultOptions.GetFloat(optionName);
    }

    protected override float GetSelectableValue()
    {
        return selectable.value;
    }

    protected override void SetSelectableValue(float value)
    {
        selectable.value = value;
    }

    protected override float TransformValue(float value)
    {
        return value / 100;
    }

    protected override float InverseTransformValue(float value)
    {
        return value * 100;
    }

    protected override string FormattedValue()
    {
        return option.percentage ? $"{option.value:P0}" : $"{option.value:F2}";
    }
}