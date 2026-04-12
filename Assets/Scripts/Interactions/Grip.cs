using UnityEngine;

/// <summary>
///     States for controlling the gripped object.
/// </summary>
public enum GripState
{
    Idle, Grabbing, Held, Releasing
}

[RequireComponent(typeof(Rigidbody))]
/// <summary>
///     GameObjects with a Grip component can be grabbed by blob characters.
/// </summary>
public class Grip : Interactable
{
    //----------------------------------------------------------------------------------------------
    // Audio
    //----------------------------------------------------------------------------------------------
    protected AudioSource audioSource;
    /// <summary>
    ///    The sound played when the object is grabbed.
    /// </summary>
    public AudioClip gripAudioClip;
    /// <summary>
    ///    The range of possible pitches for <tt>gripAudioClip</tt>.
    /// </summary>
    public Vector2 randomPitchBounds = new(0.8f, 1.2f);

    //----------------------------------------------------------------------------------------------
    // Movement
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     Minimum fraction of the remaining distance that the object moves towards the blob
    ///     during each physics update.
    /// </summary>
    private float movementFactor = 0.1f;
    /// <summary>
    ///    The distance at which the object is considered close enough to be grabbed.
    /// </summary>
    private float grabbingDistance = 0.2f;
    /// <summary>
    ///     The starting distance between the blob and the grabbed object.
    /// </summary>
    private float initialDistance = -1;
    /// <summary>
    ///    The factor by which the <tt>currentHolder</tt>'s motion affects the object's spin while grabbed.
    /// </summary>
    public float spinFactor = 0.5f;

    //----------------------------------------------------------------------------------------------
    // Scaling
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The multiplier for the object's scale once it is grabbed.
    /// </summary>
    public float finalScaleFactor = 0.5f;
    /// <summary>
    ///    The current multiplier for the object's scale, between finalScale and 1.
    /// </summary>
    private float currentScaleFactor = 1f;
    /// <summary>
    ///    The initial scale Vector3 of the object.
    /// </summary>
    private Vector3 initialScale;

    //----------------------------------------------------------------------------------------------
    // Cooldown
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The duration of the grabbing cooldown period.
    /// </summary>
    private float maxCooldown = 3f;
    /// <summary>
    ///     How long it takes for the object's scale to return to normal after releasing.
    /// </summary>
    private float cooldownScaleDuration = 0.5f;

    //----------------------------------------------------------------------------------------------
    // State
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The current state of the gripped object.
    /// </summary>
    private GripState gripState = GripState.Idle;
    /// <summary>
    ///    The blob character currently grabbing this object, or null if none.
    /// </summary>
    private Inventory currentHolder;
    private Inventory lastHolder;
    private bool isIgnoringAtomCollisions = false;
    /// <summary>
    ///    The cost of carrying the object, charged to <tt>currentHolder</tt>'s carrying capacity.
    /// </summary>
    public int burden = 1;

    //----------------------------------------------------------------------------------------------
    // Components
    //----------------------------------------------------------------------------------------------
    private Collider[] gripColliders;
    private Rigidbody gripRigidbody;

    protected virtual void Start()
    {
        gripColliders = GetComponentsInChildren<Collider>();
        gripRigidbody = GetComponent<Rigidbody>();
        audioSource = gameObject.AddComponent<AudioSource>();
        initialScale = transform.localScale;
    }

    /// <summary>
    ///     Handle movement during physics updater while in Grabbing or Held states.
    /// </summary>
    public void FixedUpdate()
    {
        if (GameInfo.GameStatus == GameState.PAUSED) return;

        if (gripState == GripState.Grabbing)
        {
            HandleGrabbingUpdate();
        }
        else if (gripState == GripState.Held)
        {
            transform.position = currentHolder.GetDisplayPosition();
            gripRigidbody.AddTorque(spinFactor * Vector3.Cross(
                Vector3.up, currentHolder.rigidBody.velocity
            ));
        }
    }

    private void HandleGrabbingUpdate()
    {
        Vector3 translation = currentHolder.GetDisplayPosition() - transform.position;
        float distance = translation.magnitude;

        ShrinkByDistance(distance);

        if (distance > grabbingDistance)
        { // move this toward the current holder
            float move = Mathf.Max(grabbingDistance, movementFactor * distance);
            transform.Translate(move * translation.normalized, Space.World);
        }
        else if (currentHolder.TryToAdd(gameObject) >= 0)
        { // in range of the current holder; have it try to pick this up
            UpdateState(GripState.Held);
            SetScale(finalScaleFactor);
        }
        else
        { // pick up failed; try to go back to the last holder, else be dropped
            currentHolder = null;
            if (lastHolder == null || !TryJoin(lastHolder)) GetDropped();
        }
    }

    /// <summary>
    ///    Handle cooldown during regular update while in Releasing state.
    /// </summary>
    protected override void OnUpdate()
    {
        if (gripState == GripState.Releasing)
        {
            GrowByCooldown();
        }
    }

    /// <summary>
    ///     On interaction, start being grabbed if the blob is not holding anything.
    /// </summary>
    /// <param name="blob">
    ///     The blob character interacting with this object.
    /// </param>
    protected override void OnInteract(BlobController blob)
    {
        if (!blob.inventory.IsFull() && blob.IsSticky() && blob.inventory.CanFit(burden))
        {
            TryJoin(blob.inventory);
        }
    }

    public bool TryJoin(Inventory inventory)
    {
        // transfer between inventories if necessary
        if (!inventory.CanFit(burden) ||
            (currentHolder != null && currentHolder.TryToRemove(gameObject) == null)) 
        {
            return false;
        }

        lastHolder = currentHolder;
        currentHolder = inventory;
        initialDistance = (currentHolder.GetDisplayPosition() - transform.position).magnitude;

        foreach (Collider collider in gripColliders)
        {
            collider.isTrigger = true;
        }
        gripRigidbody.useGravity = false;

        UpdateState(GripState.Grabbing);

        audioSource.PlayRandomPitchOneShot(gripAudioClip, randomPitchBounds);
        SetInteractionEnabled(false);

        return true;
    }

    /// <summary>
    ///     Public interface for releasing the object.
    /// </summary>
    public bool TryLeaveInventory(bool skipCooldown = false, Vector3? exitPosition = null, Vector3? exitImpulse = null)
    {
        if (currentHolder == null || currentHolder.TryToRemove(gameObject) == null) return false;

        GetDropped(skipCooldown, exitPosition, exitImpulse);
        return true;
    }

    public void GetDropped(bool skipCooldown = false, Vector3? exitPosition = null, Vector3? exitImpulse = null)
    {
        lastHolder = currentHolder;
        currentHolder = null;
        initialDistance = -1;

        IgnoreAtomCollisions(true);
        foreach (Collider collider in gripColliders)
        {
            collider.isTrigger = false;
        }
        gripRigidbody.useGravity = true;

        UpdateState(GripState.Releasing);

        if (exitPosition != null) transform.position = (Vector3)exitPosition;
        if (exitImpulse != null) gripRigidbody.AddForce((Vector3)exitImpulse, ForceMode.Impulse);

        StartInteractionCooldown(skipCooldown ? 0.1f : maxCooldown);
    }

    protected override void OnInteractionCooldownStart()
    {
        gameObject.SetLayer(Utilities.IGNORE_CAMERA_LAYER);
    }

    /// <summary>
    ///     Transition between Releasing and Idle states.
    /// </summary>
    protected override void OnInteractionCooldownEnd()
    {
        // Don't allow an object to become collide-able again while a blob is inside of it.
        GameObject[] atoms = GameObject.FindGameObjectsWithTag("Atom");
        foreach (GameObject atom in atoms)
        {
            foreach (Collider collider in gripColliders) {
                if (collider.bounds.Intersects(atom.GetComponent<Collider>().bounds))
                {
                    StartInteractionCooldown(maxCooldown);
                    return;
                }
            }
        }

        BecomeIdle();
    }

    private void BecomeIdle()
    {
        UpdateState(GripState.Idle);
        SetScale(1);
        IgnoreAtomCollisions(false);
    }

    /// <summary>
    ///     Enable or disable collisions between this object and all atoms.
    /// </summary>
    /// <param name="ignore">
    ///     <tt>true</tt> to disable collisions, <tt>false</tt> to enable them.
    /// </param>
    private void IgnoreAtomCollisions(bool ignore)
    {
        if (isIgnoringAtomCollisions == ignore)
            return;
        
        GameObject[] atoms = GameObject.FindGameObjectsWithTag("Atom");
        foreach (GameObject atom in atoms)
        {
            Collider atomCollider = atom.GetComponent<Collider>();
            foreach (Collider collider in gripColliders) {
                Physics.IgnoreCollision(collider, atomCollider, ignore);
            }
        }
        isIgnoringAtomCollisions = ignore;
    }

    /// <summary>
    ///     Shrink the object linearly between scale factors 1 and finalScaleFactor while the
    ///     object is being grabbed. The scale will monotonically decrease as distance decreases.
    /// </summary>
    /// <param name="dist">
    ///     The distance parameter, between 0 and initialDistance.
    /// </param>
    private void ShrinkByDistance(float dist)
    {
        float scale = Utilities.Clamp(
            dist * (1 - finalScaleFactor) / initialDistance + finalScaleFactor,
            0, 1
        );

        if (currentScaleFactor > scale) SetScale(scale);
    }

    private void SetScale(float scale) {
        transform.localScale = scale * initialScale;
        currentScaleFactor = scale;
    }

    /// <summary>
    ///     Grow the object linearly between scale factors finalScaleFactor and 1 while the
    ///     object is being released. The scale will monotonically increase as cooldown decreases.
    /// </summary>
    private void GrowByCooldown()
    {
        float scale = Utilities.Clamp(
            finalScaleFactor + (cooldownTime - maxCooldown) * (finalScaleFactor - 1) / cooldownScaleDuration,
            0, 1
        );

        if (currentScaleFactor < scale)
        {
            transform.localScale = scale * initialScale;
            currentScaleFactor = scale;
        }
    }

    private void UpdateState(GripState newState)
    {
        if (newState != GripState.Held)
            gameObject.SetLayer(Utilities.DEFAULT_LAYER);

        gripState = newState;
    }

    public override string ToString()
    {
        return string.Format("{0} (Burden: {1}, GripState: {2})", gameObject.name, burden, gripState.ToString());
    }
}
