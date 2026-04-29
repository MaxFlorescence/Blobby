using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     A class defining the main menu of the game.
/// </summary>
public class MainMenu : Menu
{
    /// <summary>
    ///     The scene to play upon clicking the "Play" button.
    /// </summary>
    public string playScene = GameInfo.DEMO_LEVEL;
    /// <summary>
    ///     Iff <tt>true</tt>, the start level cutscene for <tt>playScene</tt> will play when the
    ///     menu is shown.
    /// </summary>
    public bool startCutsceneOnPlay = true;

    private void Start() {
        ShowMenu();
    }

    public void PlayButton() {
        GameInfo.StartCutscene = startCutsceneOnPlay;
        GameInfo.GameStatus = GameState.Playing;
        GameInfo.FinishStatus = FinishState.Unfinished;
        UnityEngine.SceneManagement.SceneManager.LoadScene(playScene);
    }

    public void OptionsButton()
    {
        HideMenu();
        GameInfo.OptionsMenu.ShowMenu(this);
    }

    public void QuitButton() {
        Application.Quit();
    }
}
