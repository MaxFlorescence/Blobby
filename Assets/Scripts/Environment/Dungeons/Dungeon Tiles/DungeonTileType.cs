using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     The different types of dungeon tiles that can exist.
/// </summary>
public enum DungeonTileType
{
    Empty,
    Hallway, Corner, Junction, Crossing, Dead_End,
    Stairs_Up, Stairs_Down,
    Entrance
}

public static class TileTypeExtensions
{
    /// <summary>
    ///     A mapping from dungeon tile types to their corresponding assets.
    /// </summary>
    private static readonly Dictionary<DungeonTileType, CorridorAssetsStruct> TileToCorridor = new()
    {
        {DungeonTileType.Dead_End,    LoadCorridorAssets("DungeonDeadEnd")},    // |_| (no wall forward)
        {DungeonTileType.Corner,      LoadCorridorAssets("DungeonCorner")},     // '_| (no wall left/forward)
        {DungeonTileType.Hallway,     LoadCorridorAssets("DungeonHallway")},    // | | (no wall back/foward)
        {DungeonTileType.Junction,    LoadCorridorAssets("DungeonJunction")},   // '_' (wall back)
        {DungeonTileType.Crossing,    LoadCorridorAssets("DungeonCrossing")},   // :: (no walls)
        {DungeonTileType.Stairs_Up,   LoadCorridorAssets("DungeonStairsUp")},   // |^| (stairs go forward)
        {DungeonTileType.Stairs_Down, LoadCorridorAssets("DungeonStairsDown")}, // |v| (stairs go back)
        {DungeonTileType.Entrance,    LoadCorridorAssets("DungeonDeadEnd")}     // TODO
    };

    /// <summary>
    ///     Loads prefabs and minimap sprites for dungeon tiles.
    /// </summary>
    /// <param name="tileName">
    ///     The tile to load the assets for.
    /// </param>
    /// <returns>
    ///     The prefab and sprite corresponding to the given tile name.
    /// </returns>
    public static CorridorAssetsStruct LoadCorridorAssets(string tileName)
    {
        return new CorridorAssetsStruct(
            prefab: Resources.Load<GameObject>(Files.DUNGEON_CORRIDORS_PATH + tileName),
            minimapIcon: Resources.Load<Sprite>(Files.MINIMAP_ICONS_PATH + tileName)
        );
    }

    /// <returns>
    ///     The prefab corresponding to this type of tile.
    /// </returns>
    public static GameObject GetPrefab(this DungeonTileType tile) {
        return TileToCorridor[tile].prefab;
    }

    /// <returns>
    ///     The minimap sprite corresponding to this type of tile.
    /// </returns>
    public static Sprite GetMapSprite(this DungeonTileType tile)
    {
        return TileToCorridor[tile].minimapIcon;
    }

    /// <returns>
    ///     The name of this type of tile.
    /// </returns>
    public static string GetName(this DungeonTileType tile)
    {
        return tile.ToString().ToLower();
    }
}