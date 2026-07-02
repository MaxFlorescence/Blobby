/// <summary>
///     A class to apply changes to float blob stats when in contact.
/// </summary>
public class FloatStatChanger : StatChanger<float>
{
    protected override void ApplyDelta(float delta)
    {
        FloatStat stat = targetBlob.Stats.GetFloat(statName);
        stat.ChangeValue(delta);

        if (respectBounds) stat.Sync();
    }
}