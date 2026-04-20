using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using IEnumerator = System.Collections.IEnumerator;

public static class Rotation
{
    public static readonly Quaternion Up = Quaternion.Euler(-90, 0, 0);
    public static readonly Quaternion Forward = Quaternion.identity;
    public static readonly Quaternion Right = Quaternion.Euler(0, 90, 0);
    public static readonly Quaternion Back = Quaternion.Euler(0, 180, 0);
    public static readonly Quaternion Left = Quaternion.Euler(0, -90, 0);
    public static readonly Quaternion Down = Quaternion.Euler(90, 0, 0);
    private static readonly Quaternion[] Orientations =
    {
        Left, Down, Back, Forward, Up, Right
    };

    public static Quaternion Parse(Vector3Int direction)
    {
        return Orientations[Utilities.IntOfDirection(direction)];
    }

    public static Quaternion Parse(string rotation)
    {
        return rotation.ToLower() switch
        {
            "up" => Up,
            "forward" => Forward,
            "right" => Right,
            "back" => Back,
            "left" => Left,
            "down" => Down,
            _ => throw new FormatException($"\"{rotation}\" does not represent an available rotation!"),
        };
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

    private static readonly Regex removeWhitespace = new(@"\s");
    public static string RemoveWhitespace(this string s)
    {
        return removeWhitespace.Replace(s, "");
    }

    public static float Min(this Vector3 vector, bool nonzero = false)
    {
        float min = float.PositiveInfinity;

        if (!nonzero || vector.x > 0) min = Mathf.Min(min, vector.x);
        if (!nonzero || vector.y > 0) min = Mathf.Min(min, vector.y);
        if (!nonzero || vector.z > 0) min = Mathf.Min(min, vector.z);
        if (min == float.PositiveInfinity) min = 0;

        return min;
    }

    public static void SetLayer(this GameObject obj, LayerMask layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = layer;
        }
    }

    /// <returns>
    ///     <tt>True</tt> iff the float is within the given bounds (inclusive).
    /// </returns>
    public static bool OutOfBounds(this float i, float upperBound, float lowerBound = 0)
    {
        return i < lowerBound || upperBound < i;
    }

    /// <returns>
    ///     <tt>True</tt> iff the integer is within the given bounds (inclusive).
    /// </returns>
    public static bool OutOfBounds(this int i, int upperBound, int lowerBound = 0)
    {
        return i < lowerBound || upperBound < i;
    }

    /// <returns>
    ///     <tt>True</tt> iff the vector is within the given bounds (inclusive), element-wise.
    ///     <br/>
    ///     If the lower bound is <tt>null</tt>, it's treated as <tt>Vector3Int.zero</tt>.
    /// </returns>
    public static bool OutOfBounds(this Vector3Int i, Vector3Int upperBounds, Vector3Int? lowerBounds = null)
    {
        Vector3Int nonNullLowerBounds = (lowerBounds == null) ? Vector3Int.zero : (Vector3Int)lowerBounds;

        return i.x.OutOfBounds(upperBounds.x, nonNullLowerBounds.x)
            || i.y.OutOfBounds(upperBounds.y, nonNullLowerBounds.y)
            || i.z.OutOfBounds(upperBounds.z, nonNullLowerBounds.z);
    }

    /// <typeparam name="T">
    ///     The type of element in the array.
    /// </typeparam>
    /// <param name="data">
    ///     The array itself.
    /// </param>
    /// <param name="i">
    ///     The integer index of the desired element.
    /// </param>
    /// <returns>
    ///     The item of the array that corresponds to the given index modulo the array's length.
    /// </returns>
    public static T ModularGet<T>(this T[] data, int i)
    {
        return data[Utilities.ModularIndex(i, data.Length)];
    }

    /// <summary>
    ///     Sets the element of the array at the given index modulo the array's length to be the
    ///     given value.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of element in the array.
    /// </typeparam>
    /// <param name="data">
    ///     The array itself.
    /// </param>
    /// <param name="i">
    ///     The integer index of the desired element.
    /// </param>
    public static void ModularSet<T>(this T[] data, int i, T value)
    {
        data[Utilities.ModularIndex(i, data.Length)] = value;
    }

    /// <summary>
    ///     Plays an AudioClip, and scales the AudioSource pitch randomly between the given bounds.
    /// </summary>
    /// <param name="audioClip">
    ///     The clip to play.
    /// </param>
    /// <param name="pitchBounds">
    ///     The minimum and maximum pitches that the AudioClip can play at.
    /// </param>
    public static void PlayRandomPitchOneShot(this AudioSource audioSource, AudioClip audioClip, Vector2? pitchBounds = null)
    {
        if (audioClip == null) return;
        
        float originalPitch = audioSource.pitch;
        audioSource.pitch = Random.Range(pitchBounds?.x ?? 1, pitchBounds?.y ?? 1);

        audioSource.PlayOneShot(audioClip);
        
        audioSource.pitch = originalPitch;
    }

    public static void DelayedExecute(this MonoBehaviour monoBehaviour, float delay, Action action)
    {
        if (delay > 0)
        {
            monoBehaviour.StartCoroutine(DelayedExecuteHelper(monoBehaviour, delay, action));
        }
        else
        {
            action.Invoke();
        }
    }

    private static IEnumerator DelayedExecuteHelper(MonoBehaviour monoBehaviour, float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        monoBehaviour.DelayedExecute(0, action);
    }
}

class Utilities : MonoBehaviour
{
    public static readonly string SOUNDS_PATH = "Sounds/";
    public static readonly string BLOB_MATERIALS_PATH = "Materials/Blob Materials/";
    public static readonly string BASIC_MATERIALS_PATH = "Materials/Basic Materials/";
    public static readonly string MISSING_MATERIAL_PATH = BASIC_MATERIALS_PATH + "MISSING";
    public static readonly string OBJECT_MATERIALS_PATH = "Materials/Object Materials/";
    public static readonly string DUNGEON_MATERIALS_PATH = "Materials/Dungeon Materials/";
    public static readonly string MINIMAP_ICONS_PATH = "Images/Minimap Icons/";
    public static readonly string DUNGEON_CORRIDORS_PATH = "Dungeon Prefabs/Corridors/";
    public static readonly string DUNGEON_LAYOUTS_PATH = "Dungeon Layouts/";

    public static int DEFAULT_LAYER { get; private set; }
    public static int INVISIBLE_LAYER { get; private set; }
    public static int INVENTORY_UI_LAYER { get; private set; }
    public static int IGNORE_CAMERA_LAYER { get; private set; }

    public static readonly Vector3Int[] cardinalDirections = new Vector3Int[]
    {
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down
    };
    public static readonly Vector3Int[] planarDirections = new Vector3Int[]
    {
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left
    };

    public static readonly Vector3Int[] cardinalAxes = new Vector3Int[]
    {
        Vector3Int.right, Vector3Int.up, Vector3Int.forward
    };

    public static readonly Vector3Int[] planarAxes = new Vector3Int[]
    {
        Vector3Int.right, Vector3Int.forward
    };

    void Awake()
    {
        DEFAULT_LAYER = LayerMask.NameToLayer("Default");
        INVISIBLE_LAYER = LayerMask.NameToLayer("Invisible");
        INVENTORY_UI_LAYER = LayerMask.NameToLayer("InventoryUI");
        IGNORE_CAMERA_LAYER = LayerMask.NameToLayer("Ignore Camera");
    }
    
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
        if (n > 0) {
            indices[zeroIndex] = 0;
        }

        return indices;
    }

    public static T SelectRandom<T>(T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }

    public static T Clamp<T>(T n, T min, T max) where T : IComparable
    {
        if (n.CompareTo(min) < 0)
        {
            return min;
        } else if (n.CompareTo(max) > 0)
        {
            return max;
        }

        return n;
    }

    public static IEnumerable<Vector3Int> RandomDirections(bool planar)
    {
        Vector3Int[] dirs = planar ? planarDirections : cardinalDirections;
        
        foreach (int i in ArgShuffle(dirs))
        {
            yield return dirs[i];
        }

        yield break;
    }

    public static IEnumerable<(int, Vector3Int)> EnumerateIndices3D(Vector3Int dims)
    {
        int i = 0;
        for (int x = 0; x < dims.x; x++)
        {
            for (int y = 0; y < dims.y; y++)
            {
                for (int z = 0; z < dims.z; z++)
                {
                    yield return (i, new(x, y, z));
                    i++;
                }
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
    public static int IndexFlatOf(Vector3Int indices, int xMax, int yMax)
    {
        return indices.z + yMax * (indices.y + xMax * indices.x);
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
    public static Vector3Int Index3dOf(int index, int xMax, int yMax)
    {
        int prodMax = xMax * yMax;

        Vector3Int position = new();
        position.x = index / prodMax;
        index %= prodMax;
        position.y = index / yMax;
        position.z = index % yMax;

        return position;
    }

    public static IEnumerable<(int, int)> Indices2D(Vector3Int dims)
    {
        for (int x = 0; x < dims.x; x++)
        {
            for (int z = 0; z < dims.z; z++)
            {
                yield return (x, z);
            }
        }
        yield break;
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
    /// </code></returns>
    public static int IntOfDirection(Vector3Int direction)
    {
        Assert.IsTrue(cardinalDirections.Contains(direction));

        int ret = 3*direction.x + 2*direction.y + direction.z;
        return ret + (ret < 0 ? 3 : 2);
    }

    /// <param name="i"></param>
    /// <param name="m"></param>
    /// <returns>
    ///     <tt>j = k * m + i</tt>, where <tt>k</tt> is an integer such that
    ///     <tt>0 &lt;= j &lt; m</tt>.
    /// </returns>
    public static int ModularIndex(int i, int m)
    {
        i %= m;
        return i < 0 ? i + m : i;
    }
}