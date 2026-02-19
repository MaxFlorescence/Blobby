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
    // PUBLIC MEMBERS
    /// <summary>
    ///     The multiplier for the object's scale once it is grabbed.
    /// </summary>
    public float finalScaleFactor = 0.5f;
    /// <summary>
    ///    The sound played when the object is grabbed.
    /// </summary>
    public AudioClip gripSound;
    /// <summary>
    ///    The range of possible pitches for <tt>gripSound</tt>.
    /// </summary>
    public Vector2 randomPitchBounds = new(0.8f, 1.2f);
    /// <summary>
    ///    The factor by which a blob's motion affects the object's spin while grabbed.
    /// </summary>
    public float spinFactor = 0.1f;
    /// <summary>
    ///    The cost of carrying the object, charged to a blob's carrying capacity.
    /// </summary>
    public int burden = 1;

    // PRIVATE MEMBERS
    // Movement
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

    // Scaling
    /// <summary>
    ///    The current multiplier for the object's scale, between finalScale and 1.
    /// </summary>
    private float currentScaleFactor = 1f;
    /// <summary>
    ///    The initial scale Vector3 of the object.
    /// </summary>
    private Vector3 initialScale;

    // Cooldown
    /// <summary>
    ///     The duration of the grabbing cooldown period.
    /// </summary>
    private float maxCooldown = 3f;
    /// <summary>
    ///     How long it takes for the object's scale to return to normal after releasing.
    /// </summary>
    private float cooldownScaleDuration = 0.5f;

    // State
    /// <summary>
    ///     The current state of the gripped object.
    /// </summary>
    private GripState gripState = GripState.Idle;
    /// <summary>
    ///    The blob character currently grabbing this object, or null if none.
    /// </summary>
    private BlobController grabbedBy;
    private bool isIgnoringAtomCollisions = false;

    // Components
    private Collider[] gripColliders;
    private Rigidbody gripRigidbody;
    protected AudioSource audioSource;

    protected virtual void Start()
    {
        gripColliders = GetComponentsInChildren<Collider>();
        gripRigidbody = GetComponent<Rigidbody>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.8f;
        initialScale = transform.localScale;

        if (gripSound == null)
        {
            gripSound = Resources.Load("Sounds/bubbles", typeof(AudioClip)) as AudioClip;
        }
    }

    /// <summary>
    ///     Handle movement during physics updater while in Grabbing or Held states.
    /// </summary>
    public void FixedUpdate()
    {
        if (GameInfo.GameStatus != GameState.PAUSED)
        {
            if (gripState == GripState.Grabbing)
            {
                Vector3 translation = grabbedBy.transform.position - transform.position;
                float distance = translation.magnitude;

                ShrinkByDistance(distance);

                if (distance > grabbingDistance)
                {
                    float move = Mathf.Max(grabbingDistance, movementFactor * distance);
                    transform.Translate(move * translation.normalized, relativeTo: Space.World);
                }
                else
                {
                    if (grabbedBy.TryToGrab(gameObject))
                    {
                        gripState = GripState.Held;
                        SetScale(finalScaleFactor);
                    }
                    else
                    {
                        Release();
                    }
                }
            }
            else if (gripState == GripState.Held)
            {
                transform.position = grabbedBy.transform.position;
                gripRigidbody.AddTorque(spinFactor * Vector3.Cross(Vector3.up, grabbedBy.GetVelocity()));
            }
        }
    }

    /// <summary>
    ///    Handle cooldown during regular update while in Releasing state.
    /// </summary>
    protected override void OnUpdate()
    {
        if (gripState == GripState.Releasing) // && CoolingDown() // iff GripState.Releasing
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
        if (blob.IsHolding(null) && blob.IsSticky() && blob.CanCarry(burden))
        {
            GrabBy(blob);
        }
    }

    protected void GrabBy(BlobController blob)
    {
        initialDistance = (blob.transform.position - transform.position).magnitude;
        grabbedBy = blob;

        foreach (Collider collider in gripColliders) {
            collider.isTrigger = true;
        }
        gripRigidbody.useGravity = false;

        gripState = GripState.Grabbing;

        audioSource.pitch = Random.Range(randomPitchBounds.x, randomPitchBounds.y);
        audioSource.PlayOneShot(gripSound);
        SetInteractionEnabled(false);
    }

    /// <summary>
    ///     Public interface for releasing the object.
    /// </summary>
    public void Release()
    {
        StartInteractionCooldown(maxCooldown);
    }

    /// <summary>
    ///    Start the releasing process, which lasts for the cooldown.
    /// </summary>
    protected override void OnInteractionCooldownStart()
    {
        grabbedBy = null;
        IgnoreAtomCollisions(true);
        initialDistance = -1;
        
        foreach (Collider collider in gripColliders) {
            collider.isTrigger = false;
        }
        gripRigidbody.useGravity = true;
        gripState = GripState.Releasing;
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
                    Release();
                    return;
                }
            }
        }

        gripState = GripState.Idle;
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
            foreach (Collider collider in gripColliders) {
                Physics.IgnoreCollision(collider, atom.GetComponent<Collider>(), ignore);
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
        float scale = dist * (1 - finalScaleFactor) / initialDistance + finalScaleFactor;
        if (scale < 0)
        {
            scale = 0;
        }
        if (scale > 1)
        {
            scale = 1;
        }

        if (currentScaleFactor > scale)
        {
            SetScale(scale);
        }
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
        float scale = finalScaleFactor + (cooldownTime - maxCooldown) * (finalScaleFactor - 1) / cooldownScaleDuration;
        if (scale < 0)
        {
            scale = 0;
        }
        if (scale > 1)
        {
            scale = 1;
        }

        if (currentScaleFactor < scale)
        {
            transform.localScale = scale * initialScale;
            currentScaleFactor = scale;
        }
    }

    public void SetVisible(bool visible)
    {        
        int newLayer = visible ? Utilities.DEFAULT_LAYER : Utilities.INVISIBLE_LAYER;

        gameObject.layer = newLayer;
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = newLayer;
        }
    }

    public override string ToString()
    {
        return string.Format("{0} (Burden: {1}, GripState: {2})", gameObject.name, burden, gripState.ToString());
    }
}
