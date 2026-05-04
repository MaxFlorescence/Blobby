using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A class defining extensions for <tt>Vector3Int</tt>s.
/// </summary>
public static class Vector3IntExtensions
{
    /// <summary>
    ///     An array of the standard 3D basis vectors and their negatives.
    /// </summary>
    private static readonly Vector3Int[] cardinalDirections = new Vector3Int[]
    {
        Vector3Int.right,
        Vector3Int.up,
        Vector3Int.forward,
        Vector3Int.left,
        Vector3Int.down,
        Vector3Int.back
    };

    /// <summary>
    ///     An array of the standard 2D basis vectors and their negatives, as a subspace of 3D.
    /// </summary>
    private static readonly Vector3Int[] planarDirections = new Vector3Int[]
    {
        Vector3Int.right,
        Vector3Int.forward,
        Vector3Int.left,
        Vector3Int.back
    };

    /// <summary>
    ///     An array of standard basis vectors and their negatives:
    ///     <code>
    ///     {
    ///         Vector3Int.right,
    ///         Vector3Int.up,      // iff planar is true
    ///         Vector3Int.forward,
    ///         Vector3Int.left,
    ///         Vector3Int.down,    // iff planar is true
    ///         Vector3Int.back
    ///     }
    ///     </code>
    /// </summary>
    public static Vector3Int[] Directions(bool planar = false) => planar ? planarDirections : cardinalDirections;

    /// <summary>
    ///     An array of the standard 3D basis vectors.
    /// </summary>
    private static readonly Vector3Int[] cardinalAxes = new Vector3Int[]
    {
        Vector3Int.right, Vector3Int.up, Vector3Int.forward
    };
    
    /// <summary>
    ///     An array of the standard 2D basis vectors, as a subspace of 3D.
    /// </summary>
    private static readonly Vector3Int[] planarAxes = new Vector3Int[]
    {
        Vector3Int.right, Vector3Int.forward
    };

    /// <summary>
    ///     An array of standard basis vectors:
    ///     <code>
    ///     {
    ///         Vector3Int.right,
    ///         Vector3Int.up,      // iff planar is true
    ///         Vector3Int.forward
    ///     }
    ///     </code>
    /// </summary>
    public static Vector3Int[] Axes(bool planar = false) => planar ? planarAxes : cardinalAxes;

    /// <returns>
    ///     An <tt>IEnumerable</tt> over the components of the given vector (<tt>x</tt>, then
    ///     <tt>y</tt>, then <tt>z</tt>).
    /// </returns>
    public static IEnumerable<int> GetEnumerator(this Vector3Int vector)
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
    public static IEnumerable<(int, int)> Enumerate(this Vector3Int vector)
    {
        return vector.GetEnumerator().Enumerate();
    }

    /// <returns>
    ///     <tt>True</tt> iff the vector is within the given bounds (inclusive), element-wise.
    ///     <br/>
    ///     If the lower bound is <tt>null</tt>, it's treated as <tt>Vector3Int.zero</tt>.
    /// </returns>
    public static bool OutOfBounds(this Vector3Int i, Vector3Int upperBounds, Vector3Int? lowerBounds = null)
    {
        Vector3Int nonNullLowerBounds = lowerBounds ?? Vector3Int.zero;

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
    public static int Min(this Vector3Int vector, bool nonzero = false)
    {
        return (int)((Vector3)vector).Min(nonzero);
    }

    /// <param name="nonzero">
    ///     If <tt>true</tt>, excludes zero components of the vector.
    /// </param>
    /// <returns>
    ///     The maximum-valued component of the given vector.
    /// </returns>
    public static int Max(this Vector3Int vector, bool nonzero = false)
    {
        return (int)((Vector3)vector).Max(nonzero);
    }

    /// <param name="dims">
    ///     The size of each dimension of the enumerator.
    /// </param>
    /// <returns>
    ///     An <tt>IEnumerable</tt> over all vectors <tt>(x, y, z)</tt> such that:
    ///     <br/><tt>0 &lt;= x &lt; dims.x</tt>,
    ///     <br/><tt>0 &lt;= y &lt; dims.y</tt>,
    ///     <br/><tt>0 &lt;= z &lt; dims.z</tt>.
    ///     <br/>
    ///     Incrementing <tt>z</tt> first, then <tt>y</tt>, then <tt>x</tt>.
    /// </returns>
    public static IEnumerable<Vector3Int> Indices3D(this Vector3Int dims)
    {
        for (int x = 0; x < dims.x; x++)
        {
            for (int y = 0; y < dims.y; y++)
            {
                for (int z = 0; z < dims.z; z++)
                {
                    yield return new(x, y, z);
                }
            }
        }
        yield break;
    }

    /// <param name="dims">
    ///     The size of each dimension of the enumerator.
    /// </param>
    /// <returns>
    ///     An <tt>IEnumerable</tt> over all vectors <tt>(x, z)</tt> such that:
    ///     <br/><tt>0 &lt;= x &lt; dims.x</tt>,
    ///     <br/><tt>0 &lt;= z &lt; dims.z</tt>.
    ///     <br/>
    ///     Incrementing <tt>z</tt> first, then <tt>x</tt>.
    /// </returns>
    public static IEnumerable<Vector3Int> Indices2D(this Vector3Int dims)
    {
        for (int x = 0; x < dims.x; x++)
        {
            for (int z = 0; z < dims.z; z++)
            {
                yield return new(x, 0, z);
            }
        }
        yield break;
    }

    /// <param name="xDim">
    ///     The size of the indices' x dimension.
    /// </param>
    /// <param name="yDim">
    ///     The size of the 3D indices' y dimension.
    /// </param>
    /// <returns>
    ///     <tt>flatIndex = z + yMax*(y + xMax*x)</tt>
    /// </returns>
    public static int ToFlatIndex(this Vector3Int indices, int xDim, int yDim)
    {
        return indices.z + yDim * (indices.y + xDim * indices.x);
    }
}