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
    public static string MAIN_MENU = "Main Menu";
    public static string DEMO_LEVEL = "Demo Level";
    public static bool StartCutscene { get; set; }
    public static float MouseSensitivity { get; set; } = 1;
    public static GameState GameStatus { get; set; } = GameState.UNSTARTED;
    public static FinishState FinishStatus { get; set; } = FinishState.UNFINISHED;
    public static bool PauseAudio { get; set; } = false;

    public static MainCameraController ControlledCamera { get; private set; } = null;

    public static void SetControlledCamera(MainCameraController camera)
    {
        if (ControlledCamera != null)
        {
            ControlledCamera.Controlled = false;
        }

        camera.Controlled = true;
        ControlledCamera = camera;
    }

    public static BlobController ControlledBlob { get; private set; } = null;

    public static void SetControlledBlob(BlobController blob)
    {
        if (ControlledBlob != null)
        {
            ControlledBlob.Controlled = false;
        }

        blob.Controlled = true;
        ControlledBlob = blob;
    }
}
