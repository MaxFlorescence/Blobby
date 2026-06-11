using UnityEngine;

/// <summary>
///     A class defining extensions for <tt>AudioSource</tt>s.
/// </summary>
public static class AudioSourceExtensions
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
    /// <param name="volume">
    ///     The volume at which to play the AudioClip at.
    /// </param>
    public static void PlayRandomPitchOneShot(this AudioSource audioSource, AudioClip audioClip,
                                              Vector2? pitchBounds = null, float? volume = null)
    {
        if (audioClip == null) return;
        
        float originalPitch = audioSource.pitch;
        audioSource.pitch = Random.Range(pitchBounds?.x ?? 1, pitchBounds?.y ?? 1);

        audioSource.PlayOneShot(audioClip, volume ?? 1);
        
        audioSource.pitch = originalPitch;
    }

    /// <summary>
    ///     Plays an AudioClip as specified by the given sound family.
    /// </summary>
    /// <param name="soundFamily">
    ///     The sound family specifying the audio clip, pitch, and volume to play.
    /// </param>
    public static void PlayOneShot(this AudioSource audioSource, SoundFamily soundFamily)
    {
        audioSource.PlayRandomPitchOneShot(
            soundFamily.RandomClip,
            soundFamily.PitchBounds,
            soundFamily.Volume
        );
    }
}