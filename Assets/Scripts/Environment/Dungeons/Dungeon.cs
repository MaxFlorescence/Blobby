using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
///     A class defining a dungeon that has been randomly generated or loaded from a file.
/// </summary>
public class Dungeon : MonoBehaviour
{
    // ---------------------------------------------------------------------------------------------
    // PARAMETERS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The actual size of each tile of the dungeon.
    /// </summary>
    private readonly Vector3Int TILE_WORLD_SIZE = new(10, 20, 10);
    protected bool isRandomlyGenerated;
    private int activeLevelIndex = -1;
    /// <summary>
    ///     The size of the dungeon.
    /// </summary>
    private Vector3Int layoutDimensions;
    /// <summary>
    ///     The position of the entrance tile.
    /// </summary>
    protected Vector3Int entranceTilePosition;
    /// <summary>
    ///     The actual grid of tiles for the dungeon.
    /// </summary>
    private DungeonTile[,,] tileLayout;
    /// <summary>
    ///     The positions of the stairs up and stairs down tiles for each dungeon level.
    /// </summary>
    private Vector3Int[,] stairPositions;
    /// <summary>
    ///     This dungeon's tile that represents empty space.
    /// </summary>
    private DungeonTile emptyTile;
    
    // ---------------------------------------------------------------------------------------------
    // OCCLUSION
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The blocker prefab is a collection of black planes that blocks the player's line of
    ///     sight up/down stairs.
    /// </summary>
    private static GameObject blockerPrefab;
    /// <summary>
    ///     A blocker prefab that blocks line of sight up stairs.
    /// </summary>
    private GameObject upperBlocker;
    /// <summary>
    ///     A blocker prefab that blocks line of sight down stairs.
    /// </summary>
    private GameObject lowerBlocker;


    void Awake()
    {
        GameInfo.CurrentDungeon = this;

        blockerPrefab = TileTypeExtensions.LoadCorridorAssets("Blocker").prefab;
        emptyTile = gameObject.AddComponent<DungeonTile>();
        
        upperBlocker = Instantiate(blockerPrefab);
        upperBlocker.transform.parent = transform;

        lowerBlocker = Instantiate(blockerPrefab);
        lowerBlocker.transform.parent = transform;
    }

    void Update()
    {
        float blobPositionY = GameInfo.ControlledBlob.GetPosition().y;
        UpdateActiveLevel(LevelOf(blobPositionY));
    }

    /// <summary>
    ///     Generates a dungeon layout with random dimensions and random tiles.
    /// </summary>
    /// <param name="minLayoutDimensions">
    ///     The minimum size of each dimension of the layout.
    /// </param>
    /// <param name="maxLayoutDimensions">
    ///     The maximum size of each dimension of the layout.
    /// </param>
    protected void GenerateRandomLayout(Vector3Int minLayoutDimensions, Vector3Int maxLayoutDimensions, int seed = 0)
    {
        Random.InitState(seed);
        layoutDimensions.x = Random.Range(minLayoutDimensions.x, maxLayoutDimensions.x + 1);
        layoutDimensions.y = Random.Range(minLayoutDimensions.y, maxLayoutDimensions.y + 1);
        layoutDimensions.z = Random.Range(minLayoutDimensions.z, maxLayoutDimensions.z + 1);
        entranceTilePosition = new(Random.Range(0, layoutDimensions.x), layoutDimensions.y-1, Random.Range(0, layoutDimensions.z));

        DungeonLayoutGenerator tree = new(layoutDimensions, entranceTilePosition, this);
        PopulateLayout(tree);
    }

    /// <summary>
    ///     Generates a dungeon layout using the contents of the specified file.
    /// </summary>
    /// <param name="layoutFile">
    ///     The file to read the dungeon layout from. See <tt>LoadedDungeon.layoutFile</tt> for an
    ///     example dungeon layout file.
    /// </param>
    protected void GenerateLayoutFromFile(TextAsset layoutFile)
    {
        DungeonLayoutGenerator loaded = new(layoutFile, this);

        entranceTilePosition = loaded.rootPosition;
        layoutDimensions = loaded.layoutDimensions;

        PopulateLayout(loaded);
    }

    /// <summary>
    ///     Fill the <tt>tileLayout</tt> array using the given layout generator.
    /// </summary>
    private void PopulateLayout(DungeonLayoutGenerator generator)
    {
        stairPositions = new Vector3Int[layoutDimensions.y, 2];
        tileLayout = new DungeonTile[layoutDimensions.x, layoutDimensions.y, layoutDimensions.z];

        foreach ((int index, Vector3Int position) in Utilities.Enumerate(Utilities.Indices3D(layoutDimensions)))
        {
            if (generator.IsEmpty(index, position))
            {
                continue;
            }

            DungeonTile tile = generator.GetTile(index, position);
            if (tile.type == DungeonTileType.Stairs_Up)
            {
                stairPositions[position.y, 0] = position;
            }
            if (tile.type == DungeonTileType.Stairs_Down)
            {
                stairPositions[position.y, 1] = position;
            }

            tileLayout[position.x, position.y, position.z] = tile;

            foreach (Vector3Int direction in Utilities.cardinalDirections)
            {
                Vector3Int neighbor = direction + position;

                try
                {
                    tileLayout[position.x, position.y, position.z].SetNeighbor(
                        tileLayout[neighbor.x, neighbor.y, neighbor.z], direction
                    );
                }
                catch { /* do nothing */ }
            }
        }
    }

    /// <returns>
    ///     The world position of the tile at the given index.
    /// </returns>
    public Vector3 PositionOf(Vector3Int index)
    {
        return TILE_WORLD_SIZE * (index - entranceTilePosition);
    }

    // TODO: this function is weird.
    /// <summary>
    ///     Transforms the given position based on the given flags.
    /// </summary>
    /// <param name="dungeonPosition">
    ///     The position to transform.
    /// </param>
    /// <param name="relativeToCenter">
    ///     If <tt>true</tt>, transform the position to be relative to the center point of the
    ///     dungeon.
    /// </param>
    /// <param name="normalize">
    ///     If <tt>true</tt>, divide the position element-wise by the dungeon layout's dimensions.
    /// </param>
    /// <param name="correctForTiles">
    ///     If <tt>true</tt>, shift the position in the positive direction (left-up-forward) by 0.5.
    /// </param>
    /// <returns>
    ///     The transformed position.
    /// </returns>
    public Vector3 TransformPosition(Vector3 dungeonPosition, bool relativeToCenter, bool normalize, bool correctForTiles)
    {
        if (correctForTiles)
        {
            dungeonPosition += 0.5f * Vector3.one;
        }
        if (relativeToCenter)
        {
            dungeonPosition -= layoutDimensions/2;
        }
        if (normalize)
        {
            dungeonPosition.x /= layoutDimensions.x;
            dungeonPosition.y /= layoutDimensions.y;
            dungeonPosition.z /= layoutDimensions.z;
        }

        return dungeonPosition;
    }

    /// <summary>
    ///     Transforms the given position to be in the dungeon's coordinate system.
    /// </summary>
    /// <param name="worldPosition">
    ///     The position in world coordinates to transform.
    /// </param>
    /// <returns>
    ///     The position transformed such that the origin is at the center of tile (0, 0, 0) and 
    ///     each tile is a unit cube.
    /// </returns>
    public Vector3 CoordinatesOf(Vector3 worldPosition)
    {
        Vector3 relativePosition = worldPosition - PositionOf(Vector3Int.zero);
        relativePosition.x /= TILE_WORLD_SIZE.x;
        relativePosition.y /= TILE_WORLD_SIZE.y;
        relativePosition.z /= TILE_WORLD_SIZE.z;

        return relativePosition;
    }

    /// <summary>
    ///     Calculates which level a given y-value falls within.
    /// </summary>
    /// <param name="coordinateY">
    ///     The y-value to test.
    /// </param>
    /// <returns>
    ///     The index of the level that the y-value is within.
    /// </returns>
    private int LevelOf(float coordinateY)
    {
        return layoutDimensions.y - 1 - Mathf.CeilToInt((transform.position.y - coordinateY) / TILE_WORLD_SIZE.y);
    }

    /// <summary>
    ///     Set the tiles that are adjacent to the stairs of the given level to be active/inactive.
    /// </summary>
    /// <param name="levelIndex">
    ///     The index of the level to start at. The tiles around the connecting stairs in the levels
    ///     directly above and below this one will be updated.
    /// </param>
    /// <param name="active">
    ///     The state to set the tiles to.
    /// </param>
    private void SetActiveNearStairs(int levelIndex, bool active)
    {
        Vector3Int centerPos, stairsDir;

        // Upper level
        if (levelIndex < layoutDimensions.y-1)
        { // TODO: block entrance
            stairsDir = stairPositions[levelIndex+1, 1] - stairPositions[levelIndex, 0] + Vector3Int.down;
            Vector3Int flip = 180*stairsDir;
            centerPos = stairPositions[levelIndex+1, 1] + stairsDir;

            foreach ((int x, int z) in Utilities.Indices2D(new Vector3Int(3, 0, 3)))
            {
                try {
                    tileLayout[centerPos.x + x-1, levelIndex+1 , centerPos.z + z-1].SetVisible(active, false);
                } catch { /* continue */ }
            }
            upperBlocker.SetActive(true);
            upperBlocker.transform.SetPositionAndRotation(
                PositionOf(centerPos) + 10*Vector3Int.up,
                Quaternion.Euler(flip) * CardinalRotation.Parse(stairsDir)
            );
        } else
        {
            upperBlocker.SetActive(false);
        }

        // Lower level
        if (levelIndex > 0)
        {
            stairsDir = stairPositions[levelIndex-1, 0] - stairPositions[levelIndex, 1] + Vector3Int.up;
            centerPos = stairPositions[levelIndex-1, 0] + stairsDir;

            foreach ((int x, int z) in Utilities.Indices2D(new Vector3Int(3, 0, 3)))
            {
                try {
                    tileLayout[centerPos.x + x-1, levelIndex-1, centerPos.z + z-1].SetVisible(active, false);
                } catch { /* continue */ }
            }
            lowerBlocker.SetActive(true);
            lowerBlocker.transform.SetPositionAndRotation(
                PositionOf(centerPos),
                CardinalRotation.Parse(stairsDir)
            );
        } else
        {
            lowerBlocker.SetActive(false);
        }
    }

    /// <summary>
    ///     Sets all tiles in the given level, as well as the tiles near its stairs, to be active.
    ///     Also deactivates the corresponding tiles from the last active level.
    /// </summary>
    /// <param name="levelIndex">
    ///     The index of the level to activate.
    /// </param>
    public void UpdateActiveLevel(int levelIndex)
    {
        if (activeLevelIndex == levelIndex) return;
        levelIndex = Utilities.Clamp(levelIndex, 0, layoutDimensions.y - 1);

        if (activeLevelIndex >= 0) {
            foreach ((int x, int z) in Utilities.Indices2D(layoutDimensions))
            {
                tileLayout[x, activeLevelIndex, z].SetVisible(false);
            }
            SetActiveNearStairs(activeLevelIndex, false);
        }

        activeLevelIndex = levelIndex;
        foreach ((int x, int z) in Utilities.Indices2D(layoutDimensions))
        {
            tileLayout[x, activeLevelIndex, z].SetVisible(true);
        }
        SetActiveNearStairs(activeLevelIndex, true);
    }

    public DungeonTile GetEmptyTile()
    {
        return emptyTile;
    }
}