using System;

public class StatValues<T> where T : struct, IComparable
{
    public T? min;
    public T? max;
    public T? raw;

    public Action<StatValues<T>, StatValues<T>> callback = null;

    public T? Val => BoundsActive
        ? raw.Value.Clamp(min, max)
        : null;
    public bool Active => min.HasValue || max.HasValue || raw.HasValue;
    public bool BoundsActive => min.HasValue || max.HasValue;

    public StatValues(T? val = null, T? min = null, T? max = null)
    {
        this.min = min;
        this.max = max;
        raw = val;
    }

    public void Sync()
    {
        raw = Val;
    }

    public void UpdateWith(StatValues<T> other)
    {
        StatValues<T> old = callback == null ? null : Copy();

        if (other.min != null) min = other.min;
        if (other.max != null) max = other.max;
        if (other.raw != null) raw = other.raw;

        callback?.Invoke(old, this);
    }

    public StatValues<T> Copy()
    {
        return new(raw, min, max);
    }

    public override string ToString()
    {
        return $"StatValues<{typeof(T)}>(raw={raw}, min={min}, max={max}, val={Val})";
    }
}