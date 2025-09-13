using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishMenu : Menu
{
    public GameObject winText;
    public GameObject failBackground;

    public bool hasWon = false;

    public override void OnHide() {
        winText.SetActive(false);
        failBackground.SetActive(false);
    }

    public override void OnShow() {
        if (hasWon) {
            winText.SetActive(true);
        } else {
            failBackground.SetActive(true);
        }
    }

    public void RestartButton() {
        HideMenu();
        LevelStartupInfo.StartCutscene = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level");
    }

    public void QuitButton() {
        HideMenu();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}
