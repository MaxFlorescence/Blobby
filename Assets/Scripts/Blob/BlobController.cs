using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobController : MonoBehaviour
{
    private float movementIntensity = 8f;
    private float jumpIntensity = 8f;
    private Vector3 jDirection = Vector3.zero;
    private AtomController[] atomControllers;
    private bool hasFlag = false;
    private GameObject centerAtom;
    private AudioSource roundaboutAudio;
    private bool audioIsPaused = false;

    public Cannon cannon = null;
    public int N;
    public GameObject mainCamera;
    public GameObject[] blobAtoms;
    public Rigidbody[] rigidBodies;
    public FinishMenu finishMenu;
    public CheatMenu cheatMenu;
    public PauseMenu pauseMenu;
    public Squisher squisher;
    public RoundaboutPlayer roundabout;
    public float cameraDistance;
    // public SpringJoint[] springJoints;

    // private Rigidbody centerRigidBody;

    void Start() {
        atomControllers = new AtomController[blobAtoms.Length];
        centerAtom = blobAtoms[0];

        for (int i = 0; i < blobAtoms.Length; i++) {
            atomControllers[i] = blobAtoms[i].AddComponent<AtomController>();
            atomControllers[i].blobController = this;
        }

        SetupSounds();
    }

    private void SetupSounds() {
        roundaboutAudio = centerAtom.AddComponent<AudioSource>();
        roundabout = centerAtom.AddComponent<RoundaboutPlayer>();
        roundabout.audioSource = roundaboutAudio;
        roundabout.centerAtom = centerAtom.transform;

        squisher = centerAtom.AddComponent<Squisher>();
        squisher.audioSource = centerAtom.AddComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        // Vector3 fDirection = mainCamera.transform.forward;
        // Vector3 rDirection = mainCamera.transform.right;
        Vector3 fDirection = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
        Vector3 rDirection = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;

        float fScalar = Input.GetAxis("Vertical");
        float rScalar = Input.GetAxis("Horizontal");
        
        int touching = handleTouching();

        Vector3 movementForce = touching * (fScalar*fDirection + rScalar*rDirection);
        Vector3 jumpForce = touching * jDirection;
        
        foreach (AtomController controller in atomControllers) {
            controller.useGravity(cannon == null);

            controller.moveForce = movementIntensity*movementForce;
            controller.jumpForce = jumpIntensity*jumpForce;
        }


        if (jDirection != Vector3.zero) {
            if (cannon != null)
                cannon.Fire();

            jDirection = Vector3.zero;
        }
    }

    void Update() {
        if (!LevelStartupInfo.StartCutscene) {
            if (Input.GetButtonDown("Jump")) {
                jDirection = GetJumpDirection();
            }

            if (Time.timeScale > 0) {
                if (audioIsPaused) {
                    roundaboutAudio.UnPause();
                    audioIsPaused = false;
                }

                if (Input.GetKeyDown("t")) {
                    roundaboutAudio.Pause();
                    audioIsPaused = true;
                    cheatMenu.ShowMenu();
                }

                if (Input.GetKeyDown("e")) {
                    roundaboutAudio.Pause();
                    audioIsPaused = true;
                    pauseMenu.ShowMenu();
                }
            }
        }
    }

    public void teleport(Vector3 newPosition) {
        Vector3 translation = newPosition - centerAtom.transform.position;

        foreach (GameObject atom in blobAtoms) {
            atom.transform.position += translation;
        }
    }

    public Vector3 GetJumpDirection() {
        if (cannon != null) {
            return cannon.direction.normalized * 4f;
        } else {
            return Vector3.up;
        }
    }

    private int handleTouching() {
        if (cannon != null) return 1;

        foreach (AtomController controller in atomControllers) {
            if (controller.touchCount > 0) {
                return 1;
            }
        }

        return 0;
    }

    public void grabFlag(GameObject flag) {
        hasFlag = true;
        flag.GetComponent<AudioSource>().Play();

        flag.transform.localScale = 0.3f * Vector3.one;
        flag.transform.position = centerAtom.transform.position;
        flag.GetComponent<Collider>().enabled = false;
        flag.transform.SetParent(centerAtom.transform);
    }

    public void win(GameObject platform) {
        if (hasFlag) {
            platform.GetComponent<AudioSource>().Play();

            finishMenu.hasWon = true;
            finishMenu.ShowMenu();
        }
    }

    public void lose() {
        finishMenu.hasWon = false;
        finishMenu.ShowMenu();
    }
}
