using System;

/// <summary>
///     A class defining utilities for Enums.
/// </summary>
class EnumUtilities
{
    /// <returns>
    ///     The number of names defined in the given enum.
    /// </returns>
    public static int CountNames<T>() where T : Enum
    {
        return Enum.GetNames(typeof(T)).Length;
    }
}