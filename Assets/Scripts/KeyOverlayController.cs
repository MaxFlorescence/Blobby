using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyOverlayController : MonoBehaviour
{
    public GameObject overlay;
    public GameObject wPressed;
    public GameObject aPressed;
    public GameObject sPressed;
    public GameObject dPressed;
    public GameObject spacePressed;

    void Start() {
        overlay.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyDown("k")) {
            overlay.SetActive(!overlay.activeSelf);
        }

        wPressed.SetActive(Input.GetKey("w"));
        aPressed.SetActive(Input.GetKey("a"));
        sPressed.SetActive(Input.GetKey("s"));
        dPressed.SetActive(Input.GetKey("d"));
        spacePressed.SetActive(Input.GetKey("space"));
    }
}
