using UnityEngine;
using Random = UnityEngine.Random;

public class Dungeon : MonoBehaviour
{
    public TextAsset layoutFile;
    public string layoutFileName;
    public int randomSeed = 0;
    public Vector3Int randomDimMin = new(5, 5, 5);
    public Vector3Int randomDimMax = new(20, 20, 20);
    
    private int activeLevel = -1;
    private Vector3Int dims;
    private Vector3Int entrance;
    private DungeonTile[,,] layout;
    private Vector3Int[,] stairs;
    private static GameObject blockerPrefab;
    private GameObject upperBlocker;
    private GameObject lowerBlocker;
    private const string LAYOUT_PATH = "DungeonLayouts/";

    public DungeonTile NoneTile;

    void Awake()
    {
        GameInfo.CurrentDungeon = this;
    }

    void Start()
    {
        blockerPrefab = TileTypeExtensions.LoadCorridorAssets("Blocker").Item1;
        NoneTile = gameObject.AddComponent<DungeonTile>();
        
        upperBlocker = Instantiate(blockerPrefab);
        upperBlocker.transform.parent = transform;

        lowerBlocker = Instantiate(blockerPrefab);
        lowerBlocker.transform.parent = transform;

        // Generate layout at start if a method is provided
        if (randomSeed != 0)
        {
            GenerateRandomLayout(randomSeed);
        }
        else if (!layoutFile)
        {
            GenerateLayoutFromFile(layoutFileName);
        }
        else
        {
            GenerateLayoutFromFile(layoutFile);
        }

        UpdateActiveLevel(entrance.y);
    }

    void Update()
    {
        float blobPositionY = GameInfo.ControlledBlob.GetPosition().y;
        UpdateActiveLevel(LayerOf(blobPositionY));
    }

    private void GenerateRandomLayout(int seed)
    {
        Random.InitState(seed);
        dims.x = Random.Range(randomDimMin.x, randomDimMax.x + 1);
        dims.y = Random.Range(randomDimMin.y, randomDimMax.y + 1);
        dims.z = Random.Range(randomDimMin.z, randomDimMax.z + 1);
        entrance = new(Random.Range(0, dims.x), dims.y-1, Random.Range(0, dims.z));

        DungeonLayoutGenerator tree = new(dims, entrance);
        PopulateLayout(tree);
    }
    
    public void GenerateLayoutFromFile(string layoutFileName)
    {
        GenerateLayoutFromFile(
            Resources.Load(LAYOUT_PATH + layoutFileName, typeof(TextAsset)) as TextAsset
        );
    }

    private void GenerateLayoutFromFile(TextAsset layoutFile)
    {
        string[] layoutFromFile = layoutFile.text.Split("\n");
        DungeonLayoutGenerator loaded = new(layoutFromFile);
        entrance = loaded.root;
        dims = loaded.dims;

        PopulateLayout(loaded);
    }

    private void PopulateLayout(DungeonLayoutGenerator generator)
    {
        stairs = new Vector3Int[dims.y, 2];
        layout = new DungeonTile[dims.x, dims.y, dims.z];

        int flatIndex = 0;
        foreach ((int x, int y, int z) in Utilities.Indices3D(dims))
        {
            Vector3Int index = new(x, y, z);

            if (generator.IsNone(flatIndex, index))
            {
                continue;
            }

            DungeonTile tile = generator.GetTile(flatIndex, index, this);
            if (tile.Type == DungeonTileType.STAIRS_UP)
            {
                stairs[y, 0] = index;
            }
            if (tile.Type == DungeonTileType.STAIRS_DOWN)
            {
                stairs[y, 1] = index;
            }

            layout[x, y, z] = tile;

            foreach (Vector3Int dir in Utilities.cardinalDirections)
            {
                Vector3Int pos = dir + index;

                try
                {
                    layout[x, y, z].AddNeighbor(layout[pos.x, pos.y, pos.z], dir);
                }
                catch { /* do nothing */ }
            }

            flatIndex++;
            if (flatIndex >= generator.Length)
            {
                return;
            }
        }
    }

    public Vector3 PositionOf(Vector3Int index)
    {
        return new Vector3Int(10, 20, 10) * (index - entrance);
    }

    public Vector3 TransformPosition(Vector3 dungeonPosition, bool relativeToCenter, bool normalize, bool correctForTiles)
    {
        if (correctForTiles)
        {
            dungeonPosition += 0.5f * Vector3.one;
        }
        if (relativeToCenter)
        {
            dungeonPosition -= dims/2;
        }
        if (normalize)
        {
            dungeonPosition.x /= dims.x;
            dungeonPosition.y /= dims.y;
            dungeonPosition.z /= dims.z;
        }

        return dungeonPosition;
    }

    public Vector3 CoordinatesOf(Vector3 worldPosition, bool dungeonCentered = false, bool normalized = false, bool tileCentered = false)
    {
        Vector3 relativePosition = worldPosition - PositionOf(Vector3Int.zero);
        relativePosition /= 10;
        relativePosition.y /= 2;

        return relativePosition;
    }

    private int LayerOf(float coordinateY)
    {
        return dims.y - 1 - Mathf.CeilToInt((transform.position.y - coordinateY) / 20);
    }

    private void SetActiveNearStairs(int level, bool active)
    {
        Vector3Int centerPos, stairsDir;

        // Upper level
        if (level < dims.y-1)
        { // TODO: block entrance
            stairsDir = stairs[level+1, 1] - stairs[level, 0] + Vector3Int.down;
            Vector3Int flip = 180*stairsDir;
            centerPos = stairs[level+1, 1] + stairsDir;

            foreach ((int x, int z) in Utilities.Indices2D(new Vector3Int(3, 0, 3)))
            {
                try {
                    layout[centerPos.x + x-1, level+1 , centerPos.z + z-1].SetVisible(active, false);
                } catch { /* continue */ }
            }
            upperBlocker.SetActive(true);
            upperBlocker.transform.SetPositionAndRotation(
                PositionOf(centerPos) + 10*Vector3Int.up,
                Quaternion.Euler(flip) * Rotation.Parse(stairsDir)
            );
        } else
        {
            upperBlocker.SetActive(false);
        }

        // Lower level
        if (level > 0)
        {
            stairsDir = stairs[level-1, 0] - stairs[level, 1] + Vector3Int.up;
            centerPos = stairs[level-1, 0] + stairsDir;

            foreach ((int x, int z) in Utilities.Indices2D(new Vector3Int(3, 0, 3)))
            {
                try {
                    layout[centerPos.x + x-1, level-1, centerPos.z + z-1].SetVisible(active, false);
                } catch { /* continue */ }
            }
            lowerBlocker.SetActive(true);
            lowerBlocker.transform.SetPositionAndRotation(
                PositionOf(centerPos),
                Rotation.Parse(stairsDir)
            );
        } else
        {
            lowerBlocker.SetActive(false);
        }
    }

    public void UpdateActiveLevel(int level)
    {
        if (activeLevel == level) return;
        level = Utilities.Clamp(level, 0, dims.y - 1);

        if (activeLevel >= 0) {
            foreach ((int x, int z) in Utilities.Indices2D(dims))
            {
                layout[x, activeLevel, z].SetVisible(false);
            }
            SetActiveNearStairs(activeLevel, false);
        }

        activeLevel = level;
        foreach ((int x, int z) in Utilities.Indices2D(dims))
        {
            layout[x, activeLevel, z].SetVisible(true);
        }
        SetActiveNearStairs(activeLevel, true);
    }
}