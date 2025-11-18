using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

class DungeonLayoutGenerator
{
    public readonly int Length;
    private static readonly Regex removeWhitespace = new(@"\s");

    public readonly Vector3Int dims;
    private readonly Vector3Int root;
    private readonly string[] layoutFileLines;
    private readonly bool fromFile;
    private readonly float reconnectChance = 0.1f;
    LatticeGraph lattice;

    public DungeonLayoutGenerator(Vector3Int dims, Vector3Int root)
    {
        this.dims = dims;
        this.root = root;
        Length = dims.x * dims.y * dims.z;
        fromFile = false;
        lattice = new(dims);

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

    public DungeonTile GetTile(int index, Vector3Int position, Dungeon parent)
    {
        string tileName;
        DungeonTileType tileType;
        Quaternion tileRotation;

        if (fromFile)
        {
            (tileName, tileType, tileRotation) = GetTileFromLines(index + 1);
        }
        else
        {
            (tileName, tileType, tileRotation) = GetTileInternal(position);
        }
        Vector3 tilePosition = parent.PositionOf(position);
        
        DungeonTile tile = DungeonTile.MakeTile(tileType, tilePosition, tileRotation, parent.gameObject);
        tile.name = parent.name + "-" + tileName + "-" + string.Join("_", position);

        return tile;
    }

    private (string, DungeonTileType, Quaternion) GetTileFromLines(int index)
    {
        (string tileName, char tileRot) = SplitLine(layoutFileLines[index]);

        Enum.TryParse(tileName, true, out DungeonTileType tileType);
        return (tileName, tileType, Rotation.Parse(tileRot));
    }
    
    private (string, DungeonTileType, Quaternion) GetTileInternal(Vector3Int position)
    {
        (DungeonTileType tileType, Quaternion tileRotation) = lattice[position].TileInfo();
        string tileName;

        if (root == position)
        {
            tileName = "entrance";
            tileType = DungeonTileType.ENTRANCE;
        } else
        {
            tileName = tileType.GetName();
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
            return !lattice.IsSet(index);
        }
    }

    private void GenerateRandomLayout()
    {
        HashSet<Vector3Int> heads = new();
        foreach (Vector3Int dir in Utilities.RandomDirections(true))
        {
            try
            {
                lattice.Connect(root, dir);
                heads.Add(root + dir);
                break;
            } catch { /* continue */ }
        }

        HashSet<Vector3Int> newHeads;
        while (heads.Count > 0)
        {
            newHeads = new();
            foreach (Vector3Int head in heads)
            {
                lattice.ConnectRandom(head,
                    (_, remaining, _) => { return 1f / remaining; },
                reconnectChance, newHeads);
            }
            heads = newHeads;

            if (heads.Count() == 0 && lattice.UnsetWalls > 0)
            {
                heads.Add(lattice.GetBridgePoint());
            }
        }
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