using System;
using UnityEngine;
using Unity.VisualScripting;

/// <summary>
///     A struct defining a key point for objects.
/// </summary>
[Serializable, Inspectable]
public struct ObjectKeyPoint
{
    public static readonly string MOVING = "Moving";
    public static readonly string PAUSED = "Paused";

    /// <summary>
    ///     The position of the object at this key point.
    /// </summary>
    public Vector3 position;
    /// <summary>
    ///     The (normalized) orientation of the object at this key point.
    /// </summary>
    public Vector3 orientation;
    /// <summary>
    ///     The amount of time it takes to move from the previous key point to this one.
    /// </summary>
    public float moveTime;
    /// <summary>
    ///     The amount of time to pause for after moving to this key point, but before moving to the
    ///     next key point.
    /// </summary>
    public float pauseTime;

    public ObjectKeyPoint(Vector3 position, Vector3 orientation, float pauseTime, float moveTime)
    {
        this.position = position;
        this.orientation = orientation.normalized;
        this.moveTime = moveTime;
        this.pauseTime = pauseTime;
    }

    public override readonly string ToString()
    {
        return $"ObjectKeyPoint(position = {position}, orientation = {orientation}, moveTime = {moveTime}, pauseTime = {pauseTime})";
    } 
}

public static class ObjectKeyPointExtensions
{
    private static readonly string[] STAGES = {ObjectKeyPoint.MOVING, ObjectKeyPoint.PAUSED};
    
    /// <returns>
    ///     A two-stage StagedTimer corresponding to the ObjectKeyPoint's moveTime and
    ///     pauseTime values. The two stages will be named using <tt>ObjectKeyPoint.MOVING</tt> and
    ///     <tt>ObjectKeyPoint.PAUSED</tt>.
    /// </returns>
    public static StagedTimer MakeTimer(this ObjectKeyPoint point)
    {
        return new(new float[] {point.moveTime, point.pauseTime}, STAGES);
    }
}