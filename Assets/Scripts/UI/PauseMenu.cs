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
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameInfo.DEMO_LEVEL);
    }

    public void QuitButton()
    {
        QuitToMain();
    }
}
