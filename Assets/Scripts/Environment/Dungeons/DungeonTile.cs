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
    private static GameObject LoadCorridorPrefab(string name)
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

        GameObject tileObject = Instantiate(tileType.GetPrefab());
        tileObject.transform.SetPositionAndRotation(position, rotation);
        tileObject.transform.parent = dungeon.transform;

        DungeonTile tile = tileObject.AddComponent<DungeonTile>();
        tile.Type = tileType;
        tileObject.SetActive(false);

        return tile;
    }
    
    public DungeonTile forward, back, right, left, up, down;
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
            forward = neighbor;
        } else if (direction == Vector3Int.back)
        {
            back = neighbor;
        } else if (direction == Vector3Int.right)
        {
            right = neighbor;
        } else if (direction == Vector3Int.left)
        {
            left = neighbor;
        } else if (direction == Vector3Int.up)
        {
            up = neighbor;
        } else if (direction == Vector3Int.down)
        {
            down = neighbor;
        }

        Assert.IsTrue(Utilities.cardinalDirections.Contains(direction));
    }

    public void SetActive(bool active)
    {
        if (Type == DungeonTileType.NONE) return;
        
        gameObject.SetActive(active);
    }
}