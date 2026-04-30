using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     The different configurations of walls that can exist for a dungeon tile.
/// </summary>
[Flags]
public enum Walls
{
    All     = 0b_0111_1111,
    Locked  = 0b_1000_0000,
    Set     = 0b_0100_0000,
    Up      = 0b_0010_0000,
    Forward = 0b_0001_0000,
    Right   = 0b_0000_1000,
    Back    = 0b_0000_0100,
    Left    = 0b_0000_0010,
    Down    = 0b_0000_0001,
    Zero    = 0b_0000_0000,

    // Dungeon tile wall configurations
    Crossing = Set | Up | Down,
    Junction_Forward = Crossing | Back,
    Junction_Right = Crossing | Left,
    Junction_Back = Crossing | Forward,
    Junction_Left = Crossing | Right,
    Hallway_Right_Left = Junction_Forward | Forward,
    Hallway_Forward_Back = Junction_Right | Right,
    Corner_Left_Forward = Junction_Left | Back,
    Corner_Forward_Right = Junction_Forward | Left,
    Corner_Right_Back = Junction_Right | Forward,
    Corner_Back_Left = Junction_Back | Right,
    Dead_End_Forward = Hallway_Forward_Back | Back,
    Dead_End_Right = Hallway_Right_Left | Left,
    Dead_End_Back = Hallway_Forward_Back | Forward,
    Dead_End_Left = Hallway_Right_Left | Right,
    Stairs_Up_Forward = Dead_End_Back - Up,
    Stairs_Up_Right = Dead_End_Left - Up,
    Stairs_Up_Back = Dead_End_Forward - Up,
    Stairs_Up_Left = Dead_End_Right - Up,
    Stairs_Down_Forward = Dead_End_Forward - Down,
    Stairs_Down_Right = Dead_End_Right - Down,
    Stairs_Down_Back = Dead_End_Back - Down,
    Stairs_Down_Left = Dead_End_Left - Down
}

public static class WallsExtensions
{
    /// <summary>
    ///     Maps each <tt>Vector3Int</tt> direction to the individual wall that it points toward.
    /// </summary>
    private static readonly DirectionMap<Walls> wallsMap = new(
        upValue:      Walls.Up,      downValue:  Walls.Down,
        leftValue:    Walls.Left,    rightValue: Walls.Right,
        forwardValue: Walls.Forward, backValue:  Walls.Back,
        defaultValue: Walls.Zero
    );

    /// <param name="direction">
    ///     The direction to map to a wall.
    /// </param>
    /// <returns>
    ///     The individual wall that the given direction points toward.
    /// </returns>
    public static Walls GetWall(this Vector3Int direction) => wallsMap[direction];

    /// <summary>
    ///     Mapping from Wall configurations to dungeon tile type and orientation.
    /// </summary>
    private static readonly Dictionary<Walls, (DungeonTileType, Quaternion)> wallsToTile = new()
    {
        {Walls.Set,                  (DungeonTileType.Empty,       CardinalRotation.Forward)},
        {Walls.Crossing,             (DungeonTileType.Crossing,    CardinalRotation.Forward)},
        {Walls.Junction_Forward,     (DungeonTileType.Junction,    CardinalRotation.Forward)},
        {Walls.Junction_Right,       (DungeonTileType.Junction,    CardinalRotation.Right)},
        {Walls.Junction_Back,        (DungeonTileType.Junction,    CardinalRotation.Back)},
        {Walls.Junction_Left,        (DungeonTileType.Junction,    CardinalRotation.Left)},
        {Walls.Hallway_Forward_Back, (DungeonTileType.Hallway,     CardinalRotation.Forward)},
        {Walls.Hallway_Right_Left,   (DungeonTileType.Hallway,     CardinalRotation.Right)},
        {Walls.Corner_Left_Forward,  (DungeonTileType.Corner,      CardinalRotation.Forward)},
        {Walls.Corner_Forward_Right, (DungeonTileType.Corner,      CardinalRotation.Right)},
        {Walls.Corner_Right_Back,    (DungeonTileType.Corner,      CardinalRotation.Back)},
        {Walls.Corner_Back_Left,     (DungeonTileType.Corner,      CardinalRotation.Left)},
        {Walls.Dead_End_Forward,     (DungeonTileType.Dead_End,    CardinalRotation.Forward)},
        {Walls.Dead_End_Right,       (DungeonTileType.Dead_End,    CardinalRotation.Right)},
        {Walls.Dead_End_Back,        (DungeonTileType.Dead_End,    CardinalRotation.Back)},
        {Walls.Dead_End_Left,        (DungeonTileType.Dead_End,    CardinalRotation.Left)},
        {Walls.Stairs_Up_Forward,    (DungeonTileType.Stairs_Up,   CardinalRotation.Forward)},
        {Walls.Stairs_Up_Right,      (DungeonTileType.Stairs_Up,   CardinalRotation.Right)},
        {Walls.Stairs_Up_Back,       (DungeonTileType.Stairs_Up,   CardinalRotation.Back)},
        {Walls.Stairs_Up_Left,       (DungeonTileType.Stairs_Up,   CardinalRotation.Left)},
        {Walls.Stairs_Down_Forward,  (DungeonTileType.Stairs_Down, CardinalRotation.Forward)},
        {Walls.Stairs_Down_Right,    (DungeonTileType.Stairs_Down, CardinalRotation.Right)},
        {Walls.Stairs_Down_Back,     (DungeonTileType.Stairs_Down, CardinalRotation.Back)},
        {Walls.Stairs_Down_Left,     (DungeonTileType.Stairs_Down, CardinalRotation.Left)}
    };

    /// <summary>
    ///     Mapping from wall configurations to characters for printing dungeon layouts.
    /// </summary>
    private static readonly Dictionary<Walls, char> wallCharacters = new()
    {
        {Walls.Set,                  '◌'},
        {Walls.Crossing,             '┼'},
        {Walls.Junction_Forward,     '┴'},
        {Walls.Junction_Right,       '├'},
        {Walls.Junction_Back,        '┬'},
        {Walls.Junction_Left,        '┤'},
        {Walls.Hallway_Forward_Back, '│'},
        {Walls.Hallway_Right_Left,   '─'},
        {Walls.Corner_Left_Forward,  '┘'},
        {Walls.Corner_Forward_Right, '└'},
        {Walls.Corner_Right_Back,    '┌'},
        {Walls.Corner_Back_Left,     '┐'},
        {Walls.Dead_End_Forward,     '╵'},
        {Walls.Dead_End_Right,       '╶'},
        {Walls.Dead_End_Back,        '╷'},
        {Walls.Dead_End_Left,        '╴'},
        {Walls.Stairs_Up_Forward,    '↓'},
        {Walls.Stairs_Up_Right,      '←'},
        {Walls.Stairs_Up_Back,       '↑'},
        {Walls.Stairs_Up_Left,       '→'},
        {Walls.Stairs_Down_Forward,  '↓'},
        {Walls.Stairs_Down_Right,    '←'},
        {Walls.Stairs_Down_Back,     '↑'},
        {Walls.Stairs_Down_Left,     '→'}
    };

    /// <param name="walls">
    ///     The walls configuration to map.
    /// </param>
    /// <returns>
    ///     The character corresponding to the given walls configuration, if it's valid.
    ///     <br/>
    ///     If the walls configuration is invalid, returns <tt>'?'</tt>.
    /// </returns>
    public static char ToChar(this Walls walls) {
        Walls unlockedWalls = walls & ~Walls.Locked;
        
        if (wallCharacters.ContainsKey(unlockedWalls))
        {
            return wallCharacters[unlockedWalls];
        }
        else
        {
            return '?';
        }
    }

    /// <param name="walls">
    ///     The walls configuration to map.
    /// </param>
    /// <returns>
    ///     The tile type and orientation corresponding to the given walls configuration.
    /// </returns>
    public static (DungeonTileType, Quaternion) TileInfo(this Walls walls)
        => wallsToTile[walls & ~Walls.Locked];
    
    /// <returns>
    ///     <tt>True</tt> iff the walls have the given wall flag(s).
    /// </returns>
    public static bool HasWalls(this Walls walls, Walls wall)
    {
        return (walls & wall) > 0;
    }
    
    /// <returns>
    ///     <tt>True</tt> iff the walls have the <tt>Set</tt> flag.
    /// </returns>
    public static bool IsSet(this Walls walls)
    {
        return walls.HasWalls(Walls.Set);
    }
    
    /// <returns>
    ///     <tt>True</tt> iff the walls have the <tt>Locked</tt> flag.
    /// </returns>
    public static bool IsLocked(this Walls walls)
    {
        return walls.HasWalls(Walls.Locked);
    }
}