/// <summary>
///     A class to apply changes to integer blob stats when in contact.
/// </summary>
public class IntStatChanger : StatChanger<int>
{
    protected override void ApplyDelta(int delta)
    {
        IntStat stat = targetBlob.Stats.GetInt(statName);
        stat.ChangeValue(delta);

        if (respectBounds) stat.Sync();
    }
}