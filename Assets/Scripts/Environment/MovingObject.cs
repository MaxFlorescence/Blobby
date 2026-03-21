using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
///     A struct defining a key point for objects.
/// </summary>
[Serializable, Inspectable]
public struct ObjectKeyPoint
{
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
}

/// <summary>
///     A class for controlling an object's movement using a sequence of key points.
/// </summary>
public class MovingObject : MonoBehaviour
{
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
    /// <summary>
    ///     The list of key points for the object to move through. Each key point defines a
    ///     position, orientation, the amount of time it takes to move to the key point, and the
    ///     amount of time to pause for before starting the next key point.
    /// </summary>
    public ObjectKeyPoint[] keyPoints;

    private int currentKeyPoint;
    private Timer timer = new();
    /// <summary>
    ///     Indicates if the object is in a moving time interval.
    /// </summary>
    private bool objectIsMoving;
    /// <summary>
    ///     Indicates if the object is progressing through its key points.
    /// </summary>
    private bool playing = true;

    void Start()
    {
        if (localCoordinates)
        {
            // ensure coordinates are world-based
            for (int i = 0; i < keyPoints.Length; i++)
            {
                keyPoints[i].position = transform.parent.TransformPoint(keyPoints[i].position);
            }
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
        objectIsMoving = false;
        timer.SetInterval(keyPoints[0].pauseTime);
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

        if (loop && currentKeyPoint >= keyPoints.Length)
        {
            currentKeyPoint = 0;
        }

        if (currentKeyPoint < keyPoints.Length)
        {
            if (timer.Update())
            {
                objectIsMoving = !objectIsMoving;

                if (objectIsMoving)
                {
                    // interpolation for last point has finished
                    currentKeyPoint++;
                    timer.SetInterval(keyPoints[currentKeyPoint].moveTime);
                } else {
                    timer.SetInterval(keyPoints[currentKeyPoint].pauseTime);
                }
            }

            if (objectIsMoving)
            {
                // interpolate position linearly
                Vector3 position = Vector3.Lerp(
                    keyPoints.ModularGet(currentKeyPoint - 1).position,
                    keyPoints[currentKeyPoint].position,
                    timer.Progress()
                );
                // interpolate direction spherically
                Vector3 direction = Vector3.Slerp(
                    keyPoints.ModularGet(currentKeyPoint - 1).orientation, 
                    keyPoints[currentKeyPoint].orientation,
                    timer.Progress()
                );

                transform.position = position;
                transform.LookAt(position + direction);
            }
        }
    }
}
