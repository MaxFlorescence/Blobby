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

/// <summary>
///     A struct for holding information about an entity in a dungeon.
/// </summary>
[Serializable]
public struct DungeonEntityStruct
{
    /// <summary>
    ///     The type of entity.
    /// </summary>
    public string entityType;
    /// <summary>
    ///     The name of the entity.
    /// </summary>
    public string entityName;
    /// <summary>
    ///     The position of the entity within its tile.
    /// </summary>
    public string entityPosition;
    /// <summary>
    ///     The orientation of the entity.
    /// </summary>
    public string entityOrientation;
}