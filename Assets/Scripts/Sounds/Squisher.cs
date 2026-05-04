using UnityEngine;

/// <summary>
///     A class for playing the squishy noises that a blob makes.
/// </summary>
public class Squisher : MonoBehaviour
{
    /// <summary>
    ///     The audioSource set by the corresponding blob controller.
    /// </summary>
    public AudioSource audioSource;
    /// <summary>
    ///     The list of possible squish noises that can be played.
    /// </summary>
    private AudioClip[] clips;
    private const int CLIP_COUNT = 8;

    void Awake() {
        clips = new AudioClip[CLIP_COUNT];

        for (int i = 0; i < CLIP_COUNT; i++) {
            clips[i] = Resources.Load<AudioClip>(Files.SOUNDS_PATH + $"squish_{i}");
        }
    }

    /// <summary>
    ///     Play a random squish noise with a random pitch.
    /// </summary>
    public void squish() {
        if (!GameInfo.StartCutscene) {
            audioSource.pitch = Random.Range(0.5f, 1.5f);
            int i = Random.Range(0, 8);
            
            audioSource.PlayOneShot(clips[i], 0.05f);
        }
    }
}
