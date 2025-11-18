using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Flags]
public enum Walls
{
    SET     = 0b_0100_0000,
    UP      = 0b_0010_0000,
    FORWARD = 0b_0001_0000,
    RIGHT   = 0b_0000_1000,
    BACK    = 0b_0000_0100,
    LEFT    = 0b_0000_0010,
    DOWN    = 0b_0000_0001,
    NONE    = 0b_0000_0000,

    // Dungeon tile wall configurations
    CROSSING = SET | UP | DOWN,
    JUNCTION_F = CROSSING | BACK,
    JUNCTION_R = CROSSING | LEFT,
    JUNCTION_B = CROSSING | FORWARD,
    JUNCTION_L = CROSSING | RIGHT,
    HALLWAY_RL = JUNCTION_F | FORWARD,
    HALLWAY_FB = JUNCTION_R | RIGHT,
    CORNER_LF = JUNCTION_L | BACK,
    CORNER_FR = JUNCTION_F | LEFT,
    CORNER_RB = JUNCTION_R | FORWARD,
    CORNER_BL = JUNCTION_B | RIGHT,
    DEAD_END_F = HALLWAY_FB | BACK,
    DEAD_END_R = HALLWAY_RL | LEFT,
    DEAD_END_B = HALLWAY_FB | FORWARD,
    DEAD_END_L = HALLWAY_RL | RIGHT,
    STAIRS_UP_F = DEAD_END_B - UP,
    STAIRS_UP_R = DEAD_END_L - UP,
    STAIRS_UP_B = DEAD_END_F - UP,
    STAIRS_UP_L = DEAD_END_R - UP,
    STAIRS_DOWN_F = DEAD_END_F - DOWN,
    STAIRS_DOWN_R = DEAD_END_R - DOWN,
    STAIRS_DOWN_B = DEAD_END_B - DOWN,
    STAIRS_DOWN_L = DEAD_END_L - DOWN,
    ALL = DEAD_END_F | FORWARD
}

public static class WallsExtensions
{
    private static readonly Dictionary<Vector3Int, Walls> dirToWall = new()
    {
        {Vector3Int.forward, Walls.FORWARD},
        {Vector3Int.right,   Walls.RIGHT},
        {Vector3Int.back,    Walls.BACK},
        {Vector3Int.left,    Walls.LEFT},
        {Vector3Int.up,      Walls.UP},
        {Vector3Int.down,    Walls.DOWN}
    };
    public static Walls GetWall(this Vector3Int dir) => dirToWall[dir];

    private static readonly Dictionary<Walls, (DungeonTileType, Quaternion)> wallsToTile = new()
    {
        {Walls.NONE,          (DungeonTileType.NONE,        Rotation.FORWARD)},
        {Walls.CROSSING,      (DungeonTileType.CROSSING,    Rotation.FORWARD)},
        {Walls.JUNCTION_F,    (DungeonTileType.JUNCTION,    Rotation.FORWARD)},
        {Walls.JUNCTION_R,    (DungeonTileType.JUNCTION,    Rotation.RIGHT)},
        {Walls.JUNCTION_B,    (DungeonTileType.JUNCTION,    Rotation.BACK)},
        {Walls.JUNCTION_L,    (DungeonTileType.JUNCTION,    Rotation.LEFT)},
        {Walls.HALLWAY_FB,    (DungeonTileType.HALLWAY,     Rotation.FORWARD)},
        {Walls.HALLWAY_RL,    (DungeonTileType.HALLWAY,     Rotation.RIGHT)},
        {Walls.CORNER_LF,     (DungeonTileType.CORNER,      Rotation.FORWARD)},
        {Walls.CORNER_FR,     (DungeonTileType.CORNER,      Rotation.RIGHT)},
        {Walls.CORNER_RB,     (DungeonTileType.CORNER,      Rotation.BACK)},
        {Walls.CORNER_BL,     (DungeonTileType.CORNER,      Rotation.LEFT)},
        {Walls.DEAD_END_F,    (DungeonTileType.DEAD_END,    Rotation.FORWARD)},
        {Walls.DEAD_END_R,    (DungeonTileType.DEAD_END,    Rotation.RIGHT)},
        {Walls.DEAD_END_B,    (DungeonTileType.DEAD_END,    Rotation.BACK)},
        {Walls.DEAD_END_L,    (DungeonTileType.DEAD_END,    Rotation.LEFT)},
        {Walls.STAIRS_UP_F,   (DungeonTileType.STAIRS_UP,   Rotation.FORWARD)},
        {Walls.STAIRS_UP_R,   (DungeonTileType.STAIRS_UP,   Rotation.RIGHT)},
        {Walls.STAIRS_UP_B,   (DungeonTileType.STAIRS_UP,   Rotation.BACK)},
        {Walls.STAIRS_UP_L,   (DungeonTileType.STAIRS_UP,   Rotation.LEFT)},
        {Walls.STAIRS_DOWN_F, (DungeonTileType.STAIRS_DOWN, Rotation.FORWARD)},
        {Walls.STAIRS_DOWN_R, (DungeonTileType.STAIRS_DOWN, Rotation.RIGHT)},
        {Walls.STAIRS_DOWN_B, (DungeonTileType.STAIRS_DOWN, Rotation.BACK)},
        {Walls.STAIRS_DOWN_L, (DungeonTileType.STAIRS_DOWN, Rotation.LEFT)}
    };
    public static (DungeonTileType, Quaternion) TileInfo(this Walls walls) => wallsToTile[walls];
}

class LatticeGraph
{
    private Walls[,,] walls;
    public readonly Vector3Int dims;
    public int UnsetWalls { get; private set; }

    public Walls this[int x, int y, int z] {
        private set => walls[x, y, z] = value;
        get => walls[x, y, z];
    }
    public Walls this[Vector3Int index] {
        private set => walls[index.x, index.y, index.z] = value;
        get => walls[index.x, index.y, index.z];
    }

    public LatticeGraph(Vector3Int dims)
    {
        this.dims = dims;
        walls = new Walls[dims.x, dims.y, dims.z];
        UnsetWalls = dims.x * dims.y * dims.z;
    }
    public LatticeGraph(int dimX, int dimY, int dimZ) : this(new Vector3Int(dimX, dimY, dimZ)) { }

    public bool IsSet(int x, int y, int z)
    {
        return (Walls.SET & walls[x, y, z]) > 0;
    }
    public bool IsSet(Vector3Int index)
    {
        return IsSet(index.x, index.y, index.z);
    }

    public void Connect(Vector3Int from, Vector3Int direction)
    {
        if (!IsSet(from)) {
            this[from] = Walls.ALL;
            UnsetWalls--;
        }
        this[from] &= ~direction.GetWall();

        Vector3Int to = from + direction;
        if (!IsSet(to)) {
            this[to] = Walls.ALL;
            UnsetWalls--;
        }
        this[to] &= ~(-direction).GetWall();
    }

    public void ConnectRandom(Vector3Int from, Func<int, int, Vector3Int, float> pdf,
        float reconnectChance = 0f, HashSet<Vector3Int> newConnections = null,
        bool planarOnly = true)
    {
        int connected = 0;
        int remaining = planarOnly ? 4 : 6;
        Vector3Int[] dirs = planarOnly ? Utilities.planarDirections : Utilities.cardinalDirections;

        foreach (Vector3Int dir in Utilities.RandomDirections(true))
        {
            Vector3Int to = from + dir;
            bool notReconnecting = true;
            
            try {
                if (IsSet(to))
                {
                    if (Random.Range(0f, 1f) < reconnectChance)
                    {
                        notReconnecting = false;
                    } else {
                        remaining--;
                        continue;
                    }
                }
                
                if (Random.Range(0f, 1f) < pdf(connected, remaining, dir))
                {
                    Connect(from, dir);
                    if (notReconnecting) {
                        newConnections?.Add(to);
                        connected++;
                    }
                }
            } catch { /* continue */ }

            remaining--;
        }
    }
    public Vector3Int? FindBridgePoint()
    {
        foreach(Vector3Int unsetIndex in ForEachIndex((x, y, z) => {
            return new Vector3Int(x, y, z);
        }, (x, y, z) => {
            return !IsSet(x, y, z);
        })) {
            foreach (Vector3Int dir in Utilities.RandomDirections(true))
            {
                try
                {
                    Vector3Int index = unsetIndex + dir;
                    if (IsSet(index))
                    {
                        return index;
                    }
                } catch { /* continue */ }
            }
        }

        return null;
    }

    public Vector3Int GetBridgePoint()
    {
        return (Vector3Int)FindBridgePoint();
    }

    private IEnumerable<T> ForEachIndex<T>(Func<int, int, int, T> transform, Func<int, int, int, bool> filter)
    {
        for (int x = 0; x < dims.x; x++)
        {
            for (int y = 0; y < dims.y; y++)
            {
                for (int z = 0; z < dims.z; z++)
                {
                    if (filter(x, y, z))
                    {
                        yield return transform(x, y, z);
                    }
                }
            }
        }
        yield break;
    }
}