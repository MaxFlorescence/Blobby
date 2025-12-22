using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class AlertSystem : MonoBehaviour
{
    TMP_Text alertBox;
    Queue<(string, float)> alertQueue = new();

    float fadeTime = 0;

    void Awake()
    {
        alertBox = GetComponent<TMP_Text>();
        GameInfo.AlertSystem = this;
    }

    public void Send(string content, float duration)
    {
        alertQueue.Enqueue((content, duration));
    }

    void Update()
    {
        if (fadeTime > 0)
        {
            fadeTime -= Time.deltaTime;
            if (fadeTime <= 0)
            {
                fadeTime = 0f;
                alertBox.text = "";
            }
        }
        
        if (alertQueue.Count > 0)
        {
            (string content, float duration) = alertQueue.Dequeue();
            alertBox.text = content;
            fadeTime = duration;
        }
    }
}