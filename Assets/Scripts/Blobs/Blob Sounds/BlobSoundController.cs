using UnityEngine;

/// <summary>
///     A class for playing the noises that a blob makes.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BlobSoundController : MonoBehaviour
{
    private AudioSource audioSource;
    
    /// <summary>
    ///     The different sound families that the blob can play.
    /// </summary>
    private BlobSoundFamiliesStruct soundFamilies;
    
    /// <summary>
    ///     The timer on which the background sound family is played.
    /// </summary>
    private readonly Timer loopTimer = new();

    void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (soundFamilies.background.IsNone) return;

        if (loopTimer.Update()) {
            audioSource.PlayOneShot(soundFamilies.background);
        }
    }

    /// <summary>
    ///     Play a random collision noise with a random pitch.
    /// </summary>
    public void CollideSound() {
        if (GameInfo.StartCutscene || soundFamilies.collision.IsNone) return;
        
        audioSource.PlayOneShot(soundFamilies.collision);
    }

    /// <summary>
    ///     Load the different sound clips that can be played by this <tt>BlobSoundController</tt>,
    ///     and set their volumes and pitches.
    /// </summary>
    /// <param name="soundData">
    ///     The data defining the sound clips to load and the parameters with which to play them.
    /// </param>
    public void SetClips(BlobSoundFamiliesStruct soundFamilies)
    {
        audioSource.Stop();

        if (!this.soundFamilies.fromTransition?.IsNone ?? false)
            audioSource.PlayOneShot(this.soundFamilies.fromTransition);

        this.soundFamilies = soundFamilies;
        
        if (!this.soundFamilies.background.IsFamily)
        {
            // TODO: assumes backgroundFamily is not a sound family
            loopTimer.SetInterval(this.soundFamilies.background.RandomClip.length);
            loopTimer.Skip();
        }

        if (!this.soundFamilies.toTransition.IsNone)
            audioSource.PlayOneShot(this.soundFamilies.toTransition);
    }
}