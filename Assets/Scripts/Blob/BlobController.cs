using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
///     This class defines the behavior of the blob character as a whole.
/// </summary>
public class BlobController : Controllable
{
    // PUBLIC MEMBERS
    public Squisher squisher;
    public RoundaboutPlayer roundabout;

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
    /// <summary>
    ///     Stores the blob's spring length factor to restore after overrides.
    /// </summary>
    private float savedSpringFactor = 1f;

    // Sticking
    /// <summary>
    ///     How many atoms can be sticky at once.
    /// </summary>
    private const int STICKY_COUNT = 2;
    /// <summary>
    ///     Index of the last sticky atom, or null if fewer than <tt>STICKY_COUNT</tt> atoms are
    ///     currently sticky.
    /// </summary>
    private int stickyHead = 0;
    /// <summary>
    ///     Circular buffer of capacity <tt>STICKY_COUNT</tt> for holding sticky atoms.
    /// </summary>
    private GameObject[] atomStickies = new GameObject[STICKY_COUNT];
    /// <summary>
    ///     Indicates if atoms can become sticky. If <tt>false</tt>, no atoms are sticky.
    /// </summary>
    private bool stickyMode = false;

    // Misc behavior
    private GameObject grabbedObject;
    /// <summary>
    ///     The factor by which the blob can grow from its original size.
    /// </summary>
    private float blobGrowingFactor = 1.5f;
    /// <summary>
    ///     The factor by which the blob can shrink from its original size.
    /// </summary>
    private float blobShrinkingFactor = 0.5f;

    // Audio
    private AudioSource roundaboutAudio;

    /// <summary>
    ///     Create the atom controllers and set up the audio sources.
    /// </summary>
    void Start()
    {
        numAtoms = createBlob.GetAtoms().Length;
        centerAtom = createBlob.GetAtoms()[0];
        if (GameInfo.ControlledBlob == null)
        {
            GameInfo.SetControlledBlob(this);
        }

        atomControllers = new AtomController[numAtoms];

        for (int i = 0; i < numAtoms; i++)
        {
            atomControllers[i] = createBlob.GetAtoms()[i].AddComponent<AtomController>();
            atomControllers[i].blobController = this;
        }
        atomControllers[0].GetComponent<AtomController>().SetCenterAtom(true);

        SetupSounds();
    }

    /// <summary>
    ///     Returns the index of the given <tt>atom</tt> in the <tt>atomStickies</tt> buffer.
    /// </summary>
    /// <param name="atom">
    ///     The atom to search for.
    /// </param>
    /// <returns>
    ///     The index of the <tt>atom</tt>, if present, -1 if not.
    /// </returns>
    private int StickyIndex(GameObject atom)
    {
        for (int i = 0; i < STICKY_COUNT; i++)
        {
            if (atom == atomStickies[i]) return i;
        }
        return -1;
    }

    /// <summary>
    ///     Try making the <tt>atom</tt> stick to the <tt>obj</tt>. If the <tt>atomStickies</tt>
    ///     buffer is at capacity, replace the oldest sticky atom.
    /// </summary>
    /// <param name="atom">
    ///     The atom to make sticky.
    /// </param>
    /// <param name="obj">
    ///     The game object to stick the <tt>atom</tt> to.
    /// </param>
    /// <returns>
    ///     <tt>true</tt> if successful, <tt>false</tt> otherwise.
    /// </returns>
    public bool TrySticking(GameObject atom, GameObject obj)
    {
        if (stickyMode && StickyIndex(atom) == -1
            && obj.TryGetComponent<Rigidbody>(out var objRigidbody))
        {
            Unstick(stickyHead);
            atom.GetComponent<AtomController>().StickTo(objRigidbody);

            atomStickies[stickyHead] = atom;
            stickyHead = (stickyHead + 1) % STICKY_COUNT;

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Unstick the atom at index <tt>i</tt>, if present.
    /// </summary>
    /// <param name="i">
    ///     The index to remove.
    /// </param>
    private void Unstick(int i)
    {
        if (0 <= i && i < STICKY_COUNT)
        {
            if (atomStickies[i] != null)
            {
                atomStickies[i].GetComponent<AtomController>().Unstick();
            }
            atomStickies[i] = null;
        }
    }

    /// <summary>
    ///     Unstick the given <tt>atom</tt>, if it's sticky.
    /// </summary>
    /// <param name="atom">
    ///     The atom to remove.
    /// </param>
    public void Unstick(GameObject atom)
    {
        int index = StickyIndex(atom);
        if (index != -1)
        {
            Unstick(index);
        }
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
        if (!movementInputEnabled || !Controlled || GameInfo.ControlledCamera == null)
            return;

        // Ensure forward/rightward movement occurs in the horizontal plane.
        Vector3 forwardForce = Vector3.ProjectOnPlane(GameInfo.ControlledCamera.transform.forward, Vector3.up).normalized;
        forwardForce *= Input.GetAxis("Vertical");

        Vector3 rightwardForce = Vector3.ProjectOnPlane(GameInfo.ControlledCamera.transform.right, Vector3.up).normalized;
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

        ApplyForces(movementForce, jumpForce, true);
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
    ///     Force the blob's spring length factor to be <tt>factor</tt>. The current factor can be
    ///     can be restored with <tt>RestoreSpringLengths()</tt>.
    /// </summary>
    public void OverrideSpringLengths(float factor)
    {
        savedSpringFactor = createBlob.GetSpringLengthFactor();
        createBlob.SetSpringLengthFactor(factor);
    }

    /// <summary>
    ///     Restore the blob's spring length factor to the value previously saved by
    ///     <tt>OverrideSpringLengths()</tt>.
    /// </summary>
    public void RestoreSpringLengths()
    {
        createBlob.SetSpringLengthFactor(savedSpringFactor);
    }

    /// <summary>
    ///     Apply user input for non-movement actions and unpause audio if needed.
    /// </summary>
    void Update()
    {
        if (!Controlled || GameInfo.StartCutscene || GameInfo.GameStatus == GameState.PAUSED)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Release();
        }

        if (movementInputEnabled)
        {
            if (Input.GetButtonDown("Jump"))
            {
                doJump = true;
            }
            if (Input.GetMouseButtonDown(0)) // left mouse shrinks
            {
                createBlob.SetSpringLengthFactor(blobShrinkingFactor);
            }
            if (Input.GetMouseButtonDown(1)) // right mouse grows
            {
                createBlob.SetSpringLengthFactor(blobGrowingFactor);
            }
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                createBlob.SetSpringLengthFactor();
            }
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                SetStickyMode(true);
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                SetStickyMode(false);
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
        SetStickyMode(false);
        StopMovement();
        Vector3 translation = newPosition - centerAtom.transform.position;

        foreach (GameObject atom in createBlob.GetAtoms())
        {
            atom.transform.position += translation;
        }
    }

    /// <summary>
    ///     Hold the blob's center atom in place or release it.
    /// </summary>
    /// <param name="hold">
    ///     <tt>true</tt> to begin holding the center atom, <tt>false</tt> to release it.
    /// </param>
    public void HoldCenterAtom(bool hold)
    {
        Rigidbody centerAtomRigidbody = centerAtom.GetComponent<Rigidbody>();
        if (hold)
        {
            centerAtomRigidbody.constraints = RigidbodyConstraints.FreezePosition;
        }
        else
        {
            centerAtomRigidbody.constraints = RigidbodyConstraints.None;
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
        if (stickyMode)
        {
            for (int i = 0; i < STICKY_COUNT; i++)
            {
                if (atomStickies[i] != null)
                {
                    return true;
                }
            }
        }
        
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
            grabbedObject = obj;
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Release the currently grabbed object.
    /// </summary>
    public void Release()
    {
        if (grabbedObject != null)
        {
            grabbedObject.GetComponent<Grip>().Release();
        }
        grabbedObject = null;
    }

    public bool IsHolding(GameObject obj)
    {
        return grabbedObject == obj;
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

    // Getters and setters
    public void SetCreateBlob(CreateBlob createBlob)
    {
        this.createBlob = createBlob;
    }

    public Vector3 GetPosition()
    {
        return centerAtom.transform.position;
    }

    private void SetStickyMode(bool enable)
    {
        stickyMode = enable;
        if (!stickyMode)
        {
            for (int i = 0; i < STICKY_COUNT; i++)
            {
                Unstick(i);
            }
        }
    }
}
