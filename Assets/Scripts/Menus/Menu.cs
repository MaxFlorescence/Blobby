using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject instance;

    private void Start() {
        HideMenu();
    }

    public void HideMenu() {
        Time.timeScale = 1;
        instance.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnHide();
    }

    virtual public void OnHide() {}

    public void ShowMenu() {
        Time.timeScale = 0;
        instance.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnShow();
    }

    virtual public void OnShow() {}
}
