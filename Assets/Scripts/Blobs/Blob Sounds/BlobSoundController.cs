using System.IO;
using UnityEngine;

/// <summary>
///     A class for playing the noises that a blob makes.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BlobSoundController : MonoBehaviour
{
    //----------------------------------------------------------------------------------------------
    // AUDIO
    //----------------------------------------------------------------------------------------------
    private AudioSource audioSource;

    //----------------------------------------------------------------------------------------------
    // COLLISION
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The different possible sounds that will play when <tt>CollideSound()</tt> is called.
    /// </summary>
    private AudioClip[] collisionSounds;

    /// <summary>
    ///     The maximum number of collision sounds that can be selected from.
    /// </summary>
    private const int MAX_CLIP_COUNT = 8;

    /// <summary>
    ///     The current number of collision sounds that can be selected from.
    /// </summary>
    private int collisionClipCount = 8;

    /// <summary>
    ///     The minimum and maximum pitches that collision sounds can be played with.
    /// </summary>
    private Vector2 collidePitchBounds;

    /// <summary>
    ///     The volume at which collisions sounds are played.
    /// </summary>
    private float collideVolume = 0;
    
    //----------------------------------------------------------------------------------------------
    // BACKGROUND
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The sound that is played continuously on loop.
    /// </summary>
    private AudioClip backgroundSound;
    
    /// <summary>
    ///     The timer on which the background sound is played.
    /// </summary>
    private readonly Timer loopTimer = new();
    
    /// <summary>
    ///     The volume at which the background sound is played.
    /// </summary>
    private float backgroundVolume = 0;

    void Start() {
        audioSource = GetComponent<AudioSource>();
        collisionSounds = new AudioClip[MAX_CLIP_COUNT];
    }

    void Update()
    {
        if (backgroundVolume <= 0) return;

        if (loopTimer.Update()) {
            audioSource.PlayOneShot(backgroundSound, backgroundVolume);
        }
    }

    /// <summary>
    ///     Play a random collision noise with a random pitch.
    /// </summary>
    public void CollideSound() {
        if (GameInfo.StartCutscene || collideVolume <= 0) return;
        
        int i = Random.Range(0, collisionClipCount);
        audioSource.PlayRandomPitchOneShot(collisionSounds[i], collidePitchBounds, collideVolume);
    }

    /// <summary>
    ///     Load the different sound clips that can be played by this <tt>BlobSoundController</tt>,
    ///     and set their volumes and pitches.
    /// </summary>
    /// <param name="soundData">
    ///     The data defining the sound clips to load and the parameters with which to play them.
    /// </param>
    public void SetClips(BlobSoundDataStruct soundData)
    {
        audioSource.Stop();

        backgroundSound = null;
        collisionClipCount = 0;
        collideVolume = 0;
        collidePitchBounds = soundData.collidePitchBounds;

        LoadClips(soundData);

        if (backgroundSound == null) {
            backgroundVolume = 0;
            loopTimer.Reset();
            return;
        }

        backgroundVolume = soundData.backgroundVolume;

        loopTimer.SetInterval(backgroundSound.length);
        loopTimer.Skip();
    }

    /// <summary>
    ///     Load the different sound clips as resources.
    /// </summary>
    /// <param name="soundData">
    ///     The data defining the sound clips to load and the parameters with which to play them.
    /// </param>
    private void LoadClips(BlobSoundDataStruct soundData)
    {
        if (soundData.family == null) return;

        collideVolume = soundData.collideVolume;

        foreach ((string file, string resource) in FileUtilities.GetFilesAndResources(
            Path.Combine(FileUtilities.SOUNDS, soundData.family), "*.wav"
        ))
        {
            AudioClip clip = Resources.Load<AudioClip>(resource);

            if (file == "background")
            {
                backgroundSound = clip;
            }
            else if (collisionClipCount < MAX_CLIP_COUNT)
            {
                collisionSounds[collisionClipCount] = clip;
                collisionClipCount++;
            }
        }
    }
}