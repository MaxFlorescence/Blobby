using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public enum DungeonTileType
{
    NONE,
    HALLWAY, CORNER, JUNCTION, CROSSING, DEAD_END,
    STAIRS_UP, STAIRS_DOWN,
    ENTRANCE
}

public class DungeonTile : MonoBehaviour
{
    public static Dictionary<DungeonTileType, GameObject> TileMap = new()
    {
        {DungeonTileType.DEAD_END,    LoadPrefab("DungeonDeadEnd")},    // |_| (no wall forward)
        {DungeonTileType.CORNER,      LoadPrefab("DungeonCorner")},     // '_| (no wall left/forward)
        {DungeonTileType.HALLWAY,     LoadPrefab("DungeonHallway")},    // | | (no wall back/foward)
        {DungeonTileType.JUNCTION,    LoadPrefab("DungeonJunction")},   // '_' (wall back)
        {DungeonTileType.CROSSING,    LoadPrefab("DungeonCrossing")},   // :: (no walls)
        {DungeonTileType.STAIRS_UP,   LoadPrefab("DungeonStairsUp")},   // |^| (stairs go forward)
        {DungeonTileType.STAIRS_DOWN, LoadPrefab("DungeonStairsDown")}, // |v| (stairs go back)
        {DungeonTileType.ENTRANCE,    LoadPrefab("DungeonDeadEnd")} // TODO
    };
    public static Dictionary<Vector3, Quaternion> RotationMap = new()
    {
        {Vector3.forward, Quaternion.identity},
        {Vector3.back,    Quaternion.Euler(0, 180, 0)},
        {Vector3.right,   Quaternion.Euler(0, 90, 0)},
        {Vector3.left,    Quaternion.Euler(0, -90, 0)}
    };

    public static readonly Vector3[] directions = new Vector3[]
    {
        Vector3.forward, Vector3.back,
        Vector3.right,   Vector3.left,
        Vector3.up,      Vector3.down
    };

    private static GameObject LoadPrefab(string name)
    {
        return Resources.Load("DungeonPrefabs/Corridors/" + name, typeof(GameObject)) as GameObject;
    }

    public static bool IsCardinalDirection(Vector3 direction, bool includeVertical = true)
    {
        if (!includeVertical && (direction == Vector3.up || direction == Vector3.down))
        {
            return false;
        }

        return directions.Contains(direction);
    }
    public static DungeonTile MakeTile(DungeonTileType tileType, Vector3 position, Vector3 rotation, GameObject dungeon)
    {
        Assert.IsTrue(IsCardinalDirection(rotation, false));

        GameObject tile = Instantiate(TileMap[tileType]);
        tile.transform.SetPositionAndRotation(position, RotationMap[rotation]);
        tile.transform.parent = dungeon.transform;
        return tile.AddComponent<DungeonTile>();
    }
    
    public DungeonTile forward, back, right, left, up, down;

    public void AddNeighbor(DungeonTile neighbor, Vector3 direction)
    {
        Assert.IsTrue(IsCardinalDirection(direction));
        
        if (direction == Vector3.forward)
        {
            forward = neighbor;
            neighbor.back = this;
        } else if (direction == Vector3.back)
        {
            back = neighbor;
            neighbor.forward = this;
        } else if (direction == Vector3.right)
        {
            right = neighbor;
            neighbor.left = this;
        } else if (direction == Vector3.left)
        {
            left = neighbor;
            neighbor.right = this;
        } else if (direction == Vector3.up)
        {
            up = neighbor;
            neighbor.down = this;
        } else if (direction == Vector3.down)
        {
            down = neighbor;
            neighbor.up = this;
        }
    } 
}