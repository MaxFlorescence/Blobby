using UnityEngine;

public class Squisher : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] clips;

    void Awake() {
        clips = new AudioClip[8];

        for (int i = 0; i < 8; i++) {
            clips[i] = Resources.Load("Sounds/squish_" + i.ToString(), typeof(AudioClip)) as AudioClip;
        }
    }

    void Start()
    {
        audioSource.volume = 0.05f;
    }

    public void squish() {
        if (!GameInfo.StartCutscene) {
            audioSource.pitch = Random.Range(0.5f, 1.5f);
            int i = Random.Range(0, 8);
            
            audioSource.PlayOneShot(clips[i]);
        }
    }
}
