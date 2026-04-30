using System;

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