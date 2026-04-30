using System;
using UnityEngine;

/// <summary>
///     A struct for holding the information about a dungeon.
/// </summary>
[Serializable]
public struct DungeonLayoutStruct
{
    /// <summary>
    ///     The name of the dungeon.
    /// </summary>
    public string dungeonName;
    /// <summary>
    ///     The size of each of the dungeon's dimensions.
    /// </summary>
    public Vector3Int layoutDimensions;
    /// <summary>
    ///     The position of the dungeon's root/entrance.
    /// </summary>
    public Vector3Int rootPosition;
    /// <summary>
    ///     The list of tiles that comprise the dungeon.
    /// </summary>
    public DungeonTileStruct[] layout;
}