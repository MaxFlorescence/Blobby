using TMPro;
using UnityEngine;

class DebugController : MonoBehaviour
{
    public TMP_Text debugWindow;

    void Update()
    {
        UpdateDebugInfo();

        if (Input.GetKeyDown(KeyCode.Slash)) {
            GameInfo.DebugMode = !GameInfo.DebugMode;
            GameInfo.AlertSystem.Send("Debug mode is " + (GameInfo.DebugMode ? "on" : "off"));
        }

        if (!GameInfo.DebugMode) return;
        
        if (Input.GetKeyDown(KeyCode.G))
            GameInfo.ControlledBlob.ToggleGhostMode();

        if (Input.GetKey(KeyCode.M))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                GameInfo.ControlledBlob.SetBlobMaterials(BlobMaterials.WATER);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                GameInfo.ControlledBlob.SetBlobMaterials(BlobMaterials.LAVA);
        }
    }

    private void UpdateDebugInfo()
    {
        debugWindow.text = GameInfo.DebugMode ? "<mspace=0.75em>" + GameInfo.ControlledBlob.ToString() : "";
    }

    public void Toggle()
    {
        GameInfo.DebugMode = !GameInfo.DebugMode;
    }
}