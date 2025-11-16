using UnityEngine;
using Random = UnityEngine.Random;

public class Dungeon : MonoBehaviour
{
    public TextAsset layoutFile;
    public string layoutFileName;
    public int randomSeed = 0;
    public Vector3Int randomDimMin = new(5, 5, 5);
    public Vector3Int randomDimMax = new(20, 20, 20);

    private bool generated = false;
    private Vector3Int dim;
    private DungeonTile[,,] layout;
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
    }

    private void GenerateRandomLayout(int seed)
    {
        if (generated) return;
        generated = true;

        Random.InitState(seed);
        dim.x = Random.Range(randomDimMin.x, randomDimMax.x + 1);
        dim.y = Random.Range(randomDimMin.y, randomDimMax.y + 1);
        dim.z = Random.Range(randomDimMin.z, randomDimMax.z + 1);
        Vector3Int entrance = new(Random.Range(0, dim.x), dim.y-1, Random.Range(0, dim.z));

        DungeonLayoutGenerator tree = new(dim, entrance);
        PopulateLayout(tree);
    }
    
    public void GenerateLayoutFromFile(string layoutFileName)
    {
        if (generated) return;

        GenerateLayoutFromFile(
            Resources.Load(LAYOUT_PATH + layoutFileName, typeof(TextAsset)) as TextAsset
        );
    }

    private void GenerateLayoutFromFile(TextAsset layoutFile)
    {
        if (generated) return;
        generated = true;

        string[] layoutFromFile = layoutFile.text.Split("\n");
        DungeonLayoutGenerator loaded = new(layoutFromFile);
        dim = loaded.dims;

        PopulateLayout(loaded);
    }

    private void PopulateLayout(DungeonLayoutGenerator generator)
    {
        layout = new DungeonTile[dim.x, dim.y, dim.z];

        int flatIndex = 0;
        for (int x = 0; x < dim.x; x++)
        {
            for (int y = 0; y < dim.y; y++)
            {
                for (int z = 0; z < dim.z; z++)
                {
                    Vector3Int index = new(x, y, z);

                    if (generator.IsNone(flatIndex, index))
                    {
                        continue;
                    }

                    Vector3 tilePosition = PositionOf(index);

                    (string tileName, DungeonTileType tileType, Vector3 tileRotation) = generator.GetTile(flatIndex, index);

                    layout[x, y, z] = DungeonTile.MakeTile(tileType, tilePosition, tileRotation, gameObject);
                    layout[x, y, z].name = name + "-" + tileName;
                    foreach (Vector3 dir in DungeonTile.directions)
                    {
                        Vector3Int pos = Vector3Int.FloorToInt(dir) + index;

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
        }
    }

    private Vector3 PositionOf(Vector3Int index)
    {
        return 10 * index;
    }
}