using UnityEngine;

/// <summary>
///      A class from which all game menus derive.
/// </summary>
public class Menu : MonoBehaviour
{
    /// <summary>
    ///     The parent gameobject of every element of this menu.
    /// </summary>
    public GameObject graphics;
    /// <summary>
    ///     <tt>True</tt> iff the menu is currently being shown.
    /// </summary>
    protected bool menuActive = false;
    /// <summary>
    ///     (Optional) the key that shows/hides this menu.
    /// </summary>
    protected string key = null;
    /// <summary>
    ///     Iff <tt>true</tt>, game audio is paused when this menu is shown.
    /// </summary>
    protected bool menuPausesAudio = true;
    /// <summary>
    ///     Iff <tt>true</tt>, this menu will not pause/unpause the game when shown/hidden.
    /// </summary>
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

    /// <summary>
    ///     Hide menu and resume game.
    /// </summary>
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

    /// <summary>
    ///     Pause game and show menu.
    /// </summary>
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

    virtual protected void OnShow() {}

    /// <summary>
    ///     Loads the main menu scene.
    /// </summary>
    protected void QuitToMain()
    {
        HideMenu();
        GameInfo.GameStatus = GameState.Unstarted;
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameInfo.MAIN_MENU);
    }
}
