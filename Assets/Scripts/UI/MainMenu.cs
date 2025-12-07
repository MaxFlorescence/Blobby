using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : Menu
{
    public string playScene = GameInfo.DEMO_LEVEL;
    public bool startCutsceneOnPlay = true;

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
        GameInfo.StartCutscene = startCutsceneOnPlay;
        GameInfo.GameStatus = GameState.PLAYING;
        GameInfo.FinishStatus = FinishState.UNFINISHED;
        UnityEngine.SceneManagement.SceneManager.LoadScene(playScene);
    }

    public void QuitButton() {
        Application.Quit();
    }
}
