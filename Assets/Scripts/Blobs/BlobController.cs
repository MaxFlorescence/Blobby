using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Squisher))]

/// <summary>
///     This class defines the behavior of the blob character as a whole.
/// </summary>
[RequireComponent(typeof(BlobLightController))]
public class BlobController : MonoBehaviour, IControllable
{
    //----------------------------------------------------------------------------------------------
    // MOTION
    //----------------------------------------------------------------------------------------------
    public bool controlled { get; set; } = false;
    /// <summary>
    ///     The position of the blob's transform.
    /// </summary>
    public Vector3 Position => transform.position;
    /// <summary>
    ///     The velocity of the blob's rigidbody.
    /// </summary>
    public Vector3 Velocity => atoms.CenterRigidbody.velocity;
    /// <summary>
    ///     Iff <tt>true</tt>, allows the player to control the blob's movement.
    /// </summary>
    private bool movementInputEnabled = true;
    /// <summary>
    ///     Multiplier for the blob's movement speed.
    /// </summary>
    private const float MOVEMENT_MULTIPLIER = 10f;
    /// <summary>
    ///     Iff <tt>true</tt>, cause the blob to jump on the next fixed update, then reset.
    /// </summary>
    private bool jumpOnNextFixedUpdate = false;
    /// <summary>
    ///     Multiplier for the blob's jump power.
    /// </summary>
    private const float JUMP_MULTIPLIER = 8f;
    /// <summary>
    ///     The direction that the blob jumps in.
    /// </summary>
    private readonly Vector3 JUMP_DIRECTION = Vector3.up;

    //----------------------------------------------------------------------------------------------
    // STRUCTURE
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The atoms that comprise this blob.
    /// </summary>
    public AtomCollection atoms;
    
    /// <summary>
    ///     Controls the joints that hold this blob together.
    /// </summary>
    public BlobJointController jointController;

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
    public bool Sticky { get; private set; } = false;
    /// <summary>
    ///     Additional multiplier applied to movement speed while the blob is sticking to something.
    /// </summary>
    private const float STICKY_MOVEMENT_MULTIPLIER = 1.2f;

    //----------------------------------------------------------------------------------------------
    // INVENTORY
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The list of gameObjects carried by the blob.
    /// </summary>
    public Inventory Inventory { get; private set; }
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
    public bool CanRelease { get; set; } = true;

    //----------------------------------------------------------------------------------------------
    // VISUALS
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     Controls the mesh for this blob.
    /// </summary>
    private BlobMeshController meshController;
    /// <summary>
    ///     Defines behaviors of the blob that depend on its material type.
    /// </summary>
    public BlobMaterial Material { get; private set; }
    /// <summary>
    ///     The mesh to use for the blob's droplet particles.
    /// </summary>
    public Mesh DropletMesh { get; private set; }
    /// <summary>
    ///     Light sources attached to the blob, paired with flags indicating their default states.
    /// </summary>
    public BlobLightController Lights;

    //----------------------------------------------------------------------------------------------
    // AUDIO
    //----------------------------------------------------------------------------------------------
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
    /// <summary>
    ///     <tt>True</tt> iff the blob is currently in ghost mode.
    /// </summary>
    public bool GhostMode { get; private set; } = false;
    /// <summary>
    ///     Multiplier for the blob's flying speed while in ghost mode.
    /// </summary>
    private const float GHOST_SPEED = 0.5f;

    void Awake()
    {
        GameInfo.SetControlledBlob(this);

        Squisher = GetComponent<Squisher>();
        meshController = GetComponentInChildren<BlobMeshController>();
        
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        DropletMesh = Instantiate(sphere.GetComponent<MeshFilter>().mesh);
        Destroy(sphere);

        Inventory = gameObject.AddComponent<Inventory>();
        Inventory.SetCapacity(CARRYING_CAPACITY);
        Inventory.SetDisplayMode(DisplayMode.UI_Only);
    }

    void Start()
    {
        inventoryCamera = transform.GetComponentInParents<Camera>(true);
        inventoryCamera.enabled = true;
        Inventory.SetAudio(PICK_UP_SOUND, DROP_SOUND, INVENTORY_PITCH_BOUNDS);

        // TODO: use data struct and set manually in inspector
        // Light[] lightComponents = transform.GetComponentsInParents<Light>();
        // Lights.Define(BlobLight.Material_Glow, lightComponents[0], false);
        // Lights.Define(BlobLight.Inventory_Icon, lightComponents[1], false);
        Lights = GetComponent<BlobLightController>();

        SetBlobMaterials(BlobMaterial.Water, true);
    }

    void FixedUpdate()
    {
        if (GhostMode) ApplyGhostMovement();

        if (!movementInputEnabled || !controlled) return;

        (Vector3 forwardForce, Vector3 rightwardForce) = GetInputAxisForces();

        // Constrain initial movementForce to the unit disk.
        Vector3 movementForce = forwardForce + rightwardForce;
        if (movementForce.magnitude > 1) movementForce = movementForce.normalized;
        movementForce *= MOVEMENT_MULTIPLIER * (Sticky ? STICKY_MOVEMENT_MULTIPLIER : 1);

        // Jumps should only require a single keypress which might not align with physics updates,
        // so detect the keypress in Update() and perform the action in FixedUpdate().
        Vector3 jumpForce = jumpOnNextFixedUpdate ? (JUMP_MULTIPLIER * JUMP_DIRECTION)
                                                  : Vector3.zero;
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
        Vector3 forwardForce = Vector3.ProjectOnPlane(GameInfo.ControlledCamera.Forward, Vector3.up)
                                      .normalized;
        forwardForce *= Input.GetAxis("Vertical");

        Vector3 rightwardForce = Vector3.ProjectOnPlane(GameInfo.ControlledCamera.Right, Vector3.up)
                                        .normalized;
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

        atoms.SetAllForces(force, impulse);
    }

    /// <summary>
    ///     Are any of the blob's atoms touching an object?
    /// </summary>
    /// <param name="obj">
    ///     The object to test for. If <tt>null</tt>, tests if anything at all is being touched.
    /// </param>
    /// <returns>
    ///     (If object is not <tt>null</tt>) <tt>True</tt> iff the blob is touching the object.<br/>
    ///     (If object is <tt>null</tt>) <tt>True</tt> iff the blob is touching something.
    /// </returns>
    public bool IsTouching(GameObject obj = null)
    {        
        if (obj == null && Sticky)
        {
            for (int i = 0; i < STICKY_COUNT; i++)
            {
                if (atomStickies[i] != null) return true;
            }
        }

        return atoms.AreAnyTouching(obj);
    }

    /// <summary>
    ///     Apply user input for non-movement actions and unpause audio if needed.
    /// </summary>
    void Update()
    {
        MoveInventoryCamera();

        if (!controlled || GameInfo.StartCutscene || GameInfo.GameStatus == GameState.Paused)
            return;

        HandleInventoryControls();

        HandleMovementControls();

        HandleDebugControls();
    }

    /// <summary>
    ///     Allow the player to control inventory dropping and selection.
    /// </summary>
    private void HandleInventoryControls()
    {
        if (CanRelease && Input.GetKeyDown(KeyCode.Q)) Inventory.TryDrop();

        float mouseScroll = Input.mouseScrollDelta.y;
        if (mouseScroll != 0) Inventory.SelectNextNonEmptyObject(mouseScroll > 0);
    }

    /// <summary>
    ///     Allow the player to control blob jumping, stickiness, and size.
    /// </summary>
    private void HandleMovementControls()
    {
        if (!movementInputEnabled) return;
        
        if (Input.GetButtonDown("Jump")) jumpOnNextFixedUpdate = true;

        if (Input.GetKeyDown(KeyCode.LeftShift)) SetStickyMode(true);
        if (Input.GetKeyUp(KeyCode.LeftShift)) SetStickyMode(false);

        // left mouse shrinks, right mouse grows
        if (Input.GetMouseButton(0))
        {
            jointController.TrySetJointProperties(new(BlobSize.Small));
        }
        else if (Input.GetMouseButton(1))
        {
            jointController.TrySetJointProperties(new(BlobSize.Large));
        }
        else
        {
            jointController.TrySetJointProperties(new(BlobSize.Medium));
        }
    }

    /// <summary>
    ///     Allow the player to control ghost mode and blob material when in debug mode.
    /// </summary>
    private void HandleDebugControls()
    {
        atoms.SetAllVisible(GameInfo.DebugMode);

        if (!GameInfo.DebugMode) return;
        
        if (Input.GetKeyDown(KeyCode.G)) {
            ToggleGhostMode();
            GameInfo.AlertSystem.Send($"Ghost mode is now {(GhostMode ? "on" : "off")}");
        }

        if (Input.GetKey(KeyCode.M))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetBlobMaterials(BlobMaterial.Water);
                GameInfo.AlertSystem.Send($"Set material to Water");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetBlobMaterials(BlobMaterial.Lava);
                GameInfo.AlertSystem.Send($"Set material to Lava");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetBlobMaterials(BlobMaterial.Honey);
                GameInfo.AlertSystem.Send($"Set material to Honey");
            }
        }
    }

    /// <summary>
    ///     Lock the inventory camera to be directly behind the inventory item held by the blob.
    /// </summary>
    private void MoveInventoryCamera()
    {
        GameObject inventorySelection = Inventory.GetObject();
        Transform targetTransform = inventorySelection != null ?
            inventorySelection.transform : transform;

        inventoryCamera.transform.position = targetTransform.position
            + INVENTORY_CAMERA_DISTANCE * Vector3.back;
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
        if (Sticky && StickyIndex(atom) == -1
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
            if (atomStickies[i] != null) atomStickies[i].GetComponent<AtomController>().Unstick();
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
        if (index != -1) Unstick(index);
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
        
        if (Input.GetButton("Jump")) translation.y += 1;
        if (Input.GetKey(KeyCode.LeftShift)) translation.y -= 1;

        atoms.TranslateAll(GHOST_SPEED * translation);
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
        atoms.TranslateAll(newPosition - atoms.Center.position);
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
        atoms.SetAllGravity(!enabled);
        if (enabled)
        {
            atoms.SetColliders(false);
            SetStickyMode(false);
            StopMovement();
            jointController.SetJointProperties(new(springOverrideFactor, true), true);
            jointController.Locked = true;
        }
        else
        {
            this.DelayedExecute(0.1f, () => atoms.SetColliders(true));
            this.DelayedExecute(delaySpringUnlock, () => jointController.Locked = false);
        }
        atoms.FreezeCenter(enabled);
    }

    /// <summary>
    ///     Set the blob character's velocity to zero.
    /// </summary>
    public void StopMovement()
    {
        atoms.SetVelocities(Vector3.zero);
        ApplyForces(Vector3.zero, Vector3.zero, false);
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
        Sticky = enable;
        if (!Sticky)
        {
            for (int i = 0; i < STICKY_COUNT; i++)
            {
                Unstick(i);
            }
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
        Lights.SetLight(BlobLight.Material_Glow, glow, true);
        jointController.SetJointProperties(
            new(1, newBlobMaterials.Has(BlobMaterialProperties.Solid)),
            false
        );

        meshController.SetMaterials(newBlobMaterials.Body());
        atoms.SetDropletMaterials(newBlobMaterials.Drops());
    }

    public override string ToString()
    {
        return $"BlobController: {name}\n"
        + $" ghostMode: {GhostMode}\n"
        + $" blobMaterials: {Material} ({Material.GetProperties()})\n"
        + $" stickyMode: {Sticky}\n"
        + $" joints: {jointController.Data}\n"
        + $" touchingSomething: {IsTouching()}\n"
        + $" inventory: {Inventory}";
    }
}
