using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions {
    public static IEnumerable<float> GetEnumerator(this Vector3 vector)
    {
        yield return vector.x;
        yield return vector.y;
        yield return vector.z;
        yield break;
    }

    /// <returns>
    ///     <tt>True</tt> iff the vector is within the given bounds (inclusive), element-wise.
    ///     <br/>
    ///     If the lower bound is <tt>null</tt>, it's treated as <tt>Vector3Int.zero</tt>.
    /// </returns>
    public static bool OutOfBounds(this Vector3 i, Vector3 upperBounds, Vector3? lowerBounds = null)
    {
        Vector3 nonNullLowerBounds = lowerBounds ?? Vector3.zero;

        return i.x.OutOfBounds(upperBounds.x, nonNullLowerBounds.x)
            || i.y.OutOfBounds(upperBounds.y, nonNullLowerBounds.y)
            || i.z.OutOfBounds(upperBounds.z, nonNullLowerBounds.z);
    }

    public static float Min(this Vector3 vector, bool nonzero = false)
    {
        float min = float.PositiveInfinity;

        if (!nonzero || vector.x != 0) min = Mathf.Min(min, vector.x);
        if (!nonzero || vector.y != 0) min = Mathf.Min(min, vector.y);
        if (!nonzero || vector.z != 0) min = Mathf.Min(min, vector.z);
        if (min == float.PositiveInfinity) min = 0;

        return min;
    }

    public static float Max(this Vector3 vector, bool nonzero = false)
    {
        return -(-vector).Min(nonzero);
    }
}