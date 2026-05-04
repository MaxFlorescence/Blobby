using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;
using IEnumerator = System.Collections.IEnumerator;
using System.IO;
using Unity.Mathematics;

public static class Extensions
{
    /// <summary>
    ///     Plays an AudioClip, and scales the AudioSource pitch randomly between the given bounds.
    /// </summary>
    /// <param name="audioClip">
    ///     The clip to play.
    /// </param>
    /// <param name="pitchBounds">
    ///     The minimum and maximum pitches that the AudioClip can play at.
    /// </param>
    public static void PlayRandomPitchOneShot(this AudioSource audioSource, AudioClip audioClip, Vector2? pitchBounds = null)
    {
        if (audioClip == null) return;
        
        float originalPitch = audioSource.pitch;
        audioSource.pitch = Random.Range(pitchBounds?.x ?? 1, pitchBounds?.y ?? 1);

        audioSource.PlayOneShot(audioClip);
        
        audioSource.pitch = originalPitch;
    }

    public static T Clone<T>(this T cloneMe) where T : struct
    {
        return cloneMe;
    }
}

class Utilities : MonoBehaviour
{

    public static int CountNames<T>() where T : Enum
    {
        return Enum.GetNames(typeof(T)).Length;
    }
}