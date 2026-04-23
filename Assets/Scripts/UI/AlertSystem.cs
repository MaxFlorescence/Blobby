using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
///     A class for displaying alerts on the screen to the player.
/// </summary>
public class AlertSystem : MonoBehaviour
{
    // ---------------------------------------------------------------------------------------------
    // PARAMETERS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The default color of alert text.
    /// </summary>
    private static readonly Color DEFAULT_COLOR = Color.white.WithAlpha(0);
    /// <summary>
    ///     The amount of time it takes for an alert to fade in once its received.
    /// </summary>
    public float fadeInTime = 0.25f;
    private const string FADING_IN = "Fading in";
    /// <summary>
    ///     The amount of time for which to display an alert at full opacity.
    /// </summary>
    public float displayTime = 3f;
    private const string DISPLAYING = "Displaying";
    /// <summary>
    ///     The amount of time it takes for an alert to fade out once its done playing.
    /// </summary>
    public float fadeOutTime = 1f;
    private const string FADING_OUT = "Fading out";

    // ---------------------------------------------------------------------------------------------
    // QUEUEING
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The queue of alerts that need to be displayed and in what color to display them.
    /// </summary>
    private Queue<(string, Color)> alertQueue = new();
    /// <summary>
    ///     StagedTimers corresponding to each alert box that keeps track of its fading status.
    /// </summary>
    private StagedTimer[] alertBoxTimers;
    private int alertBoxCount;
    /// <summary>
    ///     The index of the alert box that is currently at the highest position on-screen.
    /// </summary>
    private int headIndex = 0;

    // ---------------------------------------------------------------------------------------------
    // DISPLAYING
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The text boxes available to display alerts in.
    /// </summary>
    private TMP_Text[] alertBoxes;
    /// <summary>
    ///     The position at which to display the head alert box.
    /// </summary>
    private Vector3 screenPosition;
    /// <summary>
    ///     A vector with y component equal to the height of an alert box and zero x/z components.
    /// </summary>
    private Vector3 alertBoxHeightOffset;

    void Awake()
    {
        GameInfo.AlertSystem = this;
        float[] timerIntervals = new float[] {fadeInTime, displayTime, fadeOutTime};
        string[] stageNames = new string[] {FADING_IN, DISPLAYING, FADING_OUT};

        alertBoxes = GetComponentsInChildren<TMP_Text>();
        alertBoxCount = alertBoxes.Length;
        alertBoxTimers = new StagedTimer[alertBoxCount];

        for (int i = 0; i < alertBoxCount; i++) {
            alertBoxes[i].color = DEFAULT_COLOR;
            alertBoxes[i].text = "";
            alertBoxTimers[i] = new(timerIntervals, stageNames);
        }

        // assumes all alert boxes are the same height
        alertBoxHeightOffset = new(0, alertBoxes[0].rectTransform.rect.height, 0);
        // assumes the first found alert box started off at the bottom-most position
        screenPosition = alertBoxes[0].transform.position - alertBoxHeightOffset * (alertBoxCount - 1);
        ShiftAlertBoxes();
    }

    /// <summary>
    ///     Enqueue an alert to display to the player.
    /// </summary>
    /// <param name="content">
    ///     The content of the alert.
    /// </param>
    /// <param name="color">
    ///     The color of the alert. If <tt>null</tt>, then the default color will be used.
    /// </param>
    public void Send(string content, Color? color = null)
    {
        alertQueue.Enqueue((content, color ?? DEFAULT_COLOR));
    }

    /// <summary>
    ///     If the current head alert is done and there are more alerts queued, then replaces it
    ///     with the next queued alert and shifts the alert boxes.
    /// </summary>
    public void TryPostNext()
    {
        if (alertBoxTimers[headIndex].Complete() && alertQueue.Count > 0) {
            (string content, Color color) = alertQueue.Dequeue();

            alertBoxTimers[headIndex].Reset();
            alertBoxes[headIndex].text = content;
            alertBoxes[headIndex].color = color;
            headIndex = (headIndex + 1) % alertBoxCount;

            ShiftAlertBoxes();
        }
    }

    /// <summary>
    ///     Moves all alert boxes so that the head alert box is at <tt>screenPosition</tt> and the
    ///     rest are below it, in order and wrapping around.
    /// </summary>
    private void ShiftAlertBoxes()
    {
        int shift = alertBoxCount - headIndex;
        for (int i = 0; i < alertBoxCount; i++)
        {
            Vector3 offset = alertBoxHeightOffset * ((i + shift) % alertBoxCount);
            alertBoxes[i].transform.position = screenPosition + offset;
        }
    }

    /// <returns>
    ///     What the alpha value of the color of the alert box at the given index should be at the
    ///     current time. 
    /// </returns>
    private float AlphaFromTime(int i)
    {
        StageState timerState = alertBoxTimers[i].State;

        return timerState.stageName switch
        {
            FADING_IN => timerState.progress,
            DISPLAYING  => 1,
            FADING_OUT => 1 - timerState.progress,
            _ => 0
        };
    }

    void Update()
    {
        TryPostNext();

        for (int i = 0; i < alertBoxCount; i++) {
            if (alertBoxTimers[i].Update(mode: TimerMode.Toggle))
            {
                alertBoxes[i].text = "";
                alertBoxes[i].color = DEFAULT_COLOR;
            } else {
                alertBoxes[i].color = alertBoxes[i].color.WithAlpha(AlphaFromTime(i));
            }
        }
    }
}