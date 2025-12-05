using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum DungeonTileType
{
    NONE,
    HALLWAY, CORNER, JUNCTION, CROSSING, DEAD_END,
    STAIRS_UP, STAIRS_DOWN,
    ENTRANCE
}

public static class TileExtensions
{
    private static Dictionary<DungeonTileType, GameObject> map = new()
    {
        {DungeonTileType.DEAD_END,    LoadCorridorPrefab("DungeonDeadEnd")},    // |_| (no wall forward)
        {DungeonTileType.CORNER,      LoadCorridorPrefab("DungeonCorner")},     // '_| (no wall left/forward)
        {DungeonTileType.HALLWAY,     LoadCorridorPrefab("DungeonHallway")},    // | | (no wall back/foward)
        {DungeonTileType.JUNCTION,    LoadCorridorPrefab("DungeonJunction")},   // '_' (wall back)
        {DungeonTileType.CROSSING,    LoadCorridorPrefab("DungeonCrossing")},   // :: (no walls)
        {DungeonTileType.STAIRS_UP,   LoadCorridorPrefab("DungeonStairsUp")},   // |^| (stairs go forward)
        {DungeonTileType.STAIRS_DOWN, LoadCorridorPrefab("DungeonStairsDown")}, // |v| (stairs go back)
        {DungeonTileType.ENTRANCE,    LoadCorridorPrefab("DungeonDeadEnd")} // TODO
    };
    public static GameObject LoadCorridorPrefab(string name)
    {
        return Resources.Load("DungeonPrefabs/Corridors/" + name, typeof(GameObject)) as GameObject;
    }
    public static GameObject GetPrefab(this DungeonTileType tile) {
        return map[tile];
    }
    public static string GetName(this DungeonTileType tile)
    {
        return tile.ToString().ToLower();
    }
}

public class DungeonTile : MonoBehaviour
{
    public static DungeonTile NoneTile = new();
    public static DungeonTile MakeTile(DungeonTileType tileType, Vector3 position, Quaternion rotation, GameObject dungeon)
    {
        if (tileType == DungeonTileType.NONE)
        {
            return NoneTile;
        }

        GameObject tileObject = Instantiate(tileType.GetPrefab(), position, rotation, dungeon.transform);
        DungeonTile tile = tileObject.AddComponent<DungeonTile>();
        tile.Type = tileType;
        tile.SetVisible(false);

        return tile;
    }
    
    private DungeonTile[] neighbors = new DungeonTile[6] { null, null, null, null, null, null };
    public DungeonTileType Type { get; private set; }

    private DungeonTile()
    {
        Type = DungeonTileType.NONE;
    }

    public void SetName(string name)
    {
        if (Type == DungeonTileType.NONE) return;

        this.name = name;
    }

    public void AddNeighbor(DungeonTile neighbor, Vector3Int direction)
    {
        GoAddNeighbor(neighbor, direction);
        neighbor.GoAddNeighbor(this, -direction);
    }

    private void GoAddNeighbor(DungeonTile neighbor, Vector3Int direction) {
        if (Type == DungeonTileType.NONE) return;

        if (direction == Vector3Int.forward)
        {
            neighbors[0] = neighbor;
        } else if (direction == Vector3Int.right)
        {
            neighbors[1] = neighbor;
        } else if (direction == Vector3Int.back)
        {
            neighbors[2] = neighbor;
        } else if (direction == Vector3Int.left)
        {
            neighbors[3] = neighbor;
        } else if (direction == Vector3Int.up)
        {
            neighbors[4] = neighbor;
        } else if (direction == Vector3Int.down)
        {
            neighbors[5] = neighbor;
        }

        Assert.IsTrue(Utilities.cardinalDirections.Contains(direction));
    }

    public void SetVisible(bool visible)
    {
        if (Type == DungeonTileType.NONE) return;
        int newLayer = visible ? GameInfo.DEFAULT_LAYER : GameInfo.INVISIBLE_LAYER;

        gameObject.layer = newLayer;
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = newLayer;
        }
    }
}