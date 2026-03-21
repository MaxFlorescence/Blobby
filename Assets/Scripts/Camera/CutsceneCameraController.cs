using UnityEngine;

/// <summary>
///     A struct defining a keyframe for cutscene cameras.
/// </summary>
public readonly struct CameraKeyFrame
{
    /// <summary>
    ///     The position of the camera at this keyframe.
    /// </summary>
    public readonly Vector3 position;
    /// <summary>
    ///     The (normalized) orientation of the camera at this keyframe.
    /// </summary>
    public readonly Vector3 orientation;
    /// <summary>
    ///     The amount of time it takes to move from the previous keyframe to this one.
    /// </summary>
    public readonly float moveTime;
    /// <summary>
    ///     The amount of time to pause for after moving to this keyframe, but before moving to the
    ///     next keyframe.
    /// </summary>
    public readonly float pauseTime;

    public CameraKeyFrame(Vector3 position, Vector3 orientation, float pauseTime, float moveTime)
    {
        this.position = position;
        this.orientation = orientation.normalized;
        this.pauseTime = pauseTime;
        this.moveTime = moveTime;
    }
}

/// <summary>
///     A class for controlling a camera's movement using a sequence of POVs.
/// </summary>
public class CutsceneCameraController : PriorityCamera
{
    public CameraKeyFrame[] keyFrames =
    {
        new(new(-30, 40, -47), new(0, 0, -1),  2f,   0f),
        new(new(-40, 30, -40), new(-1, 0, -1), 1.5f, 2f),
        new(new(47, 30, 47),   new(-1, 0, -1), 1f,   4f),
        new(new(47, 30, 47),   new(0, -1, 0),  0.5f, 2f)
    };
    /// <summary>
    ///     The canvas overlay to display during the cutscene.
    /// </summary>
    public GameObject cutsceneOverlay;
    private int keyFrameCount;
    private int currentKeyFrame = -1;
    private Timer timer = new();
    private bool cameraIsMoving = true;

    void Awake()
    {
        SetMaxPriority(2);
    }

    void Start()
    {
        keyFrameCount = keyFrames.Length;

        // this flag might be true only when starting the game from the main menu
        if (GameInfo.StartCutscene)
        {
            BeginCutscene();
        }
        else
        {
            EndCutscene();
        }
    }

    override protected void OnActivate()
    {
        cutsceneOverlay.SetActive(true);
    }

    override protected void OnDeactivate()
    {
        cutsceneOverlay.SetActive(false);
    }

    public void BeginCutscene()
    {
        SetPriority(2);
        
        currentKeyFrame = 0;
        timer.SetInterval(keyFrames[0].moveTime);
        cameraIsMoving = true;
        transform.position = keyFrames[0].position;
        transform.LookAt(keyFrames[0].position + keyFrames[0].orientation);
    }

    void EndCutscene()
    {
        SetPriority(0);
        
        currentKeyFrame = -1;
        GameInfo.StartCutscene = false;
    }

    void Update()
    {
        // press space to skip cutscene
        if (controlled && GameInfo.StartCutscene && Input.GetButtonUp("Jump"))
        {
            EndCutscene();
        }
    }

    /// <summary>
    ///     For the first <tt>moveTime</tt> seconds of a keyframe, interpolate the camera to its
    ///     position and orientation.
    ///     <br/>
    ///     For the next <tt>pauseTime</tt> seconds of a keyframe, do nothing.
    ///     <br/>
    ///     After these time periods, start the next keyframe.
    ///     <br/>
    ///     After all keyframes are finished, end the cutscene.
    /// </summary>
    void LateUpdate()
    {
        if (currentKeyFrame < keyFrameCount && currentKeyFrame >= 0)
        {
            if (timer.Update())
            {
                cameraIsMoving = !cameraIsMoving;

                if (cameraIsMoving)
                {
                    // interpolation for last frame has finished
                    currentKeyFrame++;
                    timer.SetInterval(keyFrames[currentKeyFrame].moveTime);
                } else {
                    timer.SetInterval(keyFrames[currentKeyFrame].pauseTime);
                }
            }

            if (cameraIsMoving)
            {
                // interpolate position linearly between the last position and the target position
                Vector3 position = Vector3.Lerp(
                    keyFrames[currentKeyFrame - 1].position,
                    keyFrames[currentKeyFrame].position,
                    timer.Progress()
                );
                // interpolate direction spherically between the last direction and the target direction
                Vector3 direction = Vector3.Slerp(
                    keyFrames[currentKeyFrame - 1].orientation, 
                    keyFrames[currentKeyFrame].orientation,
                    timer.Progress()
                );

                transform.position = position;
                transform.LookAt(position + direction);
            }
        }
        else if (currentKeyFrame == keyFrameCount)
        {
            EndCutscene();
        }
    }
}
