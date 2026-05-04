using System.IO;
using UnityEngine;

/// <summary>
///     A class defining utilities for loading data and resources.
/// </summary>
public static class Files
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
            dataString = Resources.Load<TextAsset>(DEFAULT_DATA_PATH + filename).text;
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