using UnityEngine;

/// <summary>
///     A class that maps <tt>Vector3</tt> or <tt>Vector3Int</tt> directions to values of type
///     <tt>T</tt>.
/// </summary>
public class DirectionMap<T>
{
    private const int DEFAULT_INDEX = 6;
    /// <summary>
    ///     The values corresponding to each direction case. See <tt>DirectionMap.IntOf</tt> for
    ///     the index of each direction.
    /// </summary>
    private readonly T[] values;

    public DirectionMap(T upValue, T downValue, T leftValue, T rightValue, T forwardValue, T backValue, T defaultValue)
    {
        values = new T[]
        {
            leftValue, downValue, backValue, forwardValue, upValue, rightValue, defaultValue
        };
    }

    public T this[Vector3Int direction] {
        set => values[IntOf(direction)] = value;
        get => values[IntOf(direction)];
    }

    public T this[Vector3 direction] {
        set => values[IntOf(direction)] = value;
        get => values[IntOf(direction)];
    }

    /// <param name="direction">
    ///     The direction to map.
    /// </param>
    /// <returns><code>
    ///     Vector3.left    => 0
    ///     Vector3.down    => 1
    ///     Vector3.back    => 2
    ///     Vector3.forward => 3
    ///     Vector3.up      => 4
    ///     Vector3.right   => 5
    ///     otherwise       => 6
    /// </code></returns>
    private static int IntOf(Vector3 direction)
    {
        int total = 0;
        int zeros = 0;
        int units = 0;
        foreach ((int i, float c) in Utilities.Enumerate(direction.GetEnumerator()))
        {
            int? rounded = c.RoundIfClose();
            if (rounded == 0)
            {
                zeros++;
            }
            else if (rounded == 1 || rounded == -1)
            {
                units++;
            }
            else
            {
                return DEFAULT_INDEX;
            }

            if (zeros > 2 || units > 1) return DEFAULT_INDEX;

            total += (3-i) * (int)rounded;
        }

        return total + (total < 0 ? 3 : 2);
    }

    private static int IntOf(Vector3Int direction)
    {
        return IntOf((Vector3)direction);
    }
}