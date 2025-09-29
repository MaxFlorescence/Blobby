using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject graphics;
    protected bool menuActive = false;
    protected string key = null;
    protected bool menuPausesAudio = true;

    void Start()
    {
        OnStart();
        HideMenu();
    }

    virtual protected void OnStart() {}

    protected void Update()
    {
        if (key is not null && Input.GetKeyDown(key))
        {
            if (menuActive)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }
        }
    }

    public void HideMenu() {
        menuActive = false;
        LevelStartupInfo.GameIsPaused = false;
        LevelStartupInfo.PauseAudio = false;

        Time.timeScale = 1;
        graphics.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnHide();
    }

    virtual protected void OnHide() {}

    public void ShowMenu() {
        if (!LevelStartupInfo.GameIsPaused)
        {
            menuActive = true;
            LevelStartupInfo.GameIsPaused = true;
            if (menuPausesAudio) LevelStartupInfo.PauseAudio = true;
            

            Time.timeScale = 0;
            graphics.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            OnShow();
        }
    }

    virtual protected void OnShow() {}
}
