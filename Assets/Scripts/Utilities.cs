using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BracketType
{
    NONE, SQUARE, PARENTHESES, CURLY, ANGLE
}

public static class Rotation
{
    public static readonly Quaternion UP = Quaternion.Euler(-90, 0, 0);
    public static readonly Quaternion FORWARD = Quaternion.identity;
    public static readonly Quaternion RIGHT = Quaternion.Euler(0, 90, 0);
    public static readonly Quaternion BACK = Quaternion.Euler(0, 180, 0);
    public static readonly Quaternion LEFT = Quaternion.Euler(0, -90, 0);
    public static readonly Quaternion DOWN = Quaternion.Euler(90, 0, 0);

    public static Quaternion Parse(Vector3Int direction)
    {
        int repr = 111 + 100*direction.x + 10*direction.y + direction.z;
        return repr switch
        {
            121 => UP,
            112 => FORWARD,
            211 => RIGHT,
            110 => BACK,
            011 => LEFT,
            101 => DOWN,
            _ => throw new FormatException(direction.ToString() + " does not represent an available rotation!"),
        };
    }

    public static Quaternion ParseChar(char rot, string name)
    {
        return char.ToLower(rot) switch
        {
            'u' => UP,
            'f' => FORWARD,
            'r' => RIGHT,
            'b' => BACK,
            'l' => LEFT,
            'd' => DOWN,
            _ => throw new FormatException('\"' + name + "\" does not represent an available rotation!"),
        };
    }

    public static Quaternion Parse(string rotation)
    {
        return ParseChar(rotation[0], rotation);
    }

    public static Quaternion Parse(char rotation)
    {
        return ParseChar(rotation, rotation.ToString());
    }
}

public static class Extensions
{
    public static void AddAll<T>(this HashSet<T> addTo, IEnumerable<T> addFrom) {
        foreach (T item in addFrom)
        {
            addTo.Add(item);
        }
    }

    public static bool Contains<T>(this T[] array, T query)
    {
        foreach (T item in array)
        {
            if (item.Equals(query))
            {
                return true;
            }
        }
        return false;
    }
}

class Utilities
{
    private static Dictionary<BracketType, string> bracketMap = new()
    {
        { BracketType.NONE, "" },
        { BracketType.SQUARE, "[]" },
        { BracketType.PARENTHESES, "()" },
        { BracketType.CURLY, "{}" },
        { BracketType.ANGLE, "<>" },
    };

    public static int[] ArgShuffle<T>(T[] array)
    {
        int n = array.Length;
        int[] indices = new int[n];
        int zeroIndex = 0;

        for (int i = n-1; i > 0; i--)
        {
            int k = Random.Range(0, i-1);

            indices[i] = (indices[k] == 0) ? k+1 : indices[k];
            if (indices[i] == n) zeroIndex = i;

            indices[k] = i+1;
        }
        indices[zeroIndex] = 0;

        return indices;
    }

    public static T SelectRandom<T>(T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }

    public static readonly Vector3Int[] cardinalDirections = new Vector3Int[]
    {
        Vector3Int.up,
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left,
        Vector3Int.down
    };
    public static readonly Vector3Int[] planarDirections = new Vector3Int[]
    {
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left
    };

    public static IEnumerable<Vector3Int> RandomDirections(bool planar)
    {
        Vector3Int[] dirs = planar ? planarDirections : cardinalDirections;
        
        foreach (int i in ArgShuffle(dirs))
        {
            yield return dirs[i];
        }

        yield break;
    }

    public static string Array3DToString<T>(T[,,] array, char delimeter = ',', BracketType bracketType = BracketType.SQUARE)
    {
        string brackets = bracketMap[bracketType];
        string ret = string.Format(
            "Array of dimension ({0}, {1}, {2}): {3}",
            array.GetLength(0), array.GetLength(1), array.GetLength(2), brackets[0]
        );

        for (int i = 0; i < array.GetLength(0); i++)
        {
            if (i > 0) ret += delimeter;
            string layer = brackets[..1];
            for (int j = 0; j < array.GetLength(1); j++)
            {
                if (j > 0) layer += delimeter;
                string column = brackets[..1];
                for (int k = 0; k < array.GetLength(2); k++)
                {
                    if (k > 0) column += delimeter;
                    column += array[i, j, k].ToString();
                }
                layer += column + brackets[1];
            }
            ret += layer + brackets[1];
        }

        return ret + brackets[1];
    }
    public static string Join2D(char[,] array, string delimeter = "")
    {
        string ret = "";
        for (int i = 0; i < array.GetLength(0); i++)
        {
            if (i > 0) ret += '\n';
            for (int j = array.GetLength(1)-1; j >= 0; j--)
            {
                if (j > 0) ret += delimeter;
                ret += array[i, j];
            }
        }
        return ret;
    }
}