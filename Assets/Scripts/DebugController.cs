using TMPro;
using UnityEngine;

class DebugController : MonoBehaviour
{
    public TMP_Text debugWindow;

    void Update()
    {
        UpdateDebugInfo();

        if (Input.GetKeyDown(KeyCode.Slash)) {
            Toggle();
            GameInfo.AlertSystem.Send("Debug mode is " + (GameInfo.DebugMode ? "on" : "off"));
        }
    }

    private void UpdateDebugInfo()
    {
        debugWindow.text = GameInfo.DebugMode ? "<mspace=0.75em>" + GameInfo.ControlledBlob.ToString() : "";
    }

    public void Toggle()
    {
        GameInfo.DebugMode = !GameInfo.DebugMode;

        GameInfo.ControlledBlob.SetAtomsVisible(GameInfo.DebugMode);
    }
}