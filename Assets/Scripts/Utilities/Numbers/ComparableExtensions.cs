using System;

/// <summary>
///     A class defining extensions for <tt>IComparable</tt>s.
/// </summary>
public static class ComparableExtensions
{
    /// <param name="min">
    ///     The acceptable minimum of the given value.
    /// </param>
    /// <param name="max">
    ///     The acceptable maximum of the given value.
    /// </param>
    /// <returns>
    ///     <tt>False</tt> iff the given value is within the given bounds (inclusive).
    /// </returns>
    public static bool OutOfBounds<T>(this T value, T min, T max) where T : IComparable
    {
        return value.CompareTo(min) < 0 || value.CompareTo(max) > 0;
    }
    
    /// <param name="min">
    ///     The acceptable minimum of the given value.
    /// </param>
    /// <param name="max">
    ///     The acceptable maximum of the given value.
    /// </param>
    /// <returns>
    ///     The value within the range <tt>[min, max]</tt> that is closest to the given value.
    /// </returns>
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable
    {
        if (value.CompareTo(min) <= 0) return min;
        if (value.CompareTo(max) >= 0) return max;
        return value;
    }
}