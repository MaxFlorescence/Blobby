using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject graphics;
    protected bool menuActive = false;
    protected string key = null;
    protected bool menuPausesAudio = true;
    protected bool isSubmenu = false;

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

        OnUpdate();
    }

    virtual protected void OnUpdate() {}

    public void HideMenu() {
        if (!isSubmenu)
        {
            GameInfo.GameStatus = GameState.Playing;
            GameInfo.PauseAudio = false;
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        menuActive = false;
        graphics.SetActive(false);
        OnHide();
    }

    virtual protected void OnHide() {}

    public void ShowMenu() {
        if (!isSubmenu)
        {
            if (GameInfo.GameStatus == GameState.Paused) return;
            
            GameInfo.GameStatus = GameState.Paused;
            if (menuPausesAudio) GameInfo.PauseAudio = true;
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        menuActive = true;
        graphics.SetActive(true);
        OnShow();
    }

    protected void QuitToMain()
    {
        HideMenu();
        GameInfo.GameStatus = GameState.Unstarted;
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameInfo.MAIN_MENU);
    }

    virtual protected void OnShow() {}
}
