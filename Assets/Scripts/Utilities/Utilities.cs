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

    /// <returns>
    ///     <tt>0 &lt;= remainder &lt; divisor</tt> such that <tt>dividend = k * divisor + remainder</tt>.
    /// </returns>
    public static int Modulo(this int dividend, int divisor)
    {
        dividend %= divisor;
        return dividend < 0 ? dividend + divisor : dividend;
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