using UnityEngine;

public class FinishMenu : Menu
{
    public GameObject winText;
    public GameObject failBackground;

    public bool hasWon = false;

    protected override void OnStart()
    {
        menuPausesAudio = false;
    }

    protected override void OnHide() {
        winText.SetActive(false);
        failBackground.SetActive(false);
    }

    protected override void OnShow() {
        if (hasWon) {
            winText.SetActive(true);
        } else {
            failBackground.SetActive(true);
        }
    }

    public void RestartButton() {
        HideMenu();
        LevelStartupInfo.StartCutscene = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(LevelStartupInfo.DEMO_LEVEL);
    }

    public void QuitButton() {
        HideMenu();
        UnityEngine.SceneManagement.SceneManager.LoadScene(LevelStartupInfo.MAIN_MENU);
    }
}
