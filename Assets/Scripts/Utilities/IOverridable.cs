public interface IOverridable<T>
{
    /// <summary>
    ///     <tt>True</tt> iff this <tt>IOverridable</tt> is currently being overridden.
    /// </summary>
    public abstract bool IsOverridden { get; }

    /// <summary>
    ///     Set the value of this <tt>IOverridable</tt>. If the value is currently being overridden,
    ///     the change will occur once overriding stops.
    /// </summary>
    public void SetValue(T newValue);

    /// <summary>
    ///     Set an override for this <tt>IOverridable</tt>. All queries as to the new value will see
    ///     this override.
    /// </summary>
    public void SetOverride(T newOverride);

    /// <summary>
    ///     Clear any set override for this <tt>IOverridable</tt>. Changes made during the override
    ///     will take effect. If there was no set override, this does nothing.
    /// </summary>
    public void ClearOverride();
}