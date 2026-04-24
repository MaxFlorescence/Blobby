using UnityEngine;

/// <summary>
///     A class for controlling a WASD+Space graphic on screen with live button presses.
/// </summary>
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
        if (Input.GetKeyDown(KeyCode.K)) {
            overlay.SetActive(!overlay.activeSelf);
        }

        wPressed.SetActive(Input.GetKey(KeyCode.W));
        aPressed.SetActive(Input.GetKey(KeyCode.A));
        sPressed.SetActive(Input.GetKey(KeyCode.S));
        dPressed.SetActive(Input.GetKey(KeyCode.D));
        spacePressed.SetActive(Input.GetKey(KeyCode.Space));
    }
}
