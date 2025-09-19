using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SphereCollider))]
public class Cannon : Interactable
{
    // Properties  
    /// <summary>
    ///     The direction the cannon barrel faces.
    /// </summary>
    private Vector3 direction = Vector3.up * 3.7f + Vector3.forward * 6.5f;
    /// <summary>
    ///    The position of the cannon center.
    /// </summary>
    private Vector3 cannonCenter = Vector3.up * 2.3f;
    /// <summary>
    ///    The speed at which the cannon rotates (degrees per second).
    /// </summary>
    private float angularSpeed = 80;

    // Exiting
    /// <summary>
    ///    The force magnitude with which to fire the cannon.
    /// </summary>
    private float firePower = 32f;
    /// <summary>
    ///    The force magnitude with which to exit the cannon.
    /// </summary>
    private float cancelPower = 2f;

    // Loading
    private bool cannonLoaded = false;
    /// <summary>
    ///     The blob currently in the cannon, or null if none.
    /// </summary>
    private BlobController blob = null;
    /// <summary>
    ///     The position the blob entered the cannon from.
    /// </summary>
    private Vector3 entryPosition = Vector3.zero;

    // Components
    /// <summary>
    ///     The audio source to play when the cannon fires.
    /// </summary>
    private AudioSource cannonFireAudio;
    private SphereCollider cannonCollider;

    void Start(){
        cannonCenter += transform.position;
        cannonFireAudio = GetComponent<AudioSource>();
        cannonCollider = GetComponent<SphereCollider>();
    }

    /// <summary>
    ///    Rotate the cannon and check for input to fire or cancel.
    /// </summary>
    protected override void OnUpdate()
    {
        float angle = angularSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, angle);
        direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;

        if (cannonLoaded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Fire();
            }
            else if (Input.GetKeyDown("left shift"))
            {
                Cancel();
            }

        }
    }

    /// <summary>
    ///     Insert the interacting blob into the cannon and keep it still.
    /// </summary>
    protected override void OnInteract(BlobController blob)
    {
        if (!cannonLoaded)
        {
            cannonCollider.isTrigger = true;
            cannonLoaded = true;
            entryPosition = blob.GetPosition();
            this.blob = blob;

            blob.SetMovementInputEnabled(false);
            blob.SetGravity(false);
            blob.Teleport(cannonCenter);
            blob.ApplyForces(Vector3.zero, Vector3.zero, false);
            blob.HoldCenterAtom(true);

            SetInteractionEnabled(false);
        }
    }

    /// <summary>
    ///    Fire the blob out of the cannon in the direction the cannon is facing.
    /// </summary>
    public void Fire()
    {
        if (cannonLoaded)
        {
            blob.Teleport(cannonCenter + direction);
            blob.SetGravity(true);
            blob.ApplyForces(null, firePower * direction.normalized, false);
            blob.SetMovementInputEnabled(true, 0.5f);
            blob.HoldCenterAtom(false);

            blob = null;
            cannonLoaded = false;
            SetInteractionEnabled(true);
        }

        cannonFireAudio.Play();
        cannonCollider.isTrigger = false;
    }

    /// <summary>
    ///     Eject the currently inserted blob from the cannon without firing.
    /// </summary>
    public void Cancel()
    {
        if (cannonLoaded)
        {
            blob.Teleport(entryPosition);
            blob.SetGravity(true);
            blob.ApplyForces(null, cancelPower * (entryPosition - cannonCenter).normalized, false);
            blob.SetMovementInputEnabled(true, 0.5f);
            blob.HoldCenterAtom(false);

            blob = null;
            cannonLoaded = false;
            StartInteractionCooldown(3f);
        }

        cannonCollider.isTrigger = false;
    }
}
