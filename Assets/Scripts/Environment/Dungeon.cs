using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Dungeon : MonoBehaviour
{
    public TextAsset layoutFile = null;
    public string layoutFileName = null;
    public int randomSeed = 0;
    public Vector3Int randomDimMin = new(5, 5, 5);
    public Vector3Int randomDimMax = new(20, 20, 20);

    private bool generated = false;
    private int dimX, dimY, dimZ;
    private DungeonTile[,,] layout;
    private const string LAYOUT_PATH = "DungeonLayouts/";
    private static readonly Regex removeWhitespace = new(@"\s");
    private static readonly Dictionary<string, Vector3> directionMap = new()
    {
        {"forward", Vector3.forward},
        {"back", Vector3.back},
        {"right", Vector3.right},
        {"left", Vector3.left}
    };

    void Start()
    {
        // Generate layout at start if a method is provided
        if (layoutFile != null)
        {
            GenerateLayoutFromFile(layoutFile);
        } else if (layoutFileName != null)
        {
            GenerateLayoutFromFile(layoutFileName);
        } else if (randomSeed != 0)
        {
            GenerateRandomLayout(randomSeed);
        }
    }

    private void GenerateRandomLayout(int seed)
    { // TODO
        if (generated) return;
        generated = true;

        dimX = UnityEngine.Random.Range(randomDimMin.x, randomDimMax.x + 1);
        dimY = UnityEngine.Random.Range(randomDimMin.y, randomDimMax.y + 1);
        dimZ = UnityEngine.Random.Range(randomDimMin.z, randomDimMax.z + 1);

        // Place entrance tile
        layout = new DungeonTile[dimX, dimY, dimZ];
        Vector3Int index = new(dimX / 2, 0, 0);
        layout[index.x, index.y, index.z] = DungeonTile.MakeTile(
            DungeonTileType.ENTRANCE, PositionOf(index), Vector3.forward, gameObject
        );
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

        string[] dims = SplitLine(layoutFromFile[0]);
        dimX = int.Parse(dims[0]);
        dimY = int.Parse(dims[1]);
        dimZ = int.Parse(dims[2]);
        layout = new DungeonTile[dimX, dimY, dimZ];

        int line = 1;
        for (int x = 0; x < dimX; x++)
        {
            for (int y = 0; y < dimY; y++)
            {
                for (int z = 0; z < dimZ; z++)
                {
                    string[] tileInfo = SplitLine(layoutFromFile[line]);
                    if (tileInfo[0].ToLower().Equals("none"))
                    {
                        continue;
                    }

                    Vector3Int index = new(x, y, z);
                    Vector3 tilePosition = PositionOf(index);

                    string tileName = string.Format("{0}-{1}-{2}_{3}_{4}", name, tileInfo[0], x, y, z);
                    Enum.TryParse(tileInfo[0], true, out DungeonTileType tileType);
                    Vector3 tileRotation = directionMap[tileInfo[1]];

                    layout[x, y, z] = DungeonTile.MakeTile(tileType, tilePosition, tileRotation, gameObject);
                    layout[x, y, z].name = tileName;
                    foreach (Vector3 dir in DungeonTile.directions)
                    {
                        Vector3Int pos = Vector3Int.FloorToInt(dir) + index;

                        try
                        {
                            layout[x, y, z].AddNeighbor(layout[pos.x, pos.y, pos.z], dir);
                        }
                        catch { /* do nothing */ }
                    }

                    line++;
                    if (line >= layoutFromFile.Length)
                    {
                        return;
                    }
                }
            }
        }
    }

    private string[] SplitLine(string line, char delimiter = ',', char comment = '#')
    {
        line = removeWhitespace.Replace(line, "");
        line = line.Split(comment)[0];
        return line.Split(delimiter);
    }

    private Vector3 PositionOf(Vector3Int index)
    {
        return 10 * index;
    }
}