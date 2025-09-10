using System.Collections;
using UnityEngine;

/// <summary>
///     This class defines the behavior of the blob character as a whole.
/// </summary>
public class BlobController : MonoBehaviour
{
    // Public members
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

    // Private members
    private float movementIntensity = 8f;
    private float jumpIntensity = 8f;
    private Vector3 jumpDirection = Vector3.zero;
    private AtomController[] atomControllers;
    private GameObject grabbedObject;
    private GameObject centerAtom;
    private AudioSource roundaboutAudio;
    private bool audioIsPaused = false;
    private bool inputEnabled = true;

    /// <summary>
    ///     Create the atom controllers and set up the audio sources.
    /// </summary>
    void Start()
    {
        atomControllers = new AtomController[blobAtoms.Length];
        centerAtom = blobAtoms[0];

        for (int i = 0; i < blobAtoms.Length; i++)
        {
            atomControllers[i] = blobAtoms[i].AddComponent<AtomController>();
            atomControllers[i].blobController = this;
        }

        SetupSounds();
    }

    /// <summary>
    ///     Create and attach the audio source components to the center atom.
    /// </summary>
    private void SetupSounds()
    {
        roundaboutAudio = centerAtom.AddComponent<AudioSource>();
        roundabout = centerAtom.AddComponent<RoundaboutPlayer>();
        roundabout.audioSource = roundaboutAudio;
        roundabout.centerAtom = centerAtom.transform;

        squisher = centerAtom.AddComponent<Squisher>();
        squisher.audioSource = centerAtom.AddComponent<AudioSource>();
    }

    /// <summary>
    ///     Apply user input as blob character movement.
    /// </summary>
    void FixedUpdate()
    {
        // Ensure forward/rightward movement occurs in the horizontal plane.
        Vector3 forwardForce = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
        forwardForce *= Input.GetAxis("Vertical");

        Vector3 rightwardForce = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;
        rightwardForce *= Input.GetAxis("Horizontal");

        // Constrain initial movementForce to the unit disk.
        Vector3 movementForce = forwardForce + rightwardForce;
        if (movementForce.magnitude > 1)
        {
            movementForce = movementForce.normalized;
        }
        movementForce *= movementIntensity;

        Vector3 jumpForce = jumpIntensity * jumpDirection;

        if (inputEnabled)
        {
            ApplyForces(movementForce, jumpForce, true);
        }

        if (jumpDirection != Vector3.zero)
        {
            jumpDirection = Vector3.zero;
        }
    }

    /// <summary>
    ///     Apply a force to the BlobController.
    /// </summary>
    /// <param name="force">
    ///     The force vector to apply.
    /// </param>
    /// <param name="impulse">
    ///     The impulse force vector to apply.
    /// </param>
    /// <param name="requireTouching">
    ///     If true, apply zero forces if the blob character is not touching something.
    /// </param>
    public void ApplyForces(Vector3? force, Vector3? impulse, bool requireTouching)
    {
        if (requireTouching && !TouchingSomething())
        {
            force = Vector3.zero;
            impulse = Vector3.zero;
        }

        foreach (AtomController atom in atomControllers)
        {
            if (force.HasValue)
            {
                atom.force = force.Value;
            }
            if (impulse.HasValue)
            {
                atom.impulse = impulse.Value;
            }
        }
    }

    /// <summary>
    ///     Enable/disable gravity for the blob character.
    /// </summary>
    /// <param name="gravity">
    ///     If true, enables gravity. Disables otherwise.
    /// </param>
    public void SetGravity(bool gravity)
    {
        foreach (AtomController atom in atomControllers)
        {
            atom.useGravity(gravity);
        }
    }

    /// <summary>
    ///     Enable/disable player input to the blob character.
    /// </summary>
    /// <param name="enabled">
    ///     Iff true, input is enabled.
    /// </param>
    /// <param name="delay">
    ///     How long to wait in seconds before enabling/disabling input.
    /// </param>
    public void SetInputEnabled(bool enabled, float delay = 0f)
    {
        if (delay > 0)
        {
            StartCoroutine(DelayedSetInputEnabled(enabled, delay));
        }
        else
        {
            inputEnabled = enabled;
        }
    }

    /// <summary>
    ///     Helper function for SetInputEnabled to incorporate a delay.
    /// </summary>
    private IEnumerator DelayedSetInputEnabled(bool enabled, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetInputEnabled(enabled);
    }

    /// <summary>
    ///     Set the blob character's velocity to zero.
    /// </summary>
    public void StopMovement()
    {
        foreach (AtomController atom in atomControllers)
        {
            atom.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    /// <summary>
    ///     Apply user input for non-movement actions and unpause audio if needed.
    /// </summary>
    void Update()
    {
        if (!LevelStartupInfo.StartCutscene) // don't allow input during cutscene
            {
                if (inputEnabled && Input.GetButtonDown("Jump"))
                {
                    jumpDirection = Vector3.up;
                }

                if (Time.timeScale > 0) // only process if game is not paused
                {
                    if (audioIsPaused)
                    {
                        roundaboutAudio.UnPause();
                        audioIsPaused = false;
                    }

                    if (Input.GetKeyDown("t"))
                    {
                        roundaboutAudio.Pause();
                        audioIsPaused = true;
                        cheatMenu.ShowMenu();
                    }

                    if (Input.GetKeyDown("e"))
                    {
                        roundaboutAudio.Pause();
                        audioIsPaused = true;
                        pauseMenu.ShowMenu();
                    }

                    if (Input.GetKeyDown("q"))
                    {
                        Release();
                    }
                }
            }
    }

    /// <summary>
    ///     Teleport the blob character to a new position.
    /// </summary>
    /// <param name="newPosition">
    ///     The Vector3 position at which the center atom of the blob will be.
    /// </param>
    public void Teleport(Vector3 newPosition)
    {
        StopMovement();
        Vector3 translation = newPosition - centerAtom.transform.position;

        foreach (GameObject atom in blobAtoms)
        {
            atom.transform.position += translation;
        }
    }

    /// <summary>
    ///     Returns a bool indicating if the blob character is touching something.
    /// </summary>
    /// <returns>
    ///     <tt>true</tt> if touching something, <tt>false</tt> if not.
    /// </returns>
    private bool TouchingSomething()
    {
        foreach (AtomController controller in atomControllers)
        {
            if (controller.touchCount > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Attempt to grab the game object and keep it held by the blob character.
    ///     This can fail if another object is being held, if the object refuses to be grabbed,
    ///     or if the object does not have a Grip component.
    /// </summary>
    /// <param name="obj">
    ///     The GameObject that the blob character will try to grab.
    /// </param>
    /// <returns>
    ///     <tt>true</tt> iff the object was successfully grabbed.
    /// </returns>
    public bool TryToGrab(GameObject obj)
    {
        if (grabbedObject == null)
        {
            Grip grip = obj.GetComponent<Grip>();
            if (grip != null && grip.GetGrabbed(centerAtom))
            {
                grabbedObject = obj;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Release the currently grabbed object, if it exists.
    /// </summary>
    public void Release()
    {
        if (grabbedObject != null)
        {
            grabbedObject.GetComponent<Grip>().GetReleased();
            grabbedObject = null;
        }
    }

    // TODO: make a menuController
    public void Win(GameObject platform)
    {
        if (grabbedObject != null && grabbedObject.tag == "Flag")
        {
            platform.GetComponent<AudioSource>().Play();

            finishMenu.hasWon = true;
            finishMenu.ShowMenu();
        }
    }

    public void Lose()
    {
        finishMenu.hasWon = false;
        finishMenu.ShowMenu();
    }
}
