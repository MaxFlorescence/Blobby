using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AlertSystem : MonoBehaviour
{
    private static readonly Color DEFAULT_COLOR = Color.white.WithAlpha(0);

    public float fadeInTime = 0.25f;
    public float displayTime = 3f;
    public float fadeOutTime = 1f;

    private int alertBoxCount;
    private float totalTime;
    private Queue<(string, Color)> alertQueue = new();
    private TMP_Text[] alertBoxes;
    private float[] timers;
    private int headIndex = 0;
    private Vector3 screenPosition;
    private Vector3 alertBoxHeightOffset;

    void Awake()
    {
        GameInfo.AlertSystem = this;
        totalTime = displayTime + fadeInTime + fadeOutTime;

        alertBoxes = GetComponentsInChildren<TMP_Text>();
        alertBoxCount = alertBoxes.Length;
        timers = new float[alertBoxCount];

        for (int i = 0; i < alertBoxCount; i++) {
            alertBoxes[i].color = DEFAULT_COLOR;
            alertBoxes[i].text = "";
            timers[i] = 0f;
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
        if (timers[headIndex] == 0 && alertQueue.Count > 0) {
            (string content, Color color) = alertQueue.Dequeue();

            timers[headIndex] = totalTime;
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
        if (timers[i] <= fadeOutTime)
        {
            return timers[i] / fadeOutTime;
        }
        else if (timers[i] <= fadeOutTime + displayTime)
        {
            return 1f;
        }
        else
        {
            return 1f - (timers[i] - fadeOutTime - displayTime) / fadeInTime;
        }
    }

    void Update()
    {
        TryPostNext();

        for (int i = 0; i < alertBoxCount; i++) {
            if (timers[i] > 0)
            {
                alertBoxes[i].color = alertBoxes[i].color.WithAlpha(AlphaFromTime(i));
                timers[i] -= Time.deltaTime;
                if (timers[i] <= 0)
                {
                    timers[i] = 0f;
                    alertBoxes[i].text = "";
                    alertBoxes[i].color = DEFAULT_COLOR;
                }
            }
        }
    }
}