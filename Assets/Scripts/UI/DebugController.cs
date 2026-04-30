using TMPro;
using UnityEngine;

/// <summary>
///     A class that controls the debug mode of the game.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
class DebugController : MonoBehaviour
{
    /// <summary>
    ///     The UI element containing all the debug information that is displayed to the player.
    /// </summary>
    private TextMeshProUGUI debugInfo;

    public void Start()
    {
        debugInfo = GetComponent<TextMeshProUGUI>();
    }

    public void Update()
    {
        debugInfo.SetText(MakeDebugText());

        if (Input.GetKeyDown(KeyCode.Slash)) {
            GameInfo.DebugMode = !GameInfo.DebugMode;
            GameInfo.AlertSystem.Send("Debug mode is " + (GameInfo.DebugMode ? "on" : "off"));
        }
    }

    private string MakeDebugText()
    {
        if (!GameInfo.DebugMode) return "";

        return $"<mspace=0.75em>{GameInfo.ControlledBlob}";
    }
}