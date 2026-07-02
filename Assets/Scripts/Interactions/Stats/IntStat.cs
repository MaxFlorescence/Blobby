public class IntStat : Stat<int>
{
    public override float Proportion => (float)(Val - Min) / (Max - Min);

    public IntStat(int? val = null, int? min = null, int? max = null) : base(val, min, max) {}

    public override void ChangeValue(StatValues<int> delta)
    {
        State.UpdateWith(new(
            delta.raw.HasValue && State.raw.HasValue ? delta.raw.Value + State.raw.Value : null,
            delta.min.HasValue && State.min.HasValue ? delta.min.Value + State.min.Value : null,
            delta.max.HasValue && State.max.HasValue ? delta.max.Value + State.max.Value : null
        ));
    }

    public override string ToString()
    {
        return $"{State.Val} in [{State.min}, {State.max}] ({State.raw})";
    }
}