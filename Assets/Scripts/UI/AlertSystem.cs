using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class AlertSystem : MonoBehaviour
{
    private static readonly Color DEFAULT_COLOR = Color.white.WithAlpha(0);

    public float fadeInTime = 0.25f;
    public float fadeOutTime = 1f;

    private TMP_Text alertBox;
    private Queue<(string, float, Color)> alertQueue = new();
    private Color textColor = DEFAULT_COLOR;
    private float displayTime = 0f;
    private float timer = 0f;

    void Awake()
    {
        alertBox = GetComponent<TMP_Text>();
        alertBox.color = textColor;
        alertBox.text = "";
        GameInfo.AlertSystem = this;
    }

    public void Send(string content, float duration = 3f, Color? color = null)
    {
        alertQueue.Enqueue((content, duration, color == null ? DEFAULT_COLOR : (Color)color));
    }

    private float AlphaFromTime()
    {
        if (timer <= fadeOutTime)
        {
            return timer / fadeOutTime;
        }
        else if (timer <= fadeOutTime + displayTime)
        {
            return 1f;
        }
        else
        {
            return 1f - (timer - fadeOutTime - displayTime) / fadeInTime;
        }
    }

    void Update()
    {
        if (timer > 0)
        {
            alertBox.color = textColor.WithAlpha(AlphaFromTime());
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0f;
                alertBox.text = "";
                alertBox.color = textColor = DEFAULT_COLOR;
            }
        }
        
        if (alertQueue.Count > 0)
        {
            (string content, float duration, Color color) = alertQueue.Dequeue();
            alertBox.text = content;
            displayTime = duration;
            textColor = color;
            timer = displayTime + fadeInTime + fadeOutTime;
        }
    }
}