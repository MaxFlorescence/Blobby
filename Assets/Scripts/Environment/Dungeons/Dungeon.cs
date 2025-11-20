using UnityEngine;
using Random = UnityEngine.Random;

public class Dungeon : MonoBehaviour
{
    public TextAsset layoutFile;
    public string layoutFileName;
    public int randomSeed = 0;
    public Vector3Int randomDimMin = new(5, 5, 5);
    public Vector3Int randomDimMax = new(20, 20, 20);

    private Vector3Int dim;
    private Vector3Int entrance;
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
        Random.InitState(seed);
        dim.x = Random.Range(randomDimMin.x, randomDimMax.x + 1);
        dim.y = Random.Range(randomDimMin.y, randomDimMax.y + 1);
        dim.z = Random.Range(randomDimMin.z, randomDimMax.z + 1);
        entrance = new(Random.Range(0, dim.x), dim.y-1, Random.Range(0, dim.z));

        DungeonLayoutGenerator tree = new(dim, entrance);
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
        dim = loaded.dims;

        PopulateLayout(loaded);
    }

    private void PopulateLayout(DungeonLayoutGenerator generator)
    {
        layout = new DungeonTile[dim.x, dim.y, dim.z];

        int flatIndex = 0;
        foreach ((int x, int y, int z) in Utilities.Indices3D(dim))
        {
            Vector3Int index = new(x, y, z);

            if (generator.IsNone(flatIndex, index))
            {
                continue;
            }

            layout[x, y, z] = generator.GetTile(flatIndex, index, this);

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
}