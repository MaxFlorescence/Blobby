using UnityEngine;
using System;

public enum BlobLight
{
    InventoryIcon = 0,
    MaterialGlow = 1
}

public class BlobLightController
{
    private readonly int TYPE_COUNT = Enum.GetNames(typeof(BlobLight)).Length;
    private bool[] defaultStates;
    private Light[] lights;

    public BlobLightController()
    {
        defaultStates = new bool[TYPE_COUNT];
        lights = new Light[TYPE_COUNT];
    }

    public void AddLight(BlobLight blobLight, Light light, bool defaultState)
    {
        defaultStates[(int)blobLight] = defaultState;
        lights[(int)blobLight] = light;
    }

    public void SetLight(BlobLight blobLight, bool? enable, bool save = false)
    {
        int index = (int)blobLight;
        enable ??= !defaultStates[index];

        lights[index].enabled = (bool)enable;

        if (save)
        {
            defaultStates[index] = (bool)enable;
        }
    }

    public void ResetLight(BlobLight blobLight)
    {
        lights[(int)blobLight].enabled = defaultStates[(int)blobLight];
    }
}