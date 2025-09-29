using UnityEngine;

public class RoundaboutPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public Transform centerAtom;
    private bool playing = false;
    private GameObject floorObject;
    private float dropTime = 3.6f;
    private float time = 0;
    private bool audioIsPaused = false;

    void Start() {
        audioSource.clip = Resources.Load("Sounds/roundabout", typeof(AudioClip)) as AudioClip;
        
        floorObject = GameObject.FindGameObjectsWithTag("Floor")[0];
    }

    void Update()
    {
        if (LevelStartupInfo.PauseAudio && !audioIsPaused)
        {
            audioIsPaused = true;
            audioSource.Pause();
        }

        if (!LevelStartupInfo.PauseAudio && audioIsPaused)
        {
            audioIsPaused = false;
            audioSource.UnPause();
        }
    }

    void FixedUpdate() {
        if (!playing && centerAtom.position.y <= 15) {
            playing = true;
            audioSource.Play();
        } else if (playing && centerAtom.position.y > 15) {
            playing = false;
            audioSource.Stop();
            time = 0;
        }

        if (playing) {
            float ratio = time / dropTime;
        
            float newY = Mathf.Min(0, centerAtom.position.y - 15 + 11*ratio);

            floorObject.transform.position = new Vector3(0, newY, 0);

            time += Time.deltaTime;
        } else {
            floorObject.transform.position = Vector3.zero;
        }
    }
}
