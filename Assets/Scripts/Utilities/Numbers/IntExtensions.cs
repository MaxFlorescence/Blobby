using UnityEngine;

/// <summary>
///     A class that defines extensions for <tt>int</tt>s.
/// </summary>
public static class IntExtensions
{
    /// <returns>
    ///     <tt>0 &lt;= remainder &lt; divisor</tt> such that <tt>dividend = k * divisor + remainder</tt>.
    /// </returns>
    public static int Modulo(this int dividend, int divisor)
    {
        dividend %= divisor;
        return dividend < 0 ? dividend + divisor : dividend;
    }

    /// <summary>
    ///     Calculates the 3D index of the given flat index <tt>i</tt>
    /// </summary>
    /// <param name="xMax">
    ///     The maximum value (+1) of the 3D indices' x component.
    /// </param>
    /// <param name="yMax">
    ///     The maximum value (+1) of the 3D indices' y component.
    /// </param>
    /// <returns>
    ///     <tt>(
    ///         x = index / p,
    ///         y = (index % p) / yMax,
    ///         z = (index % p) % yMax
    ///     )</tt>
    ///     <br/> Where <tt>/</tt> indicates integer division and <tt>p = xMax * yMax</tt>.
    /// </returns>
    public static Vector3Int To3DIndex(this int index, int xMax, int yMax)
    {
        int prodMax = xMax * yMax;

        Vector3Int position = new()
        {
            x = index / prodMax
        };
        index %= prodMax;
        position.y = index / yMax;
        position.z = index % yMax;

        return position;
    }
}