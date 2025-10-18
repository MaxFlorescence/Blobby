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
        mouseSlider.value = GameInfo.MouseSensitivity;

        ShowMenu();
    }

    public void MouseSensitivitySlider() {
        GameInfo.MouseSensitivity = mouseSlider.value;
        SetMouseTMPInfo();
    }

    private void SetMouseTMPInfo() {
        mouseTMP.SetText("Mouse Sensitivity: " + (int)(100*mouseSlider.value) + "%");
    }

    public void PlayButton() {
        GameInfo.StartCutscene = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameInfo.DEMO_LEVEL);
    }

    public void QuitButton() {
        Application.Quit();
    }
}
