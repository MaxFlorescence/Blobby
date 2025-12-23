using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class AlertSystem : MonoBehaviour
{
    public float fadeInTime = 0.25f;
    public float fadeOutTime = 1f;

    private TMP_Text alertBox;
    private Queue<(string, float, Color)> alertQueue = new();
    private Color textColor = Color.white;
    private string textContent = "";
    private float displayTime = 0;
    private float timer = 0f;

    void Awake()
    {
        alertBox = GetComponent<TMP_Text>();
        alertBox.color = textColor;
        alertBox.text = textContent;
        GameInfo.AlertSystem = this;
    }

    public void Send(string content, float duration)
    {
        alertQueue.Enqueue((content, duration, Color.white));
    }

    public void SendColored(string content, float duration, Color color)
    {
        alertQueue.Enqueue((content, duration, color));
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
            return 1 - (timer - fadeOutTime - displayTime) / fadeInTime;
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