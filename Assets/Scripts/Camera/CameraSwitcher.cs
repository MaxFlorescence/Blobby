using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera mainCamera;
    public Camera secondaryCamera;
    public GameObject mainOverlay = null;
    public GameObject secondaryOverlay = null;
    private bool mainActive = true;
    private CutsceneCameraController cutsceneController;

    void Start()
    {
        cutsceneController = secondaryCamera.GetComponent<CutsceneCameraController>();

        // this flag is only true when starting the game from the main menu
        if (GameInfo.StartCutscene)
        {
            SwitchCamera(false);

            cutsceneController.BeginCutscene();
        }
        else
        {
            EndCutscene();
        }
    }

    void Update()
    {
        // press space to skip cutscene
        if ((GameInfo.StartCutscene && Input.GetButtonUp("Jump"))
            || cutsceneController.Finished())
        {
            EndCutscene();
        }
    }

    void EndCutscene()
    {
        cutsceneController.Reset();
        SwitchCamera(true);
        GameInfo.StartCutscene = false;
    }

    private void SwitchCamera(bool main)
    {
        mainActive = main;
        mainCamera.gameObject.SetActive(main);
        secondaryCamera.gameObject.SetActive(!main);
        if (mainOverlay != null)
            mainOverlay.SetActive(main);
        if (secondaryOverlay != null)
            secondaryOverlay.SetActive(!main);
    }

    public Camera GetMainCamera()
    {
        return mainCamera;
    }
    
    public bool IsMainCameraActive()
    {
        return mainActive;
    }
}
