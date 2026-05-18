using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
///     This class defines the behavior of the blob character as a whole.
/// </summary>
public class BlobController : MonoBehaviour, IControllable
{
    //----------------------------------------------------------------------------------------------
    // INPUT
    //----------------------------------------------------------------------------------------------
    private Vector3 jumpDirection = Vector3.up;
    private bool jumpOnNextFixedUpdate = false;
    private const float JUMP_MULTIPLIER = 8f;
    private bool movementInputEnabled = true;
    private const float MOVEMENT_MULTIPLIER = 10f;
    public bool controlled { get; set; } = false;

    //----------------------------------------------------------------------------------------------
    // STRUCTURE
    //----------------------------------------------------------------------------------------------
    private CreateBlob createBlob;
    /// <summary>
    ///     Quick reference for <tt>blobAtoms[0]</tt>.
    /// </summary>
    private GameObject centerAtom;
    private int numAtoms;
    private AtomController[] atomControllers;
    /// <summary>
    ///     Holds the blob's spring length factor constant.
    /// </summary>
    private bool springsLocked = false;
    /// <summary>
    ///     The factors by which the blob can shrink or grow from its original size (1f).
    /// </summary>
    private readonly float[] SIZE_FACTORS = {0.5f, 1f, 1.5f, 2.5f};

    //----------------------------------------------------------------------------------------------
    // STICKING
    //----------------------------------------------------------------------------------------------
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
    private readonly GameObject[] atomStickies = new GameObject[STICKY_COUNT];
    /// <summary>
    ///     Indicates if atoms can become sticky. If <tt>false</tt>, no atoms are sticky.
    /// </summary>
    private bool stickyMode = false;
    private const float STICKY_MOVEMENT_MULTIPLIER = 1.2f;

    //----------------------------------------------------------------------------------------------
    // INVENTORY
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The list of gameObjects carried by the blob.
    /// </summary>
    public Inventory inventory;
    /// <summary>
    ///     How much burden the blob can carry in its inventory.
    /// </summary>
    private const int CARRYING_CAPACITY = 10;
    /// <summary>
    ///     The camera providing an image of the inventory display to the UI.
    /// </summary>
    private Camera inventoryCamera;
    /// <summary>
    ///     How far away the inventory camera is from the inventory display position.
    /// </summary>
    private const float INVENTORY_CAMERA_DISTANCE = 2f;
    /// <summary>
    ///     The player can cause the blob to drop a held object iff this is <tt>true</tt>.
    /// </summary>
    private bool controlCanRelease = true;

    //----------------------------------------------------------------------------------------------
    // VISUALS
    //----------------------------------------------------------------------------------------------
    private MeshRenderer blobMesh;
    public BlobMaterial Material { get; private set; }
    public Mesh DropletMesh { get; private set; }
    /// <summary>
    ///     Light sources attached to the blob, paired with flags indicating their default states.
    /// </summary>
    public BlobLightController Lights { get; private set; }= new();
    public bool AtomsVisible { get; private set; } = false;

    //----------------------------------------------------------------------------------------------
    // AUDIO
    //----------------------------------------------------------------------------------------------
    private AudioSource audioSource;
    /// <summary>
    ///     Makes squishy noises on collisions.
    /// </summary>
    public Squisher Squisher { get; private set; }
    /// <summary>
    ///     The sound to play when obtaining an object.
    /// </summary>
    private const string PICK_UP_SOUND = "bubbles";
    /// <summary>
    ///     The sound to play when releasing an object.
    /// </summary>
    private const string DROP_SOUND = "bubble_pop";
    /// <summary>
    ///     The lowest and highest pitch to play inventory sounds at.
    /// </summary>
    private readonly Vector2 INVENTORY_PITCH_BOUNDS = new(0.8f, 1.2f);
    
    //----------------------------------------------------------------------------------------------
    // GHOST MODE
    //----------------------------------------------------------------------------------------------
    public bool GhostMode { get; private set; } = false;
    private const float GHOST_SPEED = 0.5f;

    void Awake()
    {
        GameInfo.SetControlledBlob(this);

        createBlob = transform.parent.GetComponentInChildren<CreateBlob>();
        blobMesh = createBlob.gameObject.GetComponent<MeshRenderer>();
        
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        DropletMesh = Instantiate(sphere.GetComponent<MeshFilter>().mesh);
        Destroy(sphere);

        audioSource = gameObject.AddComponent<AudioSource>();
        inventory = gameObject.AddComponent<Inventory>();
        inventory.SetCapacity(CARRYING_CAPACITY);
        inventory.SetDisplayMode(DisplayMode.UI_Only);
    }

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
        atomControllers[0].GetComponent<AtomController>().SetAsCenterAtom(true);

        inventoryCamera = transform.parent.GetComponentsInChildren<Camera>()[0];
        inventoryCamera.enabled = true;

        // TODO: order of elements in array unclear
        Light[] lightComponents = transform.parent.GetComponentsInChildren<Light>();
        Lights.Define(BlobLight.Material_Glow, lightComponents[0], false);
        Lights.Define(BlobLight.Inventory_Icon, lightComponents[1], false);

        SetBlobMaterials(BlobMaterial.Water, true);

        SetupSounds();
    }

    /// <summary>
    ///     Create and attach the audio source components to the center atom.
    /// </summary>
    private void SetupSounds()
    {
        Squisher = centerAtom.AddComponent<Squisher>();
        Squisher.audioSource = audioSource;
        
        inventory.SetAudio(PICK_UP_SOUND, DROP_SOUND, INVENTORY_PITCH_BOUNDS);
    }
    
    /// <summary>
    ///     Apply user input as blob character movement.
    /// </summary>
    void FixedUpdate()
    {
        if (GhostMode) ApplyGhostMovement();

        if (!movementInputEnabled || !controlled)
            return;

        (Vector3 forwardForce, Vector3 rightwardForce) = GetInputAxisForces();

        // Constrain initial movementForce to the unit disk.
        Vector3 movementForce = forwardForce + rightwardForce;
        if (movementForce.magnitude > 1)
        {
            movementForce = movementForce.normalized;
        }
        movementForce *= MOVEMENT_MULTIPLIER * (stickyMode ? STICKY_MOVEMENT_MULTIPLIER : 1);

        // Jumps should only require a single keypress which might not align with physics updates,
        // so detect the keypress in Update() and perform the action in FixedUpdate().
        Vector3 jumpForce = jumpOnNextFixedUpdate ? (JUMP_MULTIPLIER * jumpDirection) : Vector3.zero;
        jumpOnNextFixedUpdate = false;

        ApplyForces(movementForce, jumpForce, true);
    }

    /// <summary>
    ///     Read the player's movement input on the horizontal (left-right) and vertical
    ///     (forward-back) axes as component force vectors in the z-plane.
    /// </summary>
    /// <returns>
    ///     <tt>(Vector3, Vector3)</tt> The forward and rightward component force vectors.
    /// </returns>
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
        if (requireTouching && !IsTouching())
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
    ///     Are any of the blob's atoms touching an object?
    /// </summary>
    /// <param name="obj">
    ///     The object to test for. If null, test if the blob is touching anything.
    /// </param>
    /// <returns>
    ///     (If object is not null) True iff the blob is touching the object.<br/>
    ///     (If object is null) True iff the blob is touching anything.
    /// </returns>
    public bool IsTouching(GameObject obj = null)
    {        
        if (obj == null && stickyMode)
        {
            for (int i = 0; i < STICKY_COUNT; i++)
            {
                if (atomStickies[i] != null) return true;
            }
        }

        foreach (AtomController atom in atomControllers)
        {
            if (atom.IsTouching(obj)) return true;
        }

        return false;
    }

    /// <summary>
    ///     Apply user input for non-movement actions and unpause audio if needed.
    /// </summary>
    void Update()
    {
        MoveInventoryCamera();

        if (!controlled || GameInfo.StartCutscene || GameInfo.GameStatus == GameState.Paused) return;

        if (controlCanRelease && Input.GetKeyDown(KeyCode.Q)) inventory.TryDrop();

        float mouseScroll = Input.mouseScrollDelta.y;
        if (mouseScroll != 0) inventory.SelectNextNonEmptyObject(mouseScroll > 0);

        if (movementInputEnabled)
        {
            if (Input.GetButtonDown("Jump")) jumpOnNextFixedUpdate = true;
            if (Input.GetKeyDown(KeyCode.LeftShift)) SetStickyMode(true);
            if (Input.GetKeyUp(KeyCode.LeftShift)) SetStickyMode(false);

            // left mouse shrinks, right mouse grows
            if (Input.GetMouseButton(0))
            {
                TrySetSpringLengths(size: 0);
            }
            else if (Input.GetMouseButton(1))
            {
                TrySetSpringLengths(size: 2);
            }
            else
            {
                TrySetSpringLengths(size: 1);
            }
        }

        SetAtomsVisible(GameInfo.DebugMode);

        if (!GameInfo.DebugMode) return;
        
        if (Input.GetKeyDown(KeyCode.G)) ToggleGhostMode();

        if (Input.GetKey(KeyCode.M))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetBlobMaterials(BlobMaterial.Water);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetBlobMaterials(BlobMaterial.Lava);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetBlobMaterials(BlobMaterial.Honey);
            }
        }
    }

    /// <summary>
    ///     Lock the inventory camera to be directly behind the inventory item held by the blob.
    /// </summary>
    private void MoveInventoryCamera()
    {
        GameObject inventorySelection = inventory.GetObject();
        Transform targetTransform = inventorySelection != null ?
            inventorySelection.transform : transform;
        inventoryCamera.transform.position = targetTransform.position + INVENTORY_CAMERA_DISTANCE * Vector3.back;
        inventoryCamera.transform.LookAt(targetTransform);
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
///     Enables/disables ghost mode for the blob. Ghost mode disables gravity and enables flying
///     and clipping.
/// </summary>
    private void ToggleGhostMode()
    {
        GhostMode = !GhostMode;
        SetRestrained(GhostMode);
    }

    /// <summary>
    ///     Applies flying motion while ghost mode is active.
    /// </summary>
    private void ApplyGhostMovement()
    {
        if (!GhostMode) return;

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
            atom.transform.position += GHOST_SPEED * translation;
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
        this.DelayedExecute(delay, () => {movementInputEnabled = enabled;});
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
    ///     Restrain the blob to its current position, and override its spring lengths if necessary.
    /// </summary>
    /// <param name="enabled">
    ///     Restrain the blob iff this is <tt>true</tt>.
    /// </param>
    /// <param name="springOverrideFactor">
    ///     The spring factor to use while the blob is restrained.
    /// </param>
    /// <param name="delaySpringUnlock">
    ///     The amount of time (in seconds) to delay before unlocking the blob's springs.
    /// </param>
    public void SetRestrained(bool enabled, float springOverrideFactor = 1f, float delaySpringUnlock = 0f)
    {
        SetMovementInputEnabled(!enabled, enabled ? 0 : 0.5f);
        SetGravity(!enabled);
        if (enabled)
        {
            SetColliders(false);
            SetStickyMode(false);
            StopMovement();
            createBlob.SetJointProperties(springOverrideFactor, false, true);
            LockSprings(true);
        }
        else
        {
            SetColliders(true, 0.1f);
            this.DelayedExecute(delaySpringUnlock, () => LockSprings(false));
        }
        HoldCenterAtom(enabled);
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
        ApplyForces(Vector3.zero, Vector3.zero, false);
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
        centerAtomRigidbody.constraints = hold ?
            RigidbodyConstraints.FreezePosition :
            RigidbodyConstraints.None;
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

    //----------------------------------------------------------------------------------------------
    // Getters
    //----------------------------------------------------------------------------------------------
    /// <returns>
    ///     <tt> Vector3 </tt> The position of the blob's center atom.
    /// </returns>
    public Vector3 GetPosition()
    {
        return centerAtom.transform.position;
    }

    /// <returns>
    ///     <tt> Vector3 </tt> The velocity of the blob's center atom.
    /// </returns>
    public Vector3 GetVelocity()
    {
        return centerAtom.GetComponent<Rigidbody>().velocity;
    }

    public bool IsSticky()
    {
        return stickyMode;
    }
    
    public GameObject GetCenterAtom()
    {
        return centerAtom;
    }

    private HashSet<GameObject> TouchingObjects()
    {
        HashSet<GameObject> touchingUnion = new();

        foreach (AtomController atom in atomControllers)
        {
            touchingUnion.UnionWith(atom.touching);
        }

        return touchingUnion;
    }

    //----------------------------------------------------------------------------------------------
    // Setters
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     Change the sticky mode of the blob.
    /// </summary>
    /// <param name="enable">
    ///     Enables sicky mode iff this is <tt>True</tt>.
    /// </param>
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
    
    public void LockSprings(bool enabled)
    {
        springsLocked = enabled;
    }

    /// <summary>
    ///     Sets the spring lengths of the blob if they are unlocked. Otherwise does nothing.
    /// </summary>
    /// <param name="factor">
    ///     The new spring length factor to use.
    /// </param>
    /// <param name="immediately">
    ///     If <tt>true</tt>, force the blob's atoms to snap to their equilibrium positions
    ///     immediately.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the spring lengths were successfully updated.
    /// </returns>
    public bool TrySetSpringLengths(int size = 1, bool immediately = false)
    {
        float factor = SIZE_FACTORS[size];
        if (springsLocked || createBlob.GetSpringLengthFactor() == factor) return false;
        
        createBlob.SetJointProperties(factor, true, immediately);
        return true;
    }
    
    /// <summary>
    ///     Enables/disables each atom's collider in this blob.
    /// </summary>
    /// <param name="enable">
    ///     Enables atom colliders iff this is <tt>True</tt>.
    /// </param>
    public void SetColliders(bool enabled, float delay = 0)
    {
        this.DelayedExecute(delay, () =>
        {
            foreach (AtomController atom in atomControllers)
            {
                atom.SetCollider(enabled);
            }
        });
    }

    private void SetAtomsVisible(bool visible)
    {
        if (AtomsVisible == visible) return;
        AtomsVisible = visible;

        foreach (AtomController atom in atomControllers)
        {
            atom.SetVisible(visible);
        }
    }

    /// <summary>
    ///     Set the materials for the blob's body and droplets. This affects the blobs properties.
    /// </summary>
    /// <param name="newBlobMaterials">
    ///     The blob materials to set.
    /// </param>
    public void SetBlobMaterials(BlobMaterial newBlobMaterials, bool force = false)
    {
        if (!force && Material == newBlobMaterials) return;

        Material = newBlobMaterials;

        bool glow = newBlobMaterials.Has(BlobMaterialProperties.Glowing);
        Lights.Set(BlobLight.Material_Glow, glow, true);
        createBlob.SetJointProperties(1, !newBlobMaterials.Has(BlobMaterialProperties.Solid), false);

        blobMesh.materials = new Material[] {newBlobMaterials.Body()};

        Material dropMaterial = newBlobMaterials.Drops();
        foreach (AtomController atom in atomControllers)
        {
            atom.SetDropletMaterial(dropMaterial);
        }
    }

    /// <summary>
    ///     Enables/disables the blob's ability to release held objects from its inventory.
    /// </summary>
    public void SetControlCanRelease(bool canRelease)
    {
        controlCanRelease = canRelease;
    }

    public override string ToString()
    {
        return $"BlobController: {name}\n"
        + $" ghostMode: {GhostMode}\n"
        + $" blobMaterials: {Material} ({Material.GetProperties()})\n"
        + $" stickyMode: {stickyMode}\n"
        + $" springFactor: {createBlob.GetSpringLengthFactor()}\n"
        + $" touchingSomething: {IsTouching()}\n"
        + $" inventory: {inventory}";
    }
}
