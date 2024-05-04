using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : Menu
{
    public Slider mouseSlider;
    public GameObject mouseInfo;
    private TextMeshProUGUI mouseTMP;

    private void Start() {
        mouseTMP = mouseInfo.GetComponent<TextMeshProUGUI>();
        mouseSlider.value = LevelStartupInfo.MouseSensitivity;

        HideMenu();
    }

    public void MouseSensitivitySlider() {
        LevelStartupInfo.MouseSensitivity = mouseSlider.value;
        SetMouseTMPInfo();
    }

    private void SetMouseTMPInfo() {
        mouseTMP.SetText("Mouse Sensitivity: " + (int)(100*mouseSlider.value) + "%");
    }

    public void ResumeButton() {
        HideMenu();
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
