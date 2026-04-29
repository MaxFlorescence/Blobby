/// <summary>
///     The different states that the game can be in.
/// </summary>
public enum GameState
{
    Paused, Playing, Unstarted
}

/// <summary>
///     The different states that the game can finish in.
/// </summary>
public enum FinishState
{
    Won, Lost, Unfinished
}

public static class GameInfo
{
    // Constants
    public static readonly string MAIN_MENU = "Main Menu";
    public static readonly string DEMO_LEVEL = "Demo Level";

    // Settings
    public static OptionsStruct options;
    public static OptionsMenu OptionsMenu { get; set; }

    // Game State
    public static bool DebugMode { get; set; } = false;
    public static bool StartCutscene { get; set; } = false;
    public static GameState GameStatus { get; set; } = GameState.Unstarted;
    public static FinishState FinishStatus { get; set; } = FinishState.Unfinished;
    public static bool PauseAudio { get; set; } = false;
    public static bool ControlledCameraIsMain { get; set; } = false;

    // Global object references
    public static PriorityCamera ControlledCamera { get; set; } = null;
    public static MiniMap ActiveMiniMap { get; set; } = null;
    public static Dungeon CurrentDungeon { get; set; } = null;
    public static ConfirmationDialogMenu ConfirmationDialogMenu { get; set; } = null;

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
