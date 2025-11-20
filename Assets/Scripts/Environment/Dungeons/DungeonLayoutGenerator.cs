using System;
using UnityEngine;

class DungeonLayoutGenerator
{
    public readonly int Length;

    public readonly Vector3Int dims;
    public readonly Vector3Int root;
    private readonly string[] layoutFileLines;
    private readonly bool fromFile;
    private readonly LatticeGraph lattice;

    public DungeonLayoutGenerator(Vector3Int dims, Vector3Int root)
    {
        this.dims = dims;
        this.root = root;
        Length = dims.x * dims.y * dims.z;
        fromFile = false;
        lattice = new(dims, root);
    }

    public DungeonLayoutGenerator(string[] layoutFileLines)
    {
        string[] preppedStr = PrepareString(layoutFileLines[0]);
        dims.x = int.Parse(preppedStr[0]);
        dims.y = int.Parse(preppedStr[1]);
        dims.z = int.Parse(preppedStr[2]);

        preppedStr = PrepareString(layoutFileLines[1]);
        root.x = int.Parse(preppedStr[0]);
        root.y = int.Parse(preppedStr[1]);
        root.z = int.Parse(preppedStr[2]);

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
            // +2 because first line is dims, second line is root
            (tileName, tileType, tileRotation) = GetTileFromLines(index + 2);
        }
        else
        {
            (tileName, tileType, tileRotation) = GetTileInternal(position);
        }
        Vector3 tilePosition = parent.PositionOf(position);
        
        DungeonTile tile = DungeonTile.MakeTile(tileType, tilePosition, tileRotation, parent.gameObject);
        tile.SetName(parent.name + "-" + tileName + "-" + string.Join("_", position));

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

        // string tileName;
        // if (root == position)
        // {
        //     tileName = "entrance";
        //     tileType = DungeonTileType.ENTRANCE;
        // } else
        // {
        //     tileName = tileType.GetName();
        // }

        return (tileType.GetName(), tileType, tileRotation);
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

    private string[] PrepareString(string s, char delimiter = ',', char comment = '#')
    {
        s = s.RemoveWhitespace().Split(comment)[0].ToLower();

        return s.Split(delimiter);
    }
    
    private (string, char) SplitLine(string line)
    {
        line = PrepareString(line)[0];
        return (line[..^1], line[^1]);
    }
}