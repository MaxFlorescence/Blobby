using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : Menu
{
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
