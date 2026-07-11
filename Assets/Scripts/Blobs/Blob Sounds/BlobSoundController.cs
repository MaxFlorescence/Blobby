using UnityEngine;

/// <summary>
///     A class for playing the noises that a blob makes.
/// </summary>
public class BlobSoundController : MonoBehaviour
{
    public AudioSource oneShotAudioSource;
    public AudioSource backgroundAudioSource;
    
    /// <summary>
    ///     The different sound families that the blob can play.
    /// </summary>
    private BlobSoundFamiliesStruct soundFamilies;
    
    /// <summary>
    ///     The timer on which the background sound family is played.
    /// </summary>
    private readonly Timer loopTimer = new();

    void Update()
    {
        if (soundFamilies.background.IsNone) return;

        if (loopTimer.Update()) {
            backgroundAudioSource.PlayOneShot(soundFamilies.background);
        }
    }

    /// <summary>
    ///     Play a random collision noise with a random pitch.
    /// </summary>
    public void CollideSound() {
        if (GameInfo.StartCutscene || soundFamilies.collision.IsNone) return;
        
        oneShotAudioSource.PlayOneShot(soundFamilies.collision);
    }

    /// <summary>
    ///     Play a random damage noise with a random pitch.
    /// </summary>
    public void DamageSound() {
        if (GameInfo.StartCutscene || soundFamilies.damage.IsNone) return;
        
        oneShotAudioSource.PlayOneShot(soundFamilies.damage);
    }

    /// <summary>
    ///     Play a random death noise with a random pitch.
    /// </summary>
    public void DeathSound() {
        if (GameInfo.StartCutscene || soundFamilies.death.IsNone) return;
        
        oneShotAudioSource.PlayOneShot(soundFamilies.death);
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
        oneShotAudioSource.Stop();
        backgroundAudioSource.Stop();

        if (!this.soundFamilies.fromTransition?.IsNone ?? false)
            oneShotAudioSource.PlayOneShot(this.soundFamilies.fromTransition);

        this.soundFamilies = soundFamilies;
        
        if (!this.soundFamilies.background.IsFamily)
        {
            // TODO: assumes backgroundFamily is not a sound family
            loopTimer.SetInterval(this.soundFamilies.background.RandomClip.length);
            loopTimer.Skip();
        }

        if (!this.soundFamilies.toTransition.IsNone)
            oneShotAudioSource.PlayOneShot(this.soundFamilies.toTransition);
    }
}