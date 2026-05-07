using UnityEngine.UI;

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
}