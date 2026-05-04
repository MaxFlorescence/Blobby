using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A class defining extensions for <tt>Vector3</tt>s.
/// </summary>
public static class Vector3Extensions
{
    /// <returns>
    ///     An <tt>IEnumerable</tt> over the components of the given vector (<tt>x</tt>, then
    ///     <tt>y</tt>, then <tt>z</tt>).
    /// </returns>
    public static IEnumerable<float> GetEnumerator(this Vector3 vector)
    {
        yield return vector.x;
        yield return vector.y;
        yield return vector.z;
        yield break;
    }

    /// <returns>
    ///     An <tt>IEnumerable</tt> over the components of the given vector (<tt>x</tt>, then
    ///     <tt>y</tt>, then <tt>z</tt>), with the corresponding indices (0, then 1, then 2).
    /// </returns>
    public static IEnumerable<(int, float)> Enumerate(this Vector3 vector)
    {
        return vector.GetEnumerator().Enumerate();
    }

    /// <returns>
    ///     <tt>True</tt> iff the vector is within the given bounds (inclusive), element-wise.
    ///     <br/>
    ///     If the lower bound is <tt>null</tt>, it's treated as <tt>Vector3Int.zero</tt>.
    /// </returns>
    public static bool OutOfBounds(this Vector3 i, Vector3 upperBounds, Vector3? lowerBounds = null)
    {
        Vector3 nonNullLowerBounds = lowerBounds ?? Vector3.zero;

        return i.x.OutOfBounds(nonNullLowerBounds.x, upperBounds.x)
            || i.y.OutOfBounds(nonNullLowerBounds.y, upperBounds.y)
            || i.z.OutOfBounds(nonNullLowerBounds.z, upperBounds.z);
    }

    /// <param name="nonzero">
    ///     If <tt>true</tt>, excludes zero components of the vector.
    /// </param>
    /// <returns>
    ///     The minimum-valued component of the given vector.
    /// </returns>
    public static float Min(this Vector3 vector, bool nonzero = false)
    {
        float min = float.PositiveInfinity;

        if (!nonzero || vector.x != 0) min = Mathf.Min(min, vector.x);
        if (!nonzero || vector.y != 0) min = Mathf.Min(min, vector.y);
        if (!nonzero || vector.z != 0) min = Mathf.Min(min, vector.z);
        if (min == float.PositiveInfinity) min = 0;

        return min;
    }

    /// <param name="nonzero">
    ///     If <tt>true</tt>, excludes zero components of the vector.
    /// </param>
    /// <returns>
    ///     The maximum-valued component of the given vector.
    /// </returns>
    public static float Max(this Vector3 vector, bool nonzero = false)
    {
        return -(-vector).Min(nonzero);
    }
}