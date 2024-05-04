using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : Menu
{    
    public Slider mouseSlider;
    public GameObject mouseInfo;
    private TextMeshProUGUI mouseTMP;

    private void Start() {
        mouseTMP = mouseInfo.GetComponent<TextMeshProUGUI>();
        mouseSlider.value = LevelStartupInfo.MouseSensitivity;

        ShowMenu();
    }

    public void MouseSensitivitySlider() {
        LevelStartupInfo.MouseSensitivity = mouseSlider.value;
        SetMouseTMPInfo();
    }

    private void SetMouseTMPInfo() {
        mouseTMP.SetText("Mouse Sensitivity: " + (int)(100*mouseSlider.value) + "%");
    }

    public void PlayButton() {
        LevelStartupInfo.StartCutscene = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level");
    }

    public void QuitButton() {
        Application.Quit();
    }
}
