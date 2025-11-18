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

    public static DungeonTile MakeTile(DungeonTileType tileType, Vector3 position, Quaternion rotation, GameObject dungeon)
    {
        GameObject tile = Instantiate(tileType.GetPrefab());
        tile.transform.SetPositionAndRotation(position, rotation);
        tile.transform.parent = dungeon.transform;
        return tile.AddComponent<DungeonTile>();
    }
    
    public DungeonTile forward, back, right, left, up, down;

    public void AddNeighbor(DungeonTile neighbor, Vector3Int direction)
    {        
        if (direction == Vector3Int.forward)
        {
            forward = neighbor;
            neighbor.back = this;
        } else if (direction == Vector3Int.back)
        {
            back = neighbor;
            neighbor.forward = this;
        } else if (direction == Vector3Int.right)
        {
            right = neighbor;
            neighbor.left = this;
        } else if (direction == Vector3Int.left)
        {
            left = neighbor;
            neighbor.right = this;
        } else if (direction == Vector3Int.up)
        {
            up = neighbor;
            neighbor.down = this;
        } else if (direction == Vector3Int.down)
        {
            down = neighbor;
            neighbor.up = this;
        }

        Assert.IsTrue(Utilities.cardinalDirections.Contains(direction));
    } 
}