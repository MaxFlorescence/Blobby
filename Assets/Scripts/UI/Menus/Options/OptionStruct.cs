using System;

/// <summary>
///     A struct for holding game settings.
/// </summary>
[Serializable]
public struct OptionStruct<T>
{
    /// <summary>
    ///     The display name of the option.
    /// </summary>
    public string name;
    /// <summary>
    ///     The value that the option is set to.
    /// </summary>
    public T value;
    /// <summary>
    ///     <tt>True</tt> iff the option's current value is unsaved.
    /// </summary>
    public bool unsaved;
    /// <summary>
    ///     <tt>True</tt> iff the option's value should be displayed as a percentage.
    /// </summary>
    public bool percentage;
}