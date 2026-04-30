using System;

/// <summary>
///     A struct for holding the information about a dungeon tile.
/// </summary>
[Serializable]
public struct DungeonTileStruct
{
    /// <summary>
    ///     The type of dungeon tile.
    /// </summary>
    public string tileType;
    /// <summary>
    ///     The orientation of the dungeon tile.
    /// </summary>
    public string tileOrientation;
    public DungeonEntityStruct[] contents;
}