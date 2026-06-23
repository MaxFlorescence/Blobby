using UnityEngine;

/// <summary>
///     This class defines the behavior of the blob character as a whole.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BlobSoundController))]
[RequireComponent(typeof(BlobLightCollection))]
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
    public bool Restrained { get; private set; } = false;

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
    public BlobJointController joints;

    //----------------------------------------------------------------------------------------------
    // STICKING
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     Controls the sticking behavior of this blob.
    /// </summary>
    public AtomStickyController stickies;
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
    ///     Light sources attached to the blob, paired with flags indicating their default states.
    /// </summary>
    public BlobLightCollection Lights { get; private set; }

    //----------------------------------------------------------------------------------------------
    // AUDIO
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     Makes squishy noises on collisions.
    /// </summary>
    public BlobSoundController SoundController { get; private set; }
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

        SoundController = GetComponent<BlobSoundController>();
        meshController = GetComponentInChildren<BlobMeshController>();

        Inventory = gameObject.AddComponent<Inventory>();
        Inventory.SetCapacity(CARRYING_CAPACITY);
        Inventory.SetDisplayMode(DisplayMode.UI_Only);
    }

    void Start()
    {
        inventoryCamera = transform.GetComponentInParents<Camera>(true);
        inventoryCamera.enabled = true;
        Inventory.SetAudio(PICK_UP_SOUND, DROP_SOUND, INVENTORY_PITCH_BOUNDS);
        Lights = GetComponent<BlobLightCollection>();
        
        SetBlobMaterials(BlobMaterial.Water, true);
    }

    void FixedUpdate()
    {
        if (GhostMode) ApplyGhostMovement();

        if (!movementInputEnabled || !controlled) return;

        (Vector3 forwardForce, Vector3 rightwardForce) = GetInputAxisForces();

        // Constrain initial movementForce to the unit disk.
        Vector3 movementForce = forwardForce + rightwardForce;
        if (movementForce.sqrMagnitude > 1) movementForce = movementForce.normalized;
        movementForce *= MOVEMENT_MULTIPLIER * (stickies.Sticky ? STICKY_MOVEMENT_MULTIPLIER : 1);

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
        if (obj == null && stickies.IsSticking()) return true;

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

        HandleStickyControls();

        HandleSizeControls();

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
    ///     Allow the player to control blob jumping.
    /// </summary>
    private void HandleMovementControls()
    {
        if (!movementInputEnabled) return;
        
        if (Input.GetButtonDown("Jump")) jumpOnNextFixedUpdate = true;
    }

    /// <summary>
    ///     Allow the player to control blob stickiness.
    /// </summary>
    private void HandleStickyControls() {
        if (Material.HasAny(BlobMaterialProperties.Non_Stick | BlobMaterialProperties.Sticky)) return;

        if (Input.GetKeyDown(KeyCode.LeftShift)) stickies.SetValue(true);
        if (Input.GetKeyUp(KeyCode.LeftShift)) stickies.SetValue(false);
    }

    /// <summary>
    ///     Allow the player to control blob size.
    /// </summary>
    private void HandleSizeControls() {
        if (Material.HasAll(BlobMaterialProperties.Solid)) return;

        // left mouse shrinks, right mouse grows
        if (Input.GetMouseButton(0))
        {
            joints.SetValue(new(BlobSize.Small));
        }
        else if (Input.GetMouseButton(1))
        {
            joints.SetValue(new(BlobSize.Large));
        }
        else
        {
            joints.SetValue(new(BlobSize.Medium));
        }
    }

    /// <summary>
    ///     Allow the player to control ghost mode and blob material when in debug mode.
    /// </summary>
    private void HandleDebugControls()
    {
        if (!GameInfo.DebugMode) return;
        
        if (Input.GetKeyDown(KeyCode.G)) {
            ToggleGhostMode();
            GameInfo.AlertSystem.Send($"Ghost mode is now {(GhostMode ? "on" : "off")}");
        }
        
        if (Input.GetKeyDown(KeyCode.R)) {
            SetRestrained(!Restrained);
            GameInfo.AlertSystem.Send($"Blob is now {(Restrained ? "restrained" : "free")}");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            GameInfo.ToggleSlowMotion();
            GameInfo.AlertSystem.Send($"Slow motion {(GameInfo.SlowMotion ? "enabled" : "disabled")}");
        }

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
                SetBlobMaterials(BlobMaterial.Acid);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetBlobMaterials(BlobMaterial.Oil);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SetBlobMaterials(BlobMaterial.Honey);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SetBlobMaterials(BlobMaterial.Soda);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                SetBlobMaterials(BlobMaterial.Liquid_Nitrogen);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                SetBlobMaterials(BlobMaterial.Ferrofluid);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                SetBlobMaterials(BlobMaterial.Rubber);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                SetBlobMaterials(BlobMaterial.Aerogel);
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                SetBlobMaterials(BlobMaterialExtensions.TransistionUsing(
                    Material, BlobMaterialProperties.Cold_Transition
                ));
            }
            else if (Input.GetKeyDown(KeyCode.Plus))
            {
                SetBlobMaterials(BlobMaterialExtensions.TransistionUsing(
                    Material, BlobMaterialProperties.Heat_Transition
                ));
            }
            else if (Input.GetKeyDown(KeyCode.Tilde))
            {
                SetBlobMaterials(BlobMaterialExtensions.TransistionUsing(
                    Material, BlobMaterialProperties.Wet_Transition
                ));
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
        stickies.UnstickAll();
        StopMovement();
        atoms.TranslateAll(newPosition - atoms.CenterTransform.position);
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
    public void SetRestrained(bool enabled, float springOverrideFactor = 1f)//, float delaySpringUnlock = 0f)
    {
        Restrained = enabled;
        SetMovementInputEnabled(!enabled, enabled ? 0 : 0.5f);
        atoms.SetAllGravity(!enabled);
        if (enabled)
        {
            atoms.SetColliders(false);
            stickies.SetOverride(false);
            StopMovement();
            joints.SetOverride(new(springOverrideFactor, true, snap: true));
        }
        else
        {
            stickies.ClearOverride();
            this.DelayedExecute(0.1f, () => atoms.SetColliders(true));
            joints.ClearOverride();
            // this.DelayedExecute(delaySpringUnlock, () => jointController.Locked = false);
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
    ///     Set the materials for the blob's body and droplets. This affects the blobs properties.
    /// </summary>
    /// <param name="newBlobMaterials">
    ///     The blob materials to set.
    /// </param>
    public void SetBlobMaterials(BlobMaterial newBlobMaterials, bool force = false)
    {
        if (!force && Material == newBlobMaterials) return;

        bool isGlowing = newBlobMaterials.HasAll(BlobMaterialProperties.Glowing);
        bool isSolid = newBlobMaterials.HasAll(BlobMaterialProperties.Solid);
        bool isSticky = newBlobMaterials.HasAll(BlobMaterialProperties.Sticky);
        bool isNonStick = newBlobMaterials.HasAll(BlobMaterialProperties.Non_Stick);
        bool isLowFriction = newBlobMaterials.HasAll(BlobMaterialProperties.Low_Friction);
        bool wasSticky = Material.HasAll(BlobMaterialProperties.Sticky);

        Lights[BlobLightType.Material_Glow].SetValue(isGlowing);

        joints.SetValue(new(isFixedJoint: isSolid));

        stickies.Resize(isSolid ? 1 : 2);
        stickies.SetMotionLock(isSolid);
        
        if (isSticky)
        {
            stickies.SetValue(true);
        }
        else if (isNonStick || wasSticky)
        {
            stickies.SetValue(false);
        }

        atoms.SetPhysicMaterials(isLowFriction ? "Slippery" : "Jelly");

        meshController.SetMaterial(newBlobMaterials.BodyData());
        atoms.SetParticles(newBlobMaterials.ParticleData());
        SoundController.SetClips(newBlobMaterials.SoundData());

        Material = newBlobMaterials;

        if (GameInfo.DebugMode) GameInfo.AlertSystem.Send($"Set material to {newBlobMaterials}");
    }

    public override string ToString()
    {
        return $"BlobController: {name}\n"
        + $" ghostMode: {GhostMode}\n"
        + $" blobMaterials: {Material} ({Material.Properties()})\n"
        + $" stickyMode: {stickies.Sticky}\n"
        + $" joints: {joints}\n"
        + $" touchingSomething: {IsTouching()}\n"
        + $" inventory: {Inventory}";
    }
}
