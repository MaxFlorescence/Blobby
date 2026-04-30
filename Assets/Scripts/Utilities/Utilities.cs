using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;
using IEnumerator = System.Collections.IEnumerator;
using System.IO;
using Unity.Mathematics;

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

    public static T Clone<T>(this T cloneMe) where T : struct
    {
        return cloneMe;
    }

    public static bool Approx(this float actual, float expected, float epsilon = 1e-3f)
    {
        return math.abs(actual - expected) < epsilon;
    }

    public static int? RoundIfClose(this float number, float epsilon = 1e-3f)
    {
        int rounded = Mathf.RoundToInt(number);
        return number.Approx(rounded, epsilon) ? rounded : null;
    }

    public static IEnumerable<T> Shuffled<T>(this T[] array)
    {
        foreach (int i in Utilities.ArgShuffle(array))
        {
            yield return array[i];
        }

        yield break;
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
    public static readonly string DEFAULT_DATA_PATH = "Default Data/";

    public static int DEFAULT_LAYER { get; private set; }
    public static int INVISIBLE_LAYER { get; private set; }
    public static int INVENTORY_UI_LAYER { get; private set; }
    public static int IGNORE_CAMERA_LAYER { get; private set; }


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

    public static IEnumerable<(int, T)> Enumerate<T>(IEnumerable<T> enumerable)
    {
        int i = 0;
        foreach (T t in enumerable)
        {
            yield return (i, t);
            i++;
        }
        yield break;
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

    public static T LoadPersistentOrDefaultData<T>(string filename)
    {
        string dataPath = Path.Combine(Application.persistentDataPath, $"{filename}.json");

        string dataString;
        try
        {
            dataString = File.ReadAllText(dataPath);
        }
        catch (FileNotFoundException)
        {
            dataString = Resources.Load<TextAsset>(DEFAULT_DATA_PATH + filename).text;
        }

        return JsonUtility.FromJson<T>(dataString);
    }

    public static void SavePersistentData<T>(T data, string filename)
    {
        string dataPath = Path.Combine(Application.persistentDataPath, $"{filename}.json");
        string dataString = JsonUtility.ToJson(data);
        File.WriteAllText(dataPath, dataString);
    }

    public static int CountNames<T>() where T : Enum
    {
        return Enum.GetNames(typeof(T)).Length;
    }
}