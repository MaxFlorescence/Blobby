using UnityEngine;

/// <summary>
///     A class for generating a dungeon specified in a layout file.
/// </summary>
class LoadedDungeon : Dungeon
{
    /// <summary>
    ///     The json file to read the layout from, representing an instance of the
    ///     <tt>DungeonLayoutStructure</tt> struct.
    /// </summary>
    public TextAsset layoutFile;

    void Start()
    {
        isRandomlyGenerated = false;
        GenerateLayoutFromFile(layoutFile);
        UpdateActiveLevel(entranceTilePosition.y);
    }
}