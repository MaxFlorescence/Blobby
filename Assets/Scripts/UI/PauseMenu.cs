using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : Menu
{
    public Slider mouseSlider;
    public GameObject mouseInfo;
    private TextMeshProUGUI mouseTMP;

    protected override void OnStart()
    {
        key = "e";
        mouseTMP = mouseInfo.GetComponent<TextMeshProUGUI>();
        mouseSlider.value = LevelStartupInfo.MouseSensitivity;
    }

    public void MouseSensitivitySlider()
    {
        LevelStartupInfo.MouseSensitivity = mouseSlider.value;
        SetMouseTMPInfo();
    }

    private void SetMouseTMPInfo()
    {
        mouseTMP.SetText("Mouse Sensitivity: " + (int)(100*mouseSlider.value) + "%");
    }

    public void ResumeButton()
    {
        HideMenu();
    }

    public void RestartButton()
    {
        HideMenu();
        LevelStartupInfo.StartCutscene = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(LevelStartupInfo.DEMO_LEVEL);
    }

    public void QuitButton()
    {
        HideMenu();
        UnityEngine.SceneManagement.SceneManager.LoadScene(LevelStartupInfo.MAIN_MENU);
    }
}
