using TMPro;
using UnityEngine;

/// <summary>
///     A class defining the menu that the player sees upon finishing a level.
/// </summary>
public class FinishMenu : Menu
{
    // ---------------------------------------------------------------------------------------------
    // STATE
    // ---------------------------------------------------------------------------------------------
    public bool hasWon = false;
    /// <summary>
    ///     The scene to play upon clicking the "Restart Level" button.
    /// </summary>
    public string playScene = GameInfo.DEMO_LEVEL;

    // ---------------------------------------------------------------------------------------------
    // GRAPHICS
    // ---------------------------------------------------------------------------------------------
    public TMP_Text finishText;
    /// <summary>
    ///     Text to display on screen when the player loses.
    /// </summary>
    private const string LOSE_TEXT = "You were slain!";
    /// <summary>
    ///     Text to display on screen when the player wins.
    /// </summary>
    private const string WIN_TEXT = "You prevailed!";
    /// <summary>
    ///     Image to display on screen when the player loses.
    /// </summary>
    public GameObject failBackground;
    /// <summary>
    ///     Image to display on screen when the player wins.
    /// </summary>
    public GameObject winBackground;

    protected override void OnStart()
    {
        menuPausesAudio = false;
    }

    public void ShowMenu(bool hasWon)
    {
        this.hasWon = hasWon;
        ShowMenu();
    }

    protected override void OnUpdate() {
        if (!GameInfo.DebugMode) return;

        if (Input.GetKey(KeyCode.F))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ShowMenu(false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ShowMenu(true);
            }
        }
    }

    protected override void OnHide() {
        finishText.gameObject.SetActive(false);
        failBackground.SetActive(false);
        winBackground.SetActive(false);
    }

    protected override void OnShow() {
        finishText.gameObject.SetActive(true);

        if (hasWon) {
            finishText.text = WIN_TEXT;
            GameInfo.FinishStatus = FinishState.Won;
            winBackground.SetActive(true);
        } else {
            finishText.text = LOSE_TEXT;
            GameInfo.FinishStatus = FinishState.Lost;
            failBackground.SetActive(true);
        }
    }

    public void RestartButton() {
        HideMenu();
        GameInfo.StartCutscene = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(playScene);
    }

    public void QuitButton() {
        QuitToMain();
    }
}
