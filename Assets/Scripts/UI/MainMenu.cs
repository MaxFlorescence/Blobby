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

/// <summary>
///     The slider for the mouse sensitivity option.
/// </summary>
    public Slider mouseSlider;
/// <summary>
///     The text displaying the current mouse sensitivity option.
/// </summary>
    public TextMeshProUGUI mouseInfoText;

    private void Start() {
        mouseSlider.value = GameInfo.MouseSensitivity;

        ShowMenu();
    }

    public void MouseSensitivitySlider() {
        GameInfo.MouseSensitivity = mouseSlider.value;
        SetMouseTMPInfo();
    }

    private void SetMouseTMPInfo() {
        mouseInfoText.SetText("Mouse Sensitivity: " + (int)(100*mouseSlider.value) + "%");
    }

    public void PlayButton() {
        GameInfo.StartCutscene = startCutsceneOnPlay;
        GameInfo.GameStatus = GameState.Playing;
        GameInfo.FinishStatus = FinishState.Unfinished;
        UnityEngine.SceneManagement.SceneManager.LoadScene(playScene);
    }

    public void QuitButton() {
        Application.Quit();
    }
}
