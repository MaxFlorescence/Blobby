using System.Collections.Generic;
using UnityEngine;

public static class Vector3IntExtensions {
    private static readonly Vector3Int[] cardinalDirections = new Vector3Int[]
    {
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down
    };
    private static readonly Vector3Int[] planarDirections = new Vector3Int[]
    {
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left
    };
    public static Vector3Int[] Directions(bool planar = false) => planar ? planarDirections : cardinalDirections;

    private static readonly Vector3Int[] cardinalAxes = new Vector3Int[]
    {
        Vector3Int.right, Vector3Int.up, Vector3Int.forward
    };
    private static readonly Vector3Int[] planarAxes = new Vector3Int[]
    {
        Vector3Int.right, Vector3Int.forward
    };
    public static Vector3Int[] Axes(bool planar = false) => planar ? planarAxes : cardinalAxes;
    
    public static IEnumerable<int> GetEnumerator(this Vector3Int vector)
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
    public static bool OutOfBounds(this Vector3Int i, Vector3Int upperBounds, Vector3Int? lowerBounds = null)
    {
        Vector3Int nonNullLowerBounds = lowerBounds ?? Vector3Int.zero;

        return i.x.OutOfBounds(upperBounds.x, nonNullLowerBounds.x)
            || i.y.OutOfBounds(upperBounds.y, nonNullLowerBounds.y)
            || i.z.OutOfBounds(upperBounds.z, nonNullLowerBounds.z);
    }

    public static int Min(this Vector3Int vector, bool nonzero = false)
    {
        return (int)((Vector3)vector).Min(nonzero);
    }

    public static int Max(this Vector3Int vector, bool nonzero = false)
    {
        return (int)((Vector3)vector).Max(nonzero);
    }

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

    public static IEnumerable<Vector2Int> Indices2D(this Vector3Int dims)
    {
        for (int x = 0; x < dims.x; x++)
        {
            for (int z = 0; z < dims.z; z++)
            {
                yield return new(x, z);
            }
        }
        yield break;
    }

    /// <summary>
    ///     Calculates the flattened index of the given position <tt>(x, y, z)</tt>.
    /// </summary>
    /// <param name="xMax">
    ///     The maximum value (+1) of the 3D indices' x component.
    /// </param>
    /// <param name="yMax">
    ///     The maximum value (+1) of the 3D indices' y component.
    /// </param>
    /// <returns>
    ///     <tt>flatIndex = z + yMax*(y + xMax*x)</tt>
    /// </returns>
    public static int ToFlatIndex(this Vector3Int indices, int xMax, int yMax)
    {
        return indices.z + yMax * (indices.y + xMax * indices.x);
    }
}