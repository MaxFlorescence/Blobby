using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

/// <summary>
///     A class for controlling the actions of a cannon object.
/// </summary>
public class Cannon : Interactable, Controllable
{
    //----------------------------------------------------------------------------------------------
    // COMPONENTS
    //----------------------------------------------------------------------------------------------
    private Rigidbody cannonRigidBody;
    public Transform barrelTransform;
    private AudioSource audioSource;
    /// <summary>
    ///     Used to apply a cooldown between firing or loading actions
    /// </summary>
    private Timer controlCooldownTimer;

    //----------------------------------------------------------------------------------------------
    // AIMING
    //----------------------------------------------------------------------------------------------
    public bool controlled { get; set; } = false;
    /// <summary>
    ///     The angle that the cannon's barrel makes with the horizontal.
    /// </summary>
    private float angle = 0;
    /// <summary>
    ///     The minimum and maximum angles that the barrel can make with the horizontal.
    /// </summary>
    private readonly Vector2 ANGLE_BOUNDS = new(-30, 30);
    /// <summary>
    ///     How fast the barrel rotates during aiming.
    /// </summary>
    private float angularSpeedFactor = 0.5f;

    //----------------------------------------------------------------------------------------------
    // LOADING
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The position at which ammo sits in the barrel.
    /// </summary>
    public Transform ammoPlaceholder;
    /// <summary>
    ///     The inventory that holds ammo.
    /// </summary>
    private Inventory barrel;
    /// <summary>
    ///     The blob currently in the cannon.
    /// </summary>
    private BlobController blob = null;
    /// <summary>
    ///     The name of the audio clip to play when ammo is loaded into the cannon.
    /// </summary>
    private const string LOAD_AUDIO = "cannon_load";
    /// <summary>
    ///     The factor by which a loaded blob is squished to fit into the cannon.
    /// </summary>
    private const float LOADED_BLOB_SIZE_FACTOR = 0.3f;

    //----------------------------------------------------------------------------------------------
    // FIRING  
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The location of the barrel's muzzle.
    /// </summary>
    public Transform muzzlePlaceholder;
    /// <summary>
    ///    The force magnitude with which to fire the cannon.
    /// </summary>
    public float firePower = 32f;
    /// <summary>
    ///     The multiplier to apply to the firing force when firing non-blob ammo.
    /// </summary>
    private float ammoForceFactor = 10f;
    /// <summary>
    ///    The force magnitude with which to exit the cannon.
    /// </summary>
    private float cancelPower = 2f;
    /// <summary>
    ///     The audio clip to play when the cannon fires.
    /// </summary>
    private AudioClip fireAudioClip;
    private const string FIRE_AUDIO = "cannon_fire";

    void Start(){
        audioSource = gameObject.AddComponent<AudioSource>();
        cannonRigidBody = GetComponent<Rigidbody>();
        
        barrel = gameObject.AddComponent<Inventory>();
        barrel.SetCapacity(1);
        barrel.SetDisplayPositionCallback(() => ammoPlaceholder.position);
        barrel.SetAudio(LOAD_AUDIO);
        fireAudioClip = Resources.Load<AudioClip>(Utilities.SOUNDS_PATH + FIRE_AUDIO);

        controlCooldownTimer = new(1.5f);
    }

    /// <summary>
    ///     Keep the loaded blob in the ammo position, and if it enters ghost mode then eject it.
    ///     Also handle controls for rotating the cannon and checking for input to fire or eject.
    /// </summary>
    protected override void OnUpdate()
    {
        if (blob != null)
        {
            blob.Teleport(ammoPlaceholder.position);

            if (blob.ghostMode) {
                StartInteractionCooldown(0.5f);
                controlled = false;
                blob.SetControlCanRelease(true);
                gameObject.SetLayer(Utilities.DEFAULT_LAYER);
                blob = null;   
            }
        }

        if (!controlled) return;
        
        float verticalInput = Input.GetAxis("Vertical");
        if (verticalInput != 0) AimBarrel(verticalInput * angularSpeedFactor);

        // use a cooldown timer between loading / unloading tasks
        if (!controlCooldownTimer.Update(reset: false)) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (barrel.IsEmpty())
            { // load ammo
                barrel.TryTakeFrom(blob.inventory);
            }
            else
            { // unload ammo
                barrel.TryGiveTo(blob.inventory);
            }
            controlCooldownTimer.Reset();
        }
        else if (Input.GetButtonDown("Jump"))
        {
            Fire();
            controlCooldownTimer.Reset();
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Eject();
        }
    }

    /// <summary>
    ///     Insert the interacting blob into the cannon and keep the cannon stable.
    /// </summary>
    protected override void OnInteract(BlobController blob)
    {
        if (this.blob != null || !blob.IsSticky()) return;
        
        Reorient();

        this.blob = blob;
        controlled = true;

        blob.SetControlCanRelease(false);
        blob.SetRestrained(true, LOADED_BLOB_SIZE_FACTOR);

        gameObject.SetLayer(Utilities.IGNORE_CAMERA_LAYER);
        SetInteractionEnabled(false);

        AllowFreeRotation(false);
    }

    /// <summary>
    ///    Fire the ammo or blob out of the cannon in the direction the cannon barrel is facing.
    /// </summary>
    public void Fire()
    {
        Vector3 firePosition = muzzlePlaceholder.position;
        Vector3 fireForce = firePower * GetFireDirection();

        if (!barrel.IsEmpty())
        {
            Grip ammo = barrel.GetObject().GetComponent<Grip>();
            ammo.TryLeaveInventory(true, firePosition, fireForce*ammoForceFactor);
        }
        else if (blob != null)
        {
            RemoveBlob(firePosition, fireForce);
        }

        audioSource.PlayOneShot(fireAudioClip);
    }

    /// <summary>
    ///     Eject the currently inserted blob from the cannon without firing.
    /// </summary>
    public void Eject()
    {
        if (blob == null) return;
        
        Vector3 worldEjectPosition = muzzlePlaceholder.position;
        Vector3 cancelForce = cancelPower * (worldEjectPosition - ammoPlaceholder.position).normalized;
        RemoveBlob(worldEjectPosition, cancelForce);
    }

    /// <summary>
    ///     Remove the loaded blob from the cannon and place it at the given exit position, imparted
    ///     with the given exit force. Also stop keeping the cannon stable and reset the barrel
    ///     angle if necessary.
    /// </summary>
    /// <param name="exitPosition">
    ///     The position at which the blob will exit the cannon.
    /// </param>
    /// <param name="exitForce">
    ///     The force with which the blob will exit the cannon.
    /// </param>
    /// <param name="resetAngle">
    ///     If <tt>true</tt>, reset the angle of the cannon's barrel.
    /// </param>
    private void RemoveBlob(Vector3 exitPosition, Vector3 exitForce, bool resetAngle = false)
    {
        if (resetAngle) AimBarrel();

        StartInteractionCooldown(3f);
        controlled = false;
        gameObject.SetLayer(Utilities.DEFAULT_LAYER);

        blob.SetControlCanRelease(true);
        blob.Teleport(exitPosition);
        blob.ApplyForces(null, exitForce, false);

        blob.SetRestrained(false, delaySpringUnlock: 0.25f);

        blob = null;
        AllowFreeRotation(true);
    }

    /// <returns>
    ///     The unit vector that points from the ammo position toward the muzzle position.
    /// </returns>
    private Vector3 GetFireDirection()
    {
        return (muzzlePlaceholder.position - ammoPlaceholder.position).normalized;
    }

    /// <summary>
    ///     Adjust the position and rotation of the cannon to be upright on the ground.
    /// </summary>
    /// <param name="resetAngle">
    ///     If <tt>true</tt>, reset the angle of the cannon's barrel.
    /// </param>
    private void Reorient(bool resetAngle = false)
    {
        Vector3 aimDir = Vector3.ProjectOnPlane(
            muzzlePlaceholder.position - transform.position, Vector3.up
        );

        if (resetAngle) AimBarrel();

        transform.LookAt(aimDir + transform.position, Vector3.up);
        transform.Translate(Vector3.up * 0.1f);
    }

    /// <summary>
    ///     Rotate the cannon's barrel by the given degrees. Negative values result in upward
    ///     rotation.
    /// </summary>
    /// <param name="angleDelta">
    ///     The angle by which to rotate the barrel. Negative values result in upward rotation.
    /// </param>
    private void AimBarrel(float angleDelta = 0)
    {
        if (angleDelta == 0) angleDelta = -angle;

        if (!(angle + angleDelta).OutOfBounds(ANGLE_BOUNDS.y, ANGLE_BOUNDS.x))
        {
            angle += angleDelta;
            barrelTransform.Rotate(Vector3.forward, angleDelta);
        }
    }

    /// <summary>
    ///     Update the constraints on the cannon's rigid body to limit or unlimit rotation in the X
    ///     and Z planes.
    /// </summary>
    /// <param name="allow">
    ///     Limit rotation iff this is <tt>true</tt>.
    /// </param>
    private void AllowFreeRotation(bool allow)
    {
        cannonRigidBody.constraints = allow ?
            RigidbodyConstraints.None :
            RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
}
