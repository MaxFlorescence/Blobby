/// <summary>
///     A class defining the pause menu of the game.
/// </summary>
public class PauseMenu : Menu
{
    /// <summary>
    ///     The scene that should play when the resume button is clicked.
    /// </summary>
    public string playScene = GameInfo.DEMO_LEVEL;

    protected override void OnStart()
    {
        key = "e";
    }

    public void ResumeButton()
    {
        HideMenu();
    }

    public void OptionsButton()
    {
        HideMenu();
        GameInfo.OptionsMenu.ShowMenu(this);
    }

    public void RestartButton()
    {
        HideMenu();
        GameInfo.StartCutscene = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(playScene);
    }

    public void QuitButton()
    {
        QuitToMain();
    }
}
