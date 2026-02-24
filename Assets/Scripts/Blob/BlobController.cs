using System.Collections;
using UnityEngine;

public enum BlobLight
{
    Inventory = 0,
    Material = 1
}

/// <summary>
///     This class defines the behavior of the blob character as a whole.
/// </summary>
public class BlobController : Controllable
{
    // PUBLIC MEMBERS
    public Squisher squisher;

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
    private float stickyModifier = 1.2f;

    // Inventory
    /// <summary>
    ///     The list of gameObjects carried by the blob.
    /// </summary>
    private GameObject[] inventory;
    private const int INVENTORY_SIZE = 10;
    /// <summary>
    ///     Which inventory object is currently selected.
    /// </summary>
    private int inventorySelection = 0;
    /// <summary>
    ///     How much burden the blob can carry in its inventory.
    /// </summary>
    private const int CARRYING_CAPACITY = 10;
    /// <summary>
    ///     The current burden the blob is carrying.
    /// </summary>
    private int currentBurden = 0;
    /// <summary>
    ///     The sound to play when releasing an object.
    /// </summary>
    private AudioClip releaseSound;
    private AudioSource audioSource;
    private Camera inventoryCamera;
    private float inventoryCameraDistance = 2f;
    private (Light, bool)[] blobLights = new (Light, bool)[2];

    // Fire
    public bool canIgnite { get; private set; } = false;
    public bool canExtinguish { get; private set; } = false;

    // Visuals
    private MeshRenderer blobMesh;
    private BlobMaterials blobMaterials;
    public Mesh dropletMesh { get; private set; }

    // Misc
    /// <summary>
    ///     The factor by which the blob can grow from its original size.
    /// </summary>
    private float blobGrowingFactor = 1.5f;
    /// <summary>
    ///     The factor by which the blob can shrink from its original size.
    /// </summary>
    private float blobShrinkingFactor = 0.5f;
    // Ghost mode
    private bool ghostMode = false;
    private float ghostSpeed = 0.5f;

    void Awake()
    {
        GameInfo.SetControlledBlob(this);

        createBlob = transform.parent.GetComponentInChildren<CreateBlob>();
        blobMesh = createBlob.gameObject.GetComponent<MeshRenderer>();
        
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dropletMesh = Instantiate(sphere.GetComponent<MeshFilter>().mesh);
        Destroy(sphere);

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    ///     Create the atom controllers and set up the audio sources.
    /// </summary>
    void Start()
    {
        numAtoms = createBlob.GetAtoms().Length;
        centerAtom = createBlob.GetAtoms()[0];

        atomControllers = new AtomController[numAtoms];

        for (int i = 0; i < numAtoms; i++)
        {
            atomControllers[i] = createBlob.GetAtoms()[i].AddComponent<AtomController>();
            atomControllers[i].blobController = this;
        }
        atomControllers[0].GetComponent<AtomController>().SetCenterAtom(true);

        inventory = new GameObject[INVENTORY_SIZE];
        inventoryCamera = transform.parent.GetComponentsInChildren<Camera>()[0];
        inventoryCamera.enabled = true;

        Light[] lights = transform.parent.GetComponentsInChildren<Light>();
        blobLights[(int)BlobLight.Material] = (lights[0], false);
        blobLights[(int)BlobLight.Inventory] = (lights[1], false);

        SetBlobMaterials(BlobMaterials.WATER);

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
            && obj.TryGetComponent<Rigidbody>(out var objRigidbody)
            && !obj.CompareTag("No Sticky"))
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
        squisher = centerAtom.AddComponent<Squisher>();
        squisher.audioSource = centerAtom.AddComponent<AudioSource>();
        
        releaseSound = Resources.Load("Sounds/bubble_pop", typeof(AudioClip)) as AudioClip;
    }

    /// <summary>
    ///     Apply user input as blob character movement.
    /// </summary>
    void FixedUpdate()
    {
        if (ghostMode) ApplyGhostMovement();

        if (!movementInputEnabled || !controlled)
            return;

        (Vector3 forwardForce, Vector3 rightwardForce) = GetInputAxisForces();

        // Constrain initial movementForce to the unit disk.
        Vector3 movementForce = forwardForce + rightwardForce;
        if (movementForce.magnitude > 1)
        {
            movementForce = movementForce.normalized;
        }
        movementForce *= movementIntensity * (stickyMode ? stickyModifier : 1);

        // Jumps should only require a single keypress which might not align with physics updates,
        // so detect the keypress in Update() and perform the action in FixedUpdate().
        Vector3 jumpForce = doJump ? (jumpIntensity * jumpDirection) : Vector3.zero;
        doJump = false;

        ApplyForces(movementForce, jumpForce, true);
    }

    private (Vector3, Vector3) GetInputAxisForces() {
        // Ensure forward/rightward movement occurs in the horizontal plane.
        Vector3 forwardForce = Vector3.ProjectOnPlane(GameInfo.ControlledCamera.transform.forward, Vector3.up).normalized;
        forwardForce *= Input.GetAxis("Vertical");

        Vector3 rightwardForce = Vector3.ProjectOnPlane(GameInfo.ControlledCamera.transform.right, Vector3.up).normalized;
        rightwardForce *= Input.GetAxis("Horizontal");

        return (forwardForce, rightwardForce);
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
        MoveInventoryCamera();

        if (!controlled || GameInfo.StartCutscene || GameInfo.GameStatus == GameState.PAUSED)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Release();
        }

        float mouseScroll = Input.mouseScrollDelta.y;
        if (mouseScroll != 0)
        {
            SelectNextNonEmptyObject(mouseScroll > 0);
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

    private void MoveInventoryCamera()
    {
        Transform targetTransform = inventory[inventorySelection] != null ?
            inventory[inventorySelection].transform : transform;
        inventoryCamera.transform.position = targetTransform.position + inventoryCameraDistance * Vector3.back;
        inventoryCamera.transform.LookAt(targetTransform);
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
    ///     Returns a bool indicating if the blob character is touching the game object.
    /// </summary>
    /// <returns>
    ///     <tt>true</tt> if touching something, <tt>false</tt> if not.
    /// </returns>
    public bool IsTouching(GameObject obj)
    {        
        foreach (AtomController atom in atomControllers)
            {
                if (atom.IsTouching(obj))
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
        int objectBurden = obj.GetComponent<Grip>().burden;
        
        if (CanCarry(objectBurden)) {
            for (int i = 0; i < INVENTORY_SIZE; i++) {
                if (inventory[i] == null)
                {
                    inventory[i] = obj;
                    obj.SetLayer(Utilities.INVENTORY_UI_LAYER);
                    currentBurden += objectBurden;
                    SelectInventoryObject(i);
                    return true;
                }
            }
        }

        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(releaseSound);
        return false;
    }

    public bool CanCarry(int burden)
    {
        return currentBurden + burden <= CARRYING_CAPACITY;
    }

    /// <summary>
    ///     Release the currently grabbed object.
    /// </summary>
    public void Release()
    {
        if (currentBurden == 0)
            return;

        if (inventory[inventorySelection] != null)
        {
            inventory[inventorySelection].GetComponent<Grip>().Release();
            currentBurden -= inventory[inventorySelection].GetComponent<Grip>().burden;
        }
        inventory[inventorySelection] = null;
        SelectNextNonEmptyObject(true);
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(releaseSound);
    }

    public bool IsHolding(GameObject obj)
    {
        if (currentBurden == 0)
            return obj == null;

        for (int i = 0; i < INVENTORY_SIZE; i++) {
            if (inventory[i] == obj)
            {
                return true;
            }
        }

        return false;
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
        if (currentBurden == 0)
            return false;

        for (int i = 0; i < INVENTORY_SIZE; i++) {
            if (inventory[i] != null && inventory[i].CompareTag(tag))
            {
                return true;
            }
        }
        
        return false;
    }

    private void SelectInventoryObject(int i)
    {
        if (inventory[inventorySelection] != null) {
            inventory[inventorySelection].SetLayer(Utilities.INVISIBLE_LAYER);
        }
        if (inventory[i] != null) {
            inventory[i].SetLayer(Utilities.INVENTORY_UI_LAYER);
        }
        inventorySelection = i;
    }

    private void SelectNextNonEmptyObject(bool forward)
    {
        if (currentBurden == 0) 
            return;

        for (int i = 1; i < INVENTORY_SIZE; i++)
        {
            int index = (INVENTORY_SIZE + inventorySelection + (forward ? i : -i)) % INVENTORY_SIZE;
            if (inventory[index] != null)
            {
                SelectInventoryObject(index);
                return;
            }
        }
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
    public bool IsSticky()
    {
        return stickyMode;
    }

    public GameObject GetCenterAtom()
    {
        return centerAtom;
    }

    public void SetColliders(bool enabled)
    {
        foreach (GameObject atom in createBlob.GetAtoms())
        {
            atom.GetComponent<Collider>().enabled = enabled;
        }
    }

    public BlobMaterials GetBlobMaterials()
    {
        return blobMaterials;
    }

    public void SetLight(BlobLight light, bool? enable, bool save = false)
    {
        int index = (int)light;
        enable ??= !blobLights[index].Item2;

        blobLights[index].Item1.enabled = (bool)enable;

        if (save)
        {
            blobLights[index].Item2 = (bool)enable;
        }
    }

    public void ResetLight(BlobLight light)
    {
        blobLights[(int)light].Item1.enabled = blobLights[(int)light].Item2;
    }

    public void SetBlobMaterials(BlobMaterials newBlobMaterials)
    {
        blobMaterials = newBlobMaterials;

        canIgnite = newBlobMaterials.HasProperty(MaterialProperties.CAN_IGNITE);
        canExtinguish = newBlobMaterials.HasProperty(MaterialProperties.CAN_EXTINGUISH);
        SetLight(BlobLight.Material, newBlobMaterials.HasProperty(MaterialProperties.GLOWS), true);

        blobMesh.materials = new Material[] {newBlobMaterials.Body()};

        Material dropMaterial = newBlobMaterials.Drops();
        foreach (GameObject atom in createBlob.GetAtoms())
        {
            atom.GetComponent<AtomController>().SetDropletMaterial(dropMaterial);
        }
    }

    public void ToggleGhostMode()
    {
        ghostMode = !ghostMode;

        if (ghostMode) {
            SetColliders(false);
            SetMovementInputEnabled(false);
            SetStickyMode(false);
            SetGravity(false);
            OverrideSpringLengths(1f);
            ApplyForces(Vector3.zero, Vector3.zero, false);
            StopMovement();
        } else {
            SetColliders(true);
            SetGravity(true);
            SetMovementInputEnabled(true, 0.5f);
            RestoreSpringLengths();
        }
    }

    private void ApplyGhostMovement()
    {
        (Vector3 forwardForce, Vector3 rightwardForce) = GetInputAxisForces();

        Vector3 translation = forwardForce + rightwardForce;
        
        if (Input.GetButton("Jump"))
        {
            translation.y += 1;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            translation.y -= 1;
        }

        foreach (GameObject atom in createBlob.GetAtoms())
        {
            atom.transform.position += ghostSpeed * translation;
        }
    }

    public override string ToString()
    {
        string inventoryString = "";
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            inventoryString += string.Format("  {0}{1}: {2}\n",
                inventorySelection == i ? ">" : " ", i,
                inventory[i] == null ? "null" : inventory[i].GetComponent<Grip>().ToString()
            );
        }

        return string.Format("BlobController:\n"
        + " ghostMode: {0}\n"
        + " blobMaterials: {1} ({2})\n"
        + " stickyMode: {3}\n"
        + " inventory: (currentBurden: {4})\n{5}",
            ghostMode,
            blobMaterials.ToString(),
            blobMaterials.GetProperties().ToString(),
            stickyMode,
            currentBurden,
            inventoryString
        );
    }
}
