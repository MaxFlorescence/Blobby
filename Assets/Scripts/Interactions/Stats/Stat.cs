using System;

public abstract class Stat<T> : IOverridable<StatValues<T>> where T : struct, IComparable
{
    protected StatValues<T> state = new();
    private StatValues<T> overrideState = new();
    public bool IsOverridden => overrideState.Active;

    public StatValues<T> State => IsOverridden ? overrideState : state;
    public T Min => State.min.Value;
    public T Max => State.max.Value;
    public T Raw => State.raw.Value;
    public T Val => State.Val.Value;

    public void Sync() => State.Sync();

    public abstract float Proportion { get; }

    public void SetCallback(Action<StatValues<T>, StatValues<T>, StatValues<T>> callback = null)
    {
        state.callback = callback;
    }

    public Stat(T? val = null, T? min = null, T? max = null)
    {
        SetValue(new(val, min, max));
    }

    /// <summary>
    ///     Change the value of this IOverridable by a given delta. If the value is currently being
    ///     overridden, the change will apply to the override.
    /// </summary>
    public abstract void ChangeValue(StatValues<T> delta);

    /// <summary>
    ///     Change the value of this IOverridable by a given delta. If the value is currently being
    ///     overridden, the change will apply to the override.
    /// </summary>
    public void ChangeValue(T val, T? min = null, T? max = null)
    {
        ChangeValue(new(val, min, max));
    }

    /// <summary>
    ///     Set the value of this IOverridable. If the value is currently being overridden, the
    ///     change will apply to the override.
    /// </summary>
    public void SetValue(StatValues<T> newState)
    {
        State.UpdateWith(newState);
    }

    /// <summary>
    ///     Set the value of this IOverridable. If the value is currently being overridden, the
    ///     change will apply to the override.
    /// </summary>
    public void SetValue(T val, T? min = null, T? max = null)
    {
        SetValue(new(val, min, max));
    }

    public void SetOverride(StatValues<T> newOverride)
    {
        overrideState = state.Copy();
        overrideState.UpdateWith(newOverride);
    }

    /// <summary>
    ///     Set an override for this IOverridable. All queries as to the new value will see this override.
    /// </summary>
    public void SetOverride(T val, T? min = null, T? max = null)
    {
        SetOverride(new(val, min, max));
    }

    public void ClearOverride()
    {
        overrideState = new();
    }
}