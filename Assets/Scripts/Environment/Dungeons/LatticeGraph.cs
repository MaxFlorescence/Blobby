using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

[Flags]
public enum Walls
{
    ALL     = 0b_0111_1111,
    LOCKED  = 0b_1000_0000,
    SET     = 0b_0100_0000,
    UP      = 0b_0010_0000,
    FORWARD = 0b_0001_0000,
    RIGHT   = 0b_0000_1000,
    BACK    = 0b_0000_0100,
    LEFT    = 0b_0000_0010,
    DOWN    = 0b_0000_0001,
    NONE    = 0b_0100_0000,

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
    STAIRS_DOWN_L = DEAD_END_L - DOWN
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

    private static readonly Dictionary<Walls, char> wallCharacters = new()
    {
        {Walls.NONE,          'O'},
        {Walls.CROSSING,      '┼'},
        {Walls.JUNCTION_F,    '┴'},
        {Walls.JUNCTION_R,    '├'},
        {Walls.JUNCTION_B,    '┬'},
        {Walls.JUNCTION_L,    '┤'},
        {Walls.HALLWAY_FB,    '│'},
        {Walls.HALLWAY_RL,    '─'},
        {Walls.CORNER_LF,     '┘'},
        {Walls.CORNER_FR,     '└'},
        {Walls.CORNER_RB,     '┌'},
        {Walls.CORNER_BL,     '┐'},
        {Walls.DEAD_END_F,    '╵'},
        {Walls.DEAD_END_R,    '╶'},
        {Walls.DEAD_END_B,    '╷'},
        {Walls.DEAD_END_L,    '╴'},
        {Walls.STAIRS_UP_F,   '┆'},
        {Walls.STAIRS_UP_R,   '┄'},
        {Walls.STAIRS_UP_B,   '┆'},
        {Walls.STAIRS_UP_L,   '┄'},
        {Walls.STAIRS_DOWN_F, '┊'},
        {Walls.STAIRS_DOWN_R, '┈'},
        {Walls.STAIRS_DOWN_B, '┊'},
        {Walls.STAIRS_DOWN_L, '┈'}
    };
    public static char ToChar(this Walls walls) {
        try {
            return wallCharacters[walls & ~Walls.LOCKED];
        } catch
        {
            return '?';
        }
    }
    public static (DungeonTileType, Quaternion) TileInfo(this Walls walls) => wallsToTile[walls & ~Walls.LOCKED];
}

class LatticeGraph
{
    private Walls[,,] walls;
    public readonly Vector3Int dims;
    public int UnsetWalls { get; private set; }
    private const float STAIR_TIME = 0.5f;
    private int stairThreshold;
    private readonly float reconnectChance = 0.1f;

    public Walls this[int x, int y, int z] {
        private set => walls[x, y, z] = value;
        get => walls[x, y, z];
    }
    public Walls this[Vector3Int index] {
        private set => walls[index.x, index.y, index.z] = value;
        get => walls[index.x, index.y, index.z];
    }

    public LatticeGraph(Vector3Int dims, Vector3Int root)
    {
        this.dims = dims;
        walls = new Walls[dims.x, dims.y, dims.z];
        GenerateRandomLayout(root);
    }

    private void ResetUnsetWalls()
    {
        UnsetWalls = dims.x * dims.z;
        stairThreshold = (int)(STAIR_TIME * UnsetWalls);
    }

    private bool IsOutOfBounds(Vector3Int index)
    {
        try {
            Walls test = this[index];
            return false;
        } catch
        {
            return true;
        }
    }

    public bool IsSet(int x, int y, int z)
    {
        return (Walls.SET & walls[x, y, z]) > 0;
    }
    public bool IsSet(Vector3Int index)
    {
        return IsSet(index.x, index.y, index.z);
    }

    public void Lock(int x, int y, int z)
    {
        if (!IsSet(x, y, z))
        {
            UnsetWalls--;
        }

        this[x, y, z] |= Walls.LOCKED | Walls.SET;
    }
    public void Lock(Vector3Int index)
    {
        Lock(index.x, index.y, index.z);
    }
    public bool IsLocked(int x, int y, int z)
    {
        return (Walls.LOCKED & walls[x, y, z]) > 0;
    }
    public bool IsLocked(Vector3Int index)
    {
        return IsLocked(index.x, index.y, index.z);
    }

    public void SetAsNone(Vector3Int node)
    {
        Lock(node);
        foreach (Vector3Int dir in Utilities.planarDirections)
        {
            try {
                if (IsSet(node + dir)) {
                    this[node + dir] |= (-dir).GetWall();
                }
            }
            catch { /* continue */ }
        }
    }

    public bool Connect(Vector3Int from, Vector3Int direction)
    {
        if (IsSet(from) && (this[from] & direction.GetWall()) == 0)
        {
            return false;
        }

        Vector3Int to = from + direction;
        if (IsLocked(from) || IsLocked(to))
        {
            return false;
        }

        if (!IsSet(from)) {
            this[from] = Walls.ALL;
            UnsetWalls--;
        }
        this[from] &= ~direction.GetWall();

        if (!IsSet(to)) {
            this[to] = Walls.ALL;
            UnsetWalls--;
        }
        this[to] &= ~(-direction).GetWall();
        
        return true;
    }

    public void ConnectRandom(Vector3Int from, Func<int, int, Vector3Int, float> connectChance,
        float reconnectChance = 0f, HashSet<Vector3Int> newConnections = null,
        bool planarOnly = true)
    {
        int connected = 0;
        int remaining = planarOnly ? 4 : 6;
        Vector3Int? rejectedDir = null;

        foreach (Vector3Int dir in Utilities.RandomDirections(planarOnly))
        {
            Vector3Int to = from + dir;
            bool noLoops = true;
            
            try {
                if (IsSet(to))
                {
                    if (Random.Range(0f, 1f) < reconnectChance)
                    {
                        // allow a connection that forms a loop
                        noLoops = false;
                    } else {
                        continue;
                    }
                }
                
                if (Random.Range(0f, 1f) < connectChance(connected, remaining, dir))
                {
                    if (Connect(from, dir) && noLoops) {
                        // a new, non-looping connection was formed
                        newConnections?.Add(to);
                        connected++;
                    }
                }
                else {
                    rejectedDir = dir;
                }
            }
            catch
            { /* continue */ }
            finally {
                remaining--;
            }
        }

        // One more try
        if (connected == 0 && rejectedDir != null)
        {
            Vector3Int dir = (Vector3Int)rejectedDir;
            if (Random.Range(0f, 1f) < connectChance(connected, remaining, dir) && Connect(from, dir))
            {
                newConnections?.Add(from + dir);
            }
        }
    }

    public Vector3Int? FindBridgePoint()
    {
        foreach((int x, int y, int z) in Utilities.Indices3D(dims)) {
            Vector3Int unsetIndex = new(x, y, z);
            if (IsSet(unsetIndex)) continue;

            foreach (Vector3Int dir in Utilities.RandomDirections(true))
            {
                try
                {
                    Vector3Int index = unsetIndex + dir;
                    if (IsSet(index) && !IsLocked(index))
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
        Vector3Int? point = FindBridgePoint();

        if (point != null)
        {
            return (Vector3Int)point;
        }

        throw new InvalidOperationException("[GetBridgePoint] Expected to find a bridge point, but none existed! (UnsetWalls = " + UnsetWalls.ToString() + ')');
    }

    public void GenerateRandomLayout(Vector3Int root)
    {
        Vector3Int? entrance = root;
        Vector3Int? direction = null;
        for (int y = dims.y-1 ; y >= 0; y--) {
            Assert.IsTrue(entrance?.y == y);
            (entrance, direction) = GenerateRandomLayer((Vector3Int)entrance, direction);
        }
    }
    private (Vector3Int?, Vector3Int?) GenerateRandomLayer(Vector3Int entrancePosition, Vector3Int? entranceDirection = null) {
        ResetUnsetWalls();
        (Vector3Int?, Vector3Int?) lowerEntranceInfo = (null, null);

        HashSet<Vector3Int> heads = new() { AddEntrance(entrancePosition, entranceDirection) };
        Assert.IsTrue(heads.Count > 0);

        // Randomly add the rest of the tiles
        HashSet<Vector3Int> newHeads;
        while (heads.Count > 0)
        {
            newHeads = new();
            foreach (Vector3Int head in heads)
            {
                ConnectRandom(head,
                    (_, remaining, _) => { return 1f / remaining; },
                reconnectChance, newHeads);
            }
            heads = newHeads;

            if (entrancePosition.y > 0 && lowerEntranceInfo == (null, null) && UnsetWalls <= stairThreshold)
            {
                lowerEntranceInfo = AddRandomStairsDown(entrancePosition.y);
            }

            if (heads.Count == 0 && UnsetWalls > 0)
            {
                heads.Add(GetBridgePoint());
            }
        }

        return lowerEntranceInfo;
    }

    private Vector3Int AddEntrance(Vector3Int entrancePosition, Vector3Int? entranceDirection = null)
    {
        foreach (Vector3Int dir in Utilities.RandomDirections(true))
        {
            if (entranceDirection != null && entranceDirection != dir) {
                continue;
            }

            try
            {
                if (Connect(entrancePosition, dir)) {
                    this[entrancePosition] &= ~Walls.UP;
                    Lock(entrancePosition);
                    SetAsNone(entrancePosition - dir);
                    return entrancePosition + dir;
                }
            } catch { /* continue */ }
        }

        throw new InvalidOperationException("[AddEntrance] Could not add an entrance at " + entrancePosition.ToString());
    }

    private (Vector3Int, Vector3Int) AddRandomStairsDown(int y)
    {
        HashSet<(Vector3Int, Vector3Int)> foundSpots = new();

        foreach ((int x, int z) in Utilities.Indices2D(dims))
        {
            if (IsSet(x, y, z)) continue;
            Vector3Int index = new(x, y, z);

            foreach (Vector3Int axis in Utilities.planarAxes)
            {
                try
                {
                    if (!IsSet(index + axis) & IsSet(index - axis)) {
                        foundSpots.Add((index, axis));
                    }
                    if (!IsSet(index - axis) & IsSet(index + axis)) {
                        foundSpots.Add((index, -axis));
                    }
                } catch { /* continue */ }
            }
        }

        (Vector3Int, Vector3Int)[] possibleSpots = foundSpots.ToArray();
        foreach (int i in Utilities.ArgShuffle(possibleSpots))
        {
            (Vector3Int stairsDownIndex, Vector3Int stairDir) = possibleSpots[i];

            Vector3Int exit = stairsDownIndex + Vector3Int.down + 2*stairDir;
            if (!IsOutOfBounds(exit)) {
                if(Connect(stairsDownIndex, -stairDir))
                {
                    this[stairsDownIndex] &= ~Walls.DOWN;
                    Lock(stairsDownIndex);

                    SetAsNone(stairsDownIndex + stairDir);

                    return (stairsDownIndex + stairDir + Vector3Int.down, stairDir);
                }
            }
        }

        throw new InvalidOperationException("[AddRandomStairsDown] Could not add stairs down from level " + y.ToString() + '!');
    }

    private void PrintLayout()
    {
        string result = "";

        for (int y = dims.y-1; y >= 0; y--)
        {
            result += "Level " + y.ToString() + '\n';
            for (int z = dims.z-1; z >= 0; z--)
            {
                for (int x = 0; x < dims.x; x++) {
                    result += this[x, y, z].ToChar();
                }
                result += '\n';
            }
            result += '\n';
        }

        Debug.Log(result);
    }
}