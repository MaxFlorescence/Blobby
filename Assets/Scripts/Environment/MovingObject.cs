using System;
using Unity.VisualScripting;
using UnityEngine;

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

    public override string ToString()
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

/// <summary>
///     A class for controlling an object's movement using a sequence of key points.
/// </summary>
public class MovingObject : MonoBehaviour
{
    // ---------------------------------------------------------------------------------------------
    // PARAMETERS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     Iff <tt>true</tt>, the object's movement will repeat from the first key point after
    ///     the last key point is finished.
    /// </summary>
    public bool loop = false;
    /// <summary>
    ///     Iff <tt>true</tt>, the key point's positions will be converted from being local to this
    ///     transform's parent to being world-based.
    /// </summary>
    public bool localCoordinates = false;

    // ---------------------------------------------------------------------------------------------
    // STATE
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     Indicates if the object is progressing through its key points.
    /// </summary>
    private bool playing = true;
    
    // ---------------------------------------------------------------------------------------------
    // PROGRESSION
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The list of key points for the object to move through. Each key point defines a
    ///     position, orientation, the amount of time it takes to move to the key point, and the
    ///     amount of time to pause for before starting the next key point.
    /// </summary>
    public ObjectKeyPoint[] keyPoints;
    /// <summary>
    ///     The list of StagedTimers corresponding to the object key points.
    /// </summary>
    private StagedTimer[] keyPointTimers;
    private int currentKeyPoint;

    void Start()
    {
        keyPointTimers = new StagedTimer[keyPoints.Length];

        for (int i = 0; i < keyPoints.Length; i++)
        {
            if (localCoordinates)
            {
                // ensure coordinates are world-based and timers are initialized
                keyPoints[i].position = transform.parent.TransformPoint(keyPoints[i].position);
            }

            keyPointTimers[i] = keyPoints[i].MakeTimer();
        }

        if (keyPoints.Length == 0)
        {
            Stop();
        }
        else
        {
            Restart();
        }
    }

    public void Restart()
    {
        // skip move time for first key point
        currentKeyPoint = 0;
        keyPointTimers[0].GoToStage(ObjectKeyPoint.PAUSED);
        transform.position = keyPoints[0].position;
        transform.LookAt(keyPoints[0].position + keyPoints[0].orientation);

        Play();
    }

    public void Stop()
    {
        playing = false;
    }

    public void Play()
    {
        playing = true;
    }

    public bool IsPlaying()
    {
        return playing;
    }

    /// <summary>
    ///     For the first <tt>moveTime</tt> seconds of a key point, interpolate the object to its
    ///     position and orientation.
    ///     <br/>
    ///     For the next <tt>pauseTime</tt> seconds of a key point, do nothing.
    ///     <br/>
    ///     After these time periods, start the next key point.
    ///     <br/>
    ///     After all key points are finished, end the motion.
    /// </summary>
    void LateUpdate()
    {
        if (!playing) return;

        if (keyPointTimers[currentKeyPoint].Update()) {
            currentKeyPoint++;
            if (loop && currentKeyPoint >= keyPoints.Length) currentKeyPoint = 0;
        }

        StageState timerState = keyPointTimers[currentKeyPoint].State;

        if (timerState.stageName == ObjectKeyPoint.MOVING)
        {
            // interpolate position linearly
            Vector3 position = Vector3.Lerp(
                keyPoints.ModularGet(currentKeyPoint - 1).position,
                keyPoints.ModularGet(currentKeyPoint).position,
                timerState.progress
            );
            // interpolate direction spherically
            Vector3 direction = Vector3.Slerp(
                keyPoints.ModularGet(currentKeyPoint - 1).orientation, 
                keyPoints.ModularGet(currentKeyPoint).orientation,
                timerState.progress
            );

            transform.position = position;
            transform.LookAt(position + direction);
        }
    }
}
