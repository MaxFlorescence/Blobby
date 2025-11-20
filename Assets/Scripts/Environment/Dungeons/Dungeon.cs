using System.Collections.Generic;
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
    private const string LAYOUT_PATH = "DungeonLayouts/";

    void Start()
    {
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

    private int LayerOf(float coordinateY)
    {
        return dims.y - 1 - Mathf.CeilToInt((transform.position.y - coordinateY) / 20);
    }

    private IEnumerable<(int, int, int)> IndicesNearStairs(int level)
    {
        Vector3Int centerPos;

        if (level < dims.y-1)
        {
            centerPos = 2*stairs[level+1, 1] - stairs[level, 0] + Vector3Int.down;

            foreach ((int x, int z) in Utilities.Indices2D(new Vector3Int(3, 0, 3)))
            {
                yield return (centerPos.x + x-1, level+1 , centerPos.z + z-1);
            }
        }

        if (level > 0)
        {
            centerPos = 2*stairs[level-1, 0] - stairs[level, 1] + Vector3Int.up;

            foreach ((int x, int z) in Utilities.Indices2D(new Vector3Int(3, 0, 3)))
            {
                yield return (centerPos.x + x-1, level-1, centerPos.z + z-1);
            }
        }

        yield break;
    }

    public void UpdateActiveLevel(int level)
    {
        if (activeLevel == level) return;

        if (activeLevel >= 0) {
            foreach ((int x, int z) in Utilities.Indices2D(dims))
            {
                layout[x, activeLevel, z].SetActive(false);
            }
            foreach ((int x, int y, int z) in IndicesNearStairs(activeLevel))
            {
                try {
                    layout[x, y, z].SetActive(false);
                } catch { /* continue */ }
            }
        }

        activeLevel = level;
        foreach ((int x, int z) in Utilities.Indices2D(dims))
        {
            layout[x, activeLevel, z].SetActive(true);
        }
        foreach ((int x, int y, int z) in IndicesNearStairs(activeLevel))
        {
            try {
                layout[x, y, z].SetActive(true);
            } catch { /* continue */ }
        }
    }
}