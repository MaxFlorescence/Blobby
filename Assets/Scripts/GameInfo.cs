using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
///     States for controlling the gripped object.
/// </summary>
public enum GameState
{
    PAUSED, PLAYING, UNSTARTED
}

public enum FinishState
{
    WON, LOST, UNFINISHED
}

public static class GameInfo
{
    // Constants
    public static readonly string MAIN_MENU = "Main Menu";
    public static readonly string DEMO_LEVEL = "Demo Level";

    // Settings
    public static float MouseSensitivity { get; set; } = 1;

    // Game State
    public static bool DebugMode { get; set; } = false;
    public static bool StartCutscene { get; set; } = false;
    public static GameState GameStatus { get; set; } = GameState.UNSTARTED;
    public static FinishState FinishStatus { get; set; } = FinishState.UNFINISHED;
    public static bool PauseAudio { get; set; } = false;
    public static bool ControlledCameraIsMain { get; set; } = false;

    // Global object references
    public static PriorityCamera ControlledCamera { get; set; } = null;
    public static MiniMap ActiveMiniMap { get; set; } = null;
    public static Dungeon CurrentDungeon { get; set; } = null;

    public static BlobController ControlledBlob { get; private set; } = null;
    public static void SetControlledBlob(BlobController blob)
    {
        if (ControlledBlob != null)
        {
            ControlledBlob.controlled = false;
        }

        blob.controlled = true;
        ControlledBlob = blob;
    }

    public static AlertSystem AlertSystem { get; set; } = null;
}
