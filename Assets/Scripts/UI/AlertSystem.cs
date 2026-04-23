using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AlertSystem : MonoBehaviour
{
    private static readonly Color DEFAULT_COLOR = Color.white.WithAlpha(0);

    public float fadeInTime = 0.25f;
    private const string FADING_IN = "Fading in";
    public float displayTime = 3f;
    private const string DISPLAYING = "Displaying";
    public float fadeOutTime = 1f;
    private const string FADING_OUT = "Fading out";

    private int alertBoxCount;
    private Queue<(string, Color)> alertQueue = new();
    private TMP_Text[] alertBoxes;
    private StagedTimer[] timers;
    private int headIndex = 0;
    private Vector3 screenPosition;
    private Vector3 alertBoxHeightOffset;

    void Awake()
    {
        GameInfo.AlertSystem = this;
        float[] timerIntervals = new float[] {fadeInTime, displayTime, fadeOutTime};
        string[] stageNames = new string[] {FADING_IN, DISPLAYING, FADING_OUT};

        alertBoxes = GetComponentsInChildren<TMP_Text>();
        alertBoxCount = alertBoxes.Length;
        timers = new StagedTimer[alertBoxCount];

        for (int i = 0; i < alertBoxCount; i++) {
            alertBoxes[i].color = DEFAULT_COLOR;
            alertBoxes[i].text = "";
            timers[i] = new(timerIntervals, stageNames);
        }

        alertBoxHeightOffset = new(0 ,alertBoxes[0].rectTransform.rect.height, 0);
        screenPosition = alertBoxes[0].transform.position - alertBoxHeightOffset * (alertBoxCount - 1);
        ShiftAlertBoxes();
    }

    public void Send(string content, Color? color = null)
    {
        alertQueue.Enqueue((content, color == null ? DEFAULT_COLOR : (Color)color));
    }

    public void TryPostNext()
    {
        if (timers[headIndex].Complete() && alertQueue.Count > 0) {
            (string content, Color color) = alertQueue.Dequeue();

            timers[headIndex].Reset();
            alertBoxes[headIndex].text = content;
            alertBoxes[headIndex].color = color;
            headIndex = (headIndex + 1) % alertBoxCount;

            ShiftAlertBoxes();
        }
    }

    private void ShiftAlertBoxes()
    {
        int shift = alertBoxCount - headIndex;
        for (int i = 0; i < alertBoxCount; i++)
        {
            Vector3 offset = alertBoxHeightOffset * ((i + shift) % alertBoxCount);
            alertBoxes[i].transform.position = screenPosition + offset;
        }
    }

    private float AlphaFromTime(int i)
    {
        StageState timerState = timers[i].State;

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
            if (timers[i].Update(mode: TimerMode.Toggle))
            {
                alertBoxes[i].text = "";
                alertBoxes[i].color = DEFAULT_COLOR;
            } else {
                alertBoxes[i].color = alertBoxes[i].color.WithAlpha(AlphaFromTime(i));
            }
        }
    }
}