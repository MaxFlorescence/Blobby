using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : Menu
{
    public string playScene = GameInfo.DEMO_LEVEL;
    public Slider mouseSlider;
    public GameObject mouseInfo;
    private TextMeshProUGUI mouseTMP;

    protected override void OnStart()
    {
        key = "e";
        mouseTMP = mouseInfo.GetComponent<TextMeshProUGUI>();
        mouseSlider.value = GameInfo.MouseSensitivity;
    }

    public void MouseSensitivitySlider()
    {
        GameInfo.MouseSensitivity = mouseSlider.value;
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
        GameInfo.StartCutscene = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(playScene);
    }

    public void QuitButton()
    {
        QuitToMain();
    }
}
