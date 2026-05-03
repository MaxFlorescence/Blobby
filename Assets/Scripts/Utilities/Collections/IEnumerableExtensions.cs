using System.Collections.Generic;

/// <summary>
///     A class defining extensions for <tt>IEnumerable</tt>s.
/// </summary>
public static class IEnumerableExtensions
{
    /// <returns>
    ///     An <tt>IEnumerable</tt> over pairs of indices and items of the given enumerable.
    ///     The indices increment starting at 0.
    /// </returns>
    public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> enumerable)
    {
        int i = 0;
        foreach (T t in enumerable)
        {
            yield return (i, t);
            i++;
        }
        yield break;
    }
}