using System;
using UnityEngine;

/// <summary>
///     A class that provides quaternions corresponding to 90 degree rotations about each cardinal
///     axis, as well as some compositions of these rotations.
/// </summary>
public static class CardinalRotation
{
    /// <summary>
    ///     Performs no rotations.
    /// </summary>
    public static readonly Quaternion Forward = Quaternion.identity;
    
    // ---------------------------------------------------------------------------------------------
    // X AXIS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     Rotates an object such that its forward vector moves to its up vector, while keeping
    ///     the right vector unchanged.
    /// </summary>
    public static readonly Quaternion Up = Quaternion.Euler(-90, 0, 0);
    /// <summary>
    ///     Rotates an object such that its forward vector moves to its down vector, while keeping
    ///     the right vector unchanged.
    /// </summary>
    public static readonly Quaternion Down = Quaternion.Euler(90, 0, 0);
    /// <summary>
    ///     Rotates an object such that its foward vector moves to its back vector, while keeping
    ///     the right vector unchanged.
    /// </summary>
    public static readonly Quaternion PitchOver = Down * Down;

    // ---------------------------------------------------------------------------------------------
    // Y AXIS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     Rotates an object such that its forward vector moves to its left vector, while keeping
    ///     the up vector unchanged.
    /// </summary>
    public static readonly Quaternion Left = Quaternion.Euler(0, -90, 0);
    /// <summary>
    ///     Rotates an object such that its forward vector moves to its right vector, while keeping
    ///     the up vector unchanged.
    /// </summary>
    public static readonly Quaternion Right = Quaternion.Euler(0, 90, 0);
    /// <summary>
    ///     Rotates an object such that its forward vector moves to its back vector, while keeping
    ///     the up vector unchanged.
    /// </summary>
    public static readonly Quaternion Back = Right * Right;
    
    // ---------------------------------------------------------------------------------------------
    // Z AXIS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     Rotates an object such that its right vector moves to its down vector, while keeping
    ///     the forward vector unchanged.
    /// </summary>
    public static readonly Quaternion AntiClockwise = Quaternion.Euler(0, 0, -90);
    /// <summary>
    ///     Rotates an object such that its right vector moves to its up vector, while keeping
    ///     the forward vector unchanged.
    /// </summary>
    public static readonly Quaternion Clockwise = Quaternion.Euler(0, 0, 90);
    /// <summary>
    ///     Rotates an object such that its right vector moves to its left vector, while keeping
    ///     the forward vector unchanged.
    /// </summary>
    public static readonly Quaternion RollOver = Clockwise * Clockwise;

    private static readonly DirectionMap<Quaternion> Orientations = new(
        upValue:      Up,      downValue:  Down,
        leftValue:    Left,    rightValue: Right,
        forwardValue: Forward, backValue:  Back,
        defaultValue: Forward
    );

    /// <returns>
    ///     The CardinalRotation corresponding to the given unit direction vector. This can only
    ///     result in one of
    ///     <br/>
    ///     <tt>CardinalRotation.Up</tt>, <tt>CardinalRotation.Down</tt>,
    ///     <br/>
    ///     <tt>CardinalRotation.Left</tt>, <tt>CardinalRotation.Right</tt>,
    ///     <br/>
    ///     <tt>CardinalRotation.Forward</tt>, or <tt>CardinalRotation.Back</tt>.
    /// </returns>
    /// <exception cref="FormatException">
    ///     If given a string that does not correspond to any available cardinal rotation.
    /// </exception>
    public static Quaternion Parse(Vector3Int direction)
    {
        return Orientations[direction];
    }

    /// <returns>
    ///     The CardinalRotation corresponding to the given string.
    /// </returns>
    /// <exception cref="FormatException">
    ///     If given a string that does not correspond to any available cardinal rotation.
    /// </exception>
    public static Quaternion Parse(string rotation)
    {
        return rotation.ToLower() switch
        {
            "forward"       or "f" => Forward,

            "up"            or "u" => Up,
            "down"          or "d" => Down,
            "pitchover"     or "p" => PitchOver,

            "left"          or "l" => Left,
            "right"         or "r" => Right,
            "back"          or "b" => Back,

            "clockwise"     or "c" => Clockwise,
            "anticlockwise" or "a" => AntiClockwise,
            "rollover"      or "o" => RollOver,

            _ => throw new FormatException($"\"{rotation}\" does not represent a cardinal rotation!")
        };
    }

    /// <returns>
    ///     The CardinalRotation corresponding to the given character.
    /// </returns>
    /// <exception cref="FormatException">
    ///     If given a character that does not correspond to any available cardinal rotation.
    /// </exception>
    public static Quaternion Parse(char rotation)
    {
        return Parse(rotation.ToString());
    }
}