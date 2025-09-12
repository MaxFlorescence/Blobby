using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
///     This class defines the behavior of the blob character as a whole.
/// </summary>
public class BlobController : MonoBehaviour
{
    // PUBLIC MEMBERS
    public Squisher squisher;
    public RoundaboutPlayer roundabout;

    // TODO: move functionality to menus
    public CheatMenu cheatMenu;
    public PauseMenu pauseMenu;

    // PRIVATE MEMBERS
    // Input
    /// <summary>
    ///     Strength of the blob's movement.
    /// </summary>
    private float movementIntensity = 10f;
    /// <summary>
    ///     Strength of the blob's jumps.
    /// </summary>
    private float jumpIntensity = 8f;
    private Vector3 jumpDirection = Vector3.up;
    private bool doJump = false;
    private bool movementInputEnabled = true;

    // Atoms
    private CreateBlob createBlob;
    /// <summary>
    ///     Quick reference for <tt>blobAtoms[0]</tt>.
    /// </summary>
    private GameObject centerAtom;
    private int numAtoms;
    private AtomController[] atomControllers;

    // Behavior
    private GameObject grabbedObject;

    // Camera
    private GameObject mainCamera;
    /// <summary>
    ///    Distance from the camera to the blob character.
    /// </summary>
    private float cameraDistance = 10f;

    // Audio
    private AudioSource roundaboutAudio;
    private bool audioIsPaused = false;

    /// <summary>
    ///     Create the atom controllers and set up the audio sources.
    /// </summary>
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        numAtoms = createBlob.GetAtoms().Length;
        centerAtom = createBlob.GetAtoms()[0];

        // Allow the camera to track the blob.
        mainCamera.GetComponent<CameraController>().TrackObject(centerAtom, cameraDistance);

        // Allow the cheats menu to teleport the blob.
        cheatMenu.blobController = this;

        atomControllers = new AtomController[numAtoms];

        for (int i = 0; i < numAtoms; i++)
        {
            atomControllers[i] = createBlob.GetAtoms()[i].AddComponent<AtomController>();
            atomControllers[i].blobController = this;
        }

        SetupSounds();
    }

    public void setCreateBlob(CreateBlob createBlob)
    {
        this.createBlob = createBlob;
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

        // Jumps should only require a single keypress which might not align with physics updates,
        // so detect the keypress in Update() and perform the action in FixedUpdate().
        Vector3 jumpForce = doJump ? (jumpIntensity * jumpDirection) : Vector3.zero;
        doJump = false;

        if (movementInputEnabled)
        {
            ApplyForces(movementForce, jumpForce, true);
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
                atom.SetForce(force.Value);
            }
            if (impulse.HasValue)
            {
                atom.SetImpulse(impulse.Value);
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
            atom.SetGravity(gravity);
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
    public void SetMovementInputEnabled(bool enabled, float delay = 0f)
    {
        if (delay > 0)
        {
            StartCoroutine(DelayedSetMovementInputEnabled(enabled, delay));
        }
        else
        {
            movementInputEnabled = enabled;
        }
    }

    /// <summary>
    ///     Helper function for SetInputEnabled to incorporate a delay.
    /// </summary>
    private IEnumerator DelayedSetMovementInputEnabled(bool enabled, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetMovementInputEnabled(enabled);
    }

    /// <summary>
    ///     Set the blob character's velocity to zero.
    /// </summary>
    public void StopMovement()
    {
        foreach (AtomController atom in atomControllers)
        {
            atom.SetVelocity(Vector3.zero);
        }
    }

    /// <summary>
    ///    Get the blob character's velocity.
    /// </summary>
    /// <returns>
    ///     The velocity Vector3 of the center atom.
    /// </returns>
    public Vector3 GetVelocity()
    {
        return centerAtom.GetComponent<Rigidbody>().velocity;
    }

    /// <summary>
    ///     Apply user input for non-movement actions and unpause audio if needed.
    /// </summary>
    void Update()
    {
        if (!LevelStartupInfo.StartCutscene) // don't allow input during cutscene
            {
            if (movementInputEnabled && Input.GetButtonDown("Jump"))
                {
                doJump = true;
                }

                if (Time.timeScale > 0) // only process if game is not paused
                {
                    if (audioIsPaused)
                    {
                        roundaboutAudio.UnPause();
                        audioIsPaused = false;
                    }

                if (Input.GetKeyDown("q"))
                {
                    Release();
                }


                // TODO: move functionality to menus
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

        foreach (GameObject atom in createBlob.GetAtoms())
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
        foreach (AtomController atom in atomControllers)
        {
            if (atom.GetTouchCount() > 0)
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

    /// <summary>
    ///    Return a boolean indicating if the blob character is holding an object with the specified tag.
    /// </summary>
    /// <param name="tag">
    ///     The tag to check for.
    /// </param>
    /// <returns>
    ///     <tt>true</tt> iff the held object has the tag.
    /// </returns>
    public bool HoldingObjectWithTag(string tag)
    {
        return grabbedObject != null && grabbedObject.CompareTag(tag);
    }

    /// <summary>
    ///     Return true if the given object is one of this blob's atoms.
    /// </summary>
    /// <param name="obj">
    ///     The GameObject to check.
    /// </param>
    /// <returns>
    ///     <tt>true</tt> iff the object is one of this blob's atoms.
    /// </returns>
    public bool IsAtom(GameObject obj)
    {
        return createBlob.GetAtoms().Contains(obj);
    }

    public Vector3 GetPosition()
    {
        return centerAtom.transform.position;
    }
}
