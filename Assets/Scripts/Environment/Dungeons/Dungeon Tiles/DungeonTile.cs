using UnityEngine;

/// <summary>
///     A class defining a tile of a dungeon.
/// </summary>
public class DungeonTile : MonoBehaviour
{
    /// <summary>
    ///     Instantiates the given tile type with the given position, orientation, name.
    ///     As a child object of the given dungeon.
    /// </summary>
    /// <param name="name">
    ///     The name of the new tile object.
    /// </param>
    /// <param name="tileType">
    ///     The type of tile to instantiate.
    /// </param>
    /// <param name="position">
    ///     The dungeon position of the new tile.
    /// </param>
    /// <param name="orientation">
    ///     The orientation of the new tile.
    /// </param>
    /// <param name="dungeon">
    ///     The parent dungeon for the new tile.
    /// </param>
    /// <returns>
    ///     A reference to the <tt>DungeonTile</tt> component of the new tile.
    /// </returns>
    public static DungeonTile MakeTile(string name, DungeonTileType tileType, Vector3Int position, Quaternion orientation, Dungeon dungeon)
    {
        if (tileType == DungeonTileType.Empty)
        {
            return dungeon.GetComponent<Dungeon>().GetEmptyTile();
        }

        Vector3 tilePosition = dungeon.PositionOf(position);

        GameObject tileObject = Instantiate(tileType.GetPrefab(), tilePosition, orientation, dungeon.transform);
        DungeonTile tile = tileObject.AddComponent<DungeonTile>();

        // TODO: figure out why the example loaded dungeon's map icons are smaller than normal
        tile.mapIcon = GameInfo.ActiveMiniMap.AddIcon(
            name + "-Minimap_Icon",
            tileType.GetMapSprite(),
            dungeon.TransformPosition(position, true, true, true),
            -orientation.eulerAngles.y,
            0.333f // why?
        );

        tile.type = tileType;
        tile.SetVisible(false);

        return tile;
    }
    
    /// <summary>
    ///     References to the immediate neighbors of this tile.
    /// </summary>
    private DirectionMap<DungeonTile> neighbors = new(null, null, null, null, null, null, null);
    /// <summary>
    ///     This tile's type.
    /// </summary>
    public DungeonTileType type { get; private set; } = DungeonTileType.Empty;
    /// <summary>
    ///     The minimap icon for this tile.
    /// </summary>
    private GameObject mapIcon;

    /// <summary>
    ///     Changes the name of this tile.
    /// </summary>
    /// <param name="name">
    ///     The new tile name.
    /// </param>
    public void SetName(string name)
    {
        if (type == DungeonTileType.Empty) return;

        this.name = name;
    }

    /// <summary>
    ///     Symmetrically designates this tile to be the neighbor of the given tile, in the given
    ///     direction.
    /// </summary>
    /// <param name="neighbor">
    ///     The tile to set as this tile's neighbor, and vice versa.
    /// </param>
    /// <param name="direction">
    ///     The unit vector pointing from this tile to the given neighbor tile.
    /// </param>
    public void SetNeighbor(DungeonTile neighbor, Vector3Int direction)
    {
        GoSetNeighbor(neighbor, direction);
        neighbor.GoSetNeighbor(this, -direction);
    }

    /// <summary>
    ///     Sets this tile's neighbor in the given direction to be the given tile.
    /// </summary>
    /// <param name="neighbor">
    ///     The tile to set as this tile's neighbor.
    /// </param>
    /// <param name="direction">
    ///     The unit vector pointing from this tile to the given neighbor tile.
    /// </param>
    private void GoSetNeighbor(DungeonTile neighbor, Vector3Int direction) {
        if (type == DungeonTileType.Empty) return;

        neighbors[direction] = neighbor;
    }

    /// <param name="direction">
    ///     The unit vector pointing from this tile to a neighbor tile.
    /// </param>
    /// <returns>
    ///     The neighbor of this tile that corresponds to the given direction.
    /// </returns>
    public DungeonTile GetNeighbor(Vector3Int direction)
    {
        if (type == DungeonTileType.Empty) return null;

        return neighbors[direction];
    }

    /// <summary>
    ///     Sets this tile and all of its children to be visible or invisible depending on the
    ///     given parameters.
    /// </summary>
    /// <param name="visibleInWorld">
    ///     Iff <tt>true</tt>, sets this tile and its children to be visible in the world.
    /// </param>
    /// <param name="visibleOnMap">
    ///     If <tt>true</tt>, sets this tile and its children to be visible on the minimap.
    ///     <br/>
    ///     If <tt>false</tt>, sets this tile and its children to be invisible on the minimap.
    ///     <br/>
    ///     If <tt>null</tt>, this parameter will match the <tt>visibleInWorld</tt> parameter.
    /// </param>
    public void SetVisible(bool visibleInWorld, bool? visibleOnMap = null)
    {
        if (type == DungeonTileType.Empty) return;

        visibleOnMap ??= visibleInWorld;
        gameObject.SetLayer(visibleInWorld ? GameObjectExtensions.DEFAULT_LAYER : GameObjectExtensions.INVISIBLE_LAYER);

        mapIcon.SetActive((bool)visibleOnMap);
    }
}