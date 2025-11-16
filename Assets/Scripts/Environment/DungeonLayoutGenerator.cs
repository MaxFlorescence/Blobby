using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

class DungeonLayoutGenerator
{
    public readonly int Length;
    private static readonly Dictionary<char, Vector3> directionMap = new()
    {
        {'f', Vector3.forward},
        {'b', Vector3.back},
        {'r', Vector3.right},
        {'l', Vector3.left}
    };
    private static readonly Dictionary<Vector3Int, byte> directionMasks = new()
    {
        {Vector3Int.forward, 0b101111},
        {Vector3Int.right,   0b110111},
        {Vector3Int.back,    0b111011},
        {Vector3Int.left,    0b111101},
        {Vector3Int.up,      0b011111},
        {Vector3Int.down,    0b111110}
    };
    private static readonly Vector3Int[] planarDirections = new Vector3Int[]
    {
        Vector3Int.forward, Vector3Int.right, Vector3Int.back, Vector3Int.left
    };
    private static readonly Dictionary<byte, (string, DungeonTileType, Vector3)> wallMap = new()
    {
        // UFRBLD
        {0b000000, ("none",        DungeonTileType.NONE,        Vector3.forward)},
        {0b100001, ("crossing",    DungeonTileType.CROSSING,    Vector3.forward)},
        {0b100101, ("junction",    DungeonTileType.JUNCTION,    Vector3.forward)},
        {0b100011, ("junction",    DungeonTileType.JUNCTION,    Vector3.right)},
        {0b110001, ("junction",    DungeonTileType.JUNCTION,    Vector3.back)},
        {0b101001, ("junction",    DungeonTileType.JUNCTION,    Vector3.left)},
        {0b101011, ("hallway",     DungeonTileType.HALLWAY,     Vector3.forward)},
        {0b110101, ("hallway",     DungeonTileType.HALLWAY,     Vector3.right)},
        {0b101101, ("corner",      DungeonTileType.CORNER,      Vector3.forward)},
        {0b100111, ("corner",      DungeonTileType.CORNER,      Vector3.right)},
        {0b110011, ("corner",      DungeonTileType.CORNER,      Vector3.back)},
        {0b111001, ("corner",      DungeonTileType.CORNER,      Vector3.left)},
        {0b101111, ("dead_end",    DungeonTileType.DEAD_END,    Vector3.forward)},
        {0b110111, ("dead_end",    DungeonTileType.DEAD_END,    Vector3.right)},
        {0b111011, ("dead_end",    DungeonTileType.DEAD_END,    Vector3.back)},
        {0b111101, ("dead_end",    DungeonTileType.DEAD_END,    Vector3.left)},
        {0b011011, ("stairs_up",   DungeonTileType.STAIRS_UP,   Vector3.forward)},
        {0b011101, ("stairs_up",   DungeonTileType.STAIRS_UP,   Vector3.right)},
        {0b001111, ("stairs_up",   DungeonTileType.STAIRS_UP,   Vector3.back)},
        {0b010111, ("stairs_up",   DungeonTileType.STAIRS_UP,   Vector3.left)},
        {0b101110, ("stairs_down", DungeonTileType.STAIRS_DOWN, Vector3.forward)},
        {0b110110, ("stairs_down", DungeonTileType.STAIRS_DOWN, Vector3.right)},
        {0b111010, ("stairs_down", DungeonTileType.STAIRS_DOWN, Vector3.back)},
        {0b111100, ("stairs_down", DungeonTileType.STAIRS_DOWN, Vector3.left)}
    };
    private static readonly Regex removeWhitespace = new(@"\s");

    public readonly Vector3Int dims;
    private readonly Vector3Int root;
    private readonly string[] layoutFileLines;
    private readonly bool fromFile;
    private readonly float reconnectChance = 0.1f;

    private byte[,,] walls;

    public DungeonLayoutGenerator(Vector3Int dims, Vector3Int root)
    {
        this.dims = dims;
        this.root = root;
        Length = dims.x * dims.y * dims.z;
        fromFile = false;
        walls = new byte[dims.x, dims.y, dims.z];

        GenerateRandomLayout();
    }

    public DungeonLayoutGenerator(string[] layoutFileLines)
    {
        string[] strDims = PrepareString(layoutFileLines[0]);
        dims.x = int.Parse(strDims[0]);
        dims.y = int.Parse(strDims[1]);
        dims.z = int.Parse(strDims[2]);

        this.layoutFileLines = layoutFileLines;
        Length = layoutFileLines.Length;
        fromFile = true;
    }

    public (string, DungeonTileType, Vector3) GetTile(int index, Vector3Int position)
    {
        string tileName;
        DungeonTileType tileType;
        Vector3 tileRotation;

        if (fromFile)
        {
            (tileName, tileType, tileRotation) = GetTileFromLines(index + 1);
        }
        else
        {
            (tileName, tileType, tileRotation) = GetTileInternal(position);
        }
        
        return (tileName + "-" + string.Join("_", position), tileType, tileRotation);
    }

    private (string, DungeonTileType, Vector3) GetTileFromLines(int index)
    {
        (string tileName, char tileRot) = SplitLine(layoutFileLines[index]);

        Enum.TryParse(tileName, true, out DungeonTileType tileType);
        Vector3 tileRotation = directionMap[tileRot];
        return (tileName, tileType, tileRotation);
    }
    
    private (string, DungeonTileType, Vector3) GetTileInternal(Vector3Int position)
    {
        (string tileName, DungeonTileType tileType, Vector3 tileRotation) = wallMap[
            walls[position.x, position.y, position.z]
        ];

        if (root == position)
        {
            tileName = "entrance";
            tileType = DungeonTileType.ENTRANCE;
        }

        return (tileName, tileType, tileRotation);
    }

    public bool IsNone(int flatIndex, Vector3Int index)
    {
        if (fromFile)
        {
            string tileInfo = layoutFileLines[flatIndex].ToLower();
            return tileInfo[..4] == "none";
        }
        else
        {
            return walls[index.x, index.y, index.z] == 0;
        }
    }

    // private static Dictionary<byte, char> asciiMap = new()
    // {
    //     { 0b100001, '╋' },
    //     { 0b101001, '┣' },
    //     { 0b110001, '┻' },
    //     { 0b111001, '┗' },
    //     { 0b100011, '┫' },
    //     { 0b101011, '┃' },
    //     { 0b110011, '┛' },
    //     { 0b111011, '╹' },
    //     { 0b100101, '┳' },
    //     { 0b101101, '┏' },
    //     { 0b110101, '━' },
    //     { 0b111101, '╺' },
    //     { 0b100111, '┓' },
    //     { 0b101111, '╻' },
    //     { 0b110111, '╸' },
    //     { 0b111111, '╳' }
    // };

    private void GenerateRandomLayout()
    {
        int touchCount = 0;
// string layoutLog = "";
// char[,] repr = new char[dims.z,dims.x];
// for (int i = 0; i < dims.z; i++)
// {
//     for (int j = 0; j < dims.x; j++)
//     {
//         repr[i, j] = asciiMap[0b111111];
//     }
// }
// layoutLog += Utilities.Join2D(repr) + " " + touchCount.ToString() + "\n\n";

        Vector3Int direction = planarDirections[Random.Range(0, 3)];
        Vector3Int destination = root + direction;
        try
        { // TODO do this better
            walls[destination.x, destination.y, destination.z] = directionMasks[-direction];
            walls[root.x, root.y, root.z] = directionMasks[direction];
        }
        catch
        {
            direction *= -1;
            walls[destination.x, destination.y, destination.z] = directionMasks[-direction];
            walls[root.x, root.y, root.z] = directionMasks[direction];
        }
        touchCount += 2; // root tile and the tile directly after it are both touched
// repr[root.z, root.x] = asciiMap[walls[root.x, root.y, root.z]];
// repr[destination.z, destination.x] = asciiMap[walls[destination.x, destination.y, destination.z]];
// layoutLog += Utilities.Join2D(repr) +  " " + touchCount.ToString() + "\n\n";

        HashSet<Vector3Int> heads = new() { root + direction };
        HashSet<Vector3Int> newHeads;
        HashSet<Vector3Int> newDirs = new(3);

        while (heads.Count > 0)
        {
            newHeads = new();

            foreach (Vector3Int head in heads)
            {
                foreach (Vector3Int dir in planarDirections)
                {
                    try
                    {
                        if (walls[head.x + dir.x, head.y + dir.y, head.z + dir.z] == 0
                            || Random.Range(0f, 1f) < reconnectChance)
                        {
                            newDirs.Add(dir);
                        }
                    }
                    catch { /* do nothing */ }
                }

                int remaining = newDirs.Count;
                foreach (Vector3Int dir in newDirs)
                {
                    if (Random.Range(0f, 1f) < 1f / remaining)
                    {
                        newHeads.Add(head + dir);
                        walls[head.x, head.y, head.z] &= directionMasks[dir];
                        Vector3Int dest = head + dir;
                        if (walls[dest.x, dest.y, dest.z] == 0)
                        {
                            touchCount++;
                            walls[dest.x, dest.y, dest.z] = directionMasks[-dir];
                        }
                        else
                        {
                            walls[dest.x, dest.y, dest.z] &= directionMasks[-dir];
                        }
// repr[head.z, head.x] = asciiMap[walls[head.x, head.y, head.z]];
// repr[dest.z, dest.x] = asciiMap[walls[dest.x, dest.y, dest.z]];
// layoutLog += Utilities.Join2D(repr) + " " + touchCount.ToString() + "\n\n";
                    }
                    else
                    {
                        remaining--;
                    }
                }
                newDirs.Clear();
            }
            heads = newHeads;
            if (heads.Count() == 0 && touchCount != dims.x*dims.z)
            {
                for (int i = 0; i < dims.x; i++)
                {
                    for (int j = 0; j < dims.z; j++)
                    {
                        if (walls[i, root.y, j] == 0)
                        {
                            if (i > 0)
                            {
                                i--;
                            } else
                            {
                                j--;
                            }
                            heads.Add(new Vector3Int(i, root.y, j));
                            break;
                        }
                    }
                    if (heads.Count() > 0) break;
                }
            }
        }
// Debug.Log(layoutLog);
    }

    private string[] PrepareString(string s, char delimiter = ',', char comment = '#')
    {
        s = removeWhitespace.Replace(s, "");
        s = s.Split(comment)[0].ToLower();

        return s.Split(delimiter);
    }
    
    private (string, char) SplitLine(string line)
    {
        line = PrepareString(line)[0];
        return (line[..^1], line[^1]);
    }
}