using UnityEngine;

class DebugControls : MonoBehaviour
{
    void Update()
    {
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

    public void Toggle()
    {
        GameInfo.DebugMode = !GameInfo.DebugMode;
    }
}