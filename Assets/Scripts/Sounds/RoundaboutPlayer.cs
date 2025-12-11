using UnityEngine;

public class RoundaboutPlayer : MonoBehaviour
{
    private AudioSource audioSource = null;
    private bool playing = false;
    private float dropTime = 3.6f;
    private float time = 0;
    private bool audioIsPaused = false;

    private void SetupAudio() {
        audioSource = GameInfo.ControlledBlob.GetCenterAtom().AddComponent<AudioSource>();
        audioSource.clip = Resources.Load("Sounds/roundabout", typeof(AudioClip)) as AudioClip;
    }

    void Update()
    {
        if (audioSource == null) return;

        if (GameInfo.PauseAudio && !audioIsPaused)
        {
            audioIsPaused = true;
            audioSource.Pause();
        }

        if (!GameInfo.PauseAudio && audioIsPaused)
        {
            audioIsPaused = false;
            audioSource.UnPause();
        }
    }

    void FixedUpdate() {
        if (audioSource == null)
        {
            SetupAudio();
        }

        float height = GameInfo.ControlledBlob.GetPosition().y;

        if (!playing && height <= 15) {
            playing = true;
            audioSource.Play();
        } else if (playing && height > 15) {
            playing = false;
            audioSource.Stop();
            time = 0;
        }

        if (playing) {
            float ratio = time / dropTime;
        
            float newY = Mathf.Min(0, height - 15 + 11*ratio);

            gameObject.transform.position = new Vector3(0, newY, 0);

            time += Time.deltaTime;
        } else {
            gameObject.transform.position = Vector3.zero;
        }
    }
}
