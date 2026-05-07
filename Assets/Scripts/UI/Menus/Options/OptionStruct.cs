using System;

/// <summary>
///     A struct for holding game settings.
/// </summary>
[Serializable]
public struct OptionStruct<T>
{
    public string name;
    public T value;
    public bool unsaved;
    public string format;
}