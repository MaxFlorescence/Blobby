using UnityEngine;

/// <summary>
///     A class for controlling a camera's movement using a sequence of POVs.
/// </summary>
public class CutsceneCameraController : PriorityCamera
{
    /// <summary>
    ///     The canvas overlay to display during the cutscene.
    /// </summary>
    public GameObject cutsceneOverlay;
    private MovingObject movingObject;

    void Awake()
    {
        SetMaxPriority(2);
        movingObject = gameObject.AddComponent<MovingObject>();
        movingObject.keyPoints = new ObjectKeyPoint[]
        {
            new(new(-30, 40, -47), new(0, 0, -1),  2f,   0f),
            new(new(-40, 30, -40), new(-1, 0, -1), 1.5f, 2f),
            new(new(47, 30, 47),   new(-1, 0, -1), 1f,   4f),
            new(new(47, 30, 47),   new(0, -1, 0),  0.5f, 2f)
        };
    }

    void Start()
    {
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
        movingObject.Restart();
    }

    void EndCutscene()
    {
        SetPriority(0);
        
        movingObject.Stop();
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

    void LateUpdate()
    {
        if (!movingObject.IsPlaying())
        {
            EndCutscene();
        }
    }
}
