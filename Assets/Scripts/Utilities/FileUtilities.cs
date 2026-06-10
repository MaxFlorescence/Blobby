using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public static readonly string DEBUG_MATERIALS   = Path.Combine(MATERIALS, "Debug Materials");
    public static readonly string MISSING_MATERIAL  = Path.Combine(DEBUG_MATERIALS, "MISSING");
    public static readonly string CUSTOM_OBJECTS    = "Custom Objects";
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
            dataString = Resources.Load<TextAsset>(Path.Combine(DEFAULT_DATA, filename)).text;
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

    /// <param name="path">
    ///     The path of the directory to search.
    /// </param>
    /// <param name="pattern">
    ///     The pattern that enumerated files' names must match.
    /// </param>
    /// <param name="withExtension">
    ///     Iff <tt>true</tt>, leave the enumerated file's extensions appended to their names.
    /// </param>
    /// <returns>
    ///     An enumerable over the names of all files in the given directory.
    /// </returns>
    public static IEnumerable<string> GetFiles(string path, string pattern = "*",
                                               bool withExtension = false)
    {
        foreach (string fileName in Directory.EnumerateFiles(
            Path.Combine("Assets", "Resources", path), pattern
        ).Select(
            file => withExtension ? Path.GetFileName(file):  Path.GetFileNameWithoutExtension(file)
        ))
        {
            yield return fileName;
        }
    }

    /// <param name="path">
    ///     The path of the directory to search.
    /// </param>
    /// <param name="pattern">
    ///     The pattern that enumerated files' names must match.
    /// </param>
    /// <param name="withExtension">
    ///     Iff <tt>true</tt>, leave the enumerated file's extensions appended to their names.
    /// </param>
    /// <returns>
    ///     An enumerable over the (<tt>fileName</tt>, <tt>resourcePath</tt>) pairs of all files in
    ///     the given directory, where <br/><tt>resourcePath == Path.Combine(path, fileName)</tt>.
    /// </returns>
    public static IEnumerable<(string, string)> GetFilesAndResources(string path,
                                                                     string pattern = "*",
                                                                     bool withExtension = false)
    {
        foreach (string fileName in GetFiles(path, pattern, withExtension))
        {
            yield return (fileName, Path.Combine(path, fileName));
        }
    }
}