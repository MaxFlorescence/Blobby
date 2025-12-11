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

public static class TileTypeExtensions
{
    private static Dictionary<DungeonTileType, (GameObject, Sprite)> map = new()
    {
        {DungeonTileType.DEAD_END,    LoadCorridorAssets("DungeonDeadEnd")},    // |_| (no wall forward)
        {DungeonTileType.CORNER,      LoadCorridorAssets("DungeonCorner")},     // '_| (no wall left/forward)
        {DungeonTileType.HALLWAY,     LoadCorridorAssets("DungeonHallway")},    // | | (no wall back/foward)
        {DungeonTileType.JUNCTION,    LoadCorridorAssets("DungeonJunction")},   // '_' (wall back)
        {DungeonTileType.CROSSING,    LoadCorridorAssets("DungeonCrossing")},   // :: (no walls)
        {DungeonTileType.STAIRS_UP,   LoadCorridorAssets("DungeonStairsUp")},   // |^| (stairs go forward)
        {DungeonTileType.STAIRS_DOWN, LoadCorridorAssets("DungeonStairsDown")}, // |v| (stairs go back)
        {DungeonTileType.ENTRANCE,    LoadCorridorAssets("DungeonDeadEnd")} // TODO
    };
    public static (GameObject, Sprite) LoadCorridorAssets(string name)
    {
        GameObject prefab = Resources.Load("DungeonPrefabs/Corridors/" + name, typeof(GameObject)) as GameObject;
        Sprite sprite = Resources.Load("Images/Minimap Icons/" + name, typeof(Sprite)) as Sprite;

        return (prefab, sprite);
    }
    public static GameObject GetPrefab(this DungeonTileType tile) {
        return map[tile].Item1;
    }
    public static Sprite GetMapSprite(this DungeonTileType tile)
    {
        return map[tile].Item2;
    }
    public static string GetName(this DungeonTileType tile)
    {
        return tile.ToString().ToLower();
    }
}

public class DungeonTile : MonoBehaviour
{
    public static DungeonTile MakeTile(string name, DungeonTileType tileType, Vector3Int position, Quaternion tileRotation, Dungeon dungeon)
    {
        if (tileType == DungeonTileType.NONE)
        {
            return dungeon.GetComponent<Dungeon>().NoneTile;
        }

        Vector3 tilePosition = dungeon.PositionOf(position);

        GameObject tileObject = Instantiate(tileType.GetPrefab(), tilePosition, tileRotation, dungeon.transform);
        DungeonTile tile = tileObject.AddComponent<DungeonTile>();

        tile.mapIcon = GameInfo.ActiveMiniMap.AddIcon(
            name + "-Minimap_Icon",
            tileType.GetMapSprite(),
            dungeon.TransformPosition(position, true, true, true),
            -tileRotation.eulerAngles.y,
            0.333f // why?
        );

        tile.Type = tileType;
        tile.SetVisible(false);

        return tile;
    }
    
    private DungeonTile[] neighbors = new DungeonTile[6] { null, null, null, null, null, null };
    public DungeonTileType Type { get; private set; } = DungeonTileType.NONE;
    private GameObject mapIcon;

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

    public void SetVisible(bool visible, bool? visibleOnMap = null)
    {
        if (Type == DungeonTileType.NONE) return;
        visibleOnMap ??= visible;
        
        int newLayer = visible ? Utilities.DEFAULT_LAYER : Utilities.INVISIBLE_LAYER;

        gameObject.layer = newLayer;
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = newLayer;
        }

        mapIcon.SetActive((bool)visibleOnMap);
    }
}