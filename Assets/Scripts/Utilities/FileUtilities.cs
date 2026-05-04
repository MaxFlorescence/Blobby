using System.IO;
using UnityEngine;

/// <summary>
///     A class defining utilities for loading data and resources.
/// </summary>
public static class FileUtilities
{
    public static readonly string SOUNDS            = "Sounds";
    public static readonly string MATERIALS         = "Materials";
    public static readonly string OBJECT_MATERIALS  = Path.Combine(MATERIALS, "Object Materials");
    public static readonly string DUNGEON_MATERIALS = Path.Combine(MATERIALS, "Dungeon Materials");
    public static readonly string BLOB_MATERIALS    = Path.Combine(MATERIALS, "Blob Materials");
    public static readonly string BASIC_MATERIALS   = Path.Combine(MATERIALS, "Basic Materials");
    public static readonly string MISSING_MATERIAL  = Path.Combine(BASIC_MATERIALS, "MISSING");
    public static readonly string IMAGES            = "Images";
    public static readonly string MINIMAP_ICONS     = Path.Combine(IMAGES, "Minimap Icons");
    public static readonly string DUNGEON_PREFABS   = "Dungeon Prefabs";
    public static readonly string DUNGEON_CORRIDORS = Path.Combine(DUNGEON_PREFABS, "Corridors");
    public static readonly string DEFAULT_DATA      = "Default Data";
    public static readonly string DUNGEON_LAYOUTS   = Path.Combine(DEFAULT_DATA, "Dungeon Layouts");

    /// <returns>
    ///     The data loaded from the given file in the persistent data directory, if it exists.
    ///     If it doesn't, then returns the data loaded from the corresponding file in the default
    ///     data directory.
    /// </returns>
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
            dataString = Resources.Load<TextAsset>(DEFAULT_DATA + filename).text;
        }

        return JsonUtility.FromJson<T>(dataString);
    }

    /// <summary>
    ///     Saves the given data to the given file in the persistent data directory.
    /// </summary>
    /// <param name="data">
    ///     The data to serialize into a json file.
    /// </param>
    public static void SavePersistentData<T>(T data, string filename)
    {
        string dataPath = Path.Combine(Application.persistentDataPath, $"{filename}.json");
        string dataString = JsonUtility.ToJson(data);
        File.WriteAllText(dataPath, dataString);
    }
}