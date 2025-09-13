using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SphereCollider))]
public class Cannon : Interactable
{
    private Vector3 cannonCenter = Vector3.up * 2.3f;
    public Vector3 direction = Vector3.up*3.7f + Vector3.forward*6.5f;

    private float speed = 80;
    private float firePower = 32f;
    private float cancelPower = 2f;
    private bool cannonLoaded = false;
    private BlobController blob = null;
    private Vector3 entryPosition = Vector3.zero;
    private AudioSource audioSource;
    private SphereCollider cannonCollider;

    void Start(){
        cannonCenter += transform.position;
        audioSource = GetComponent<AudioSource>();
        cannonCollider = GetComponent<SphereCollider>();
    }

    protected override void OnUpdate() {
        float angle = speed * Time.deltaTime;
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

    protected override void OnInteract(BlobController blob)
    { // Insert the blob
        cannonCollider.isTrigger = true;
        cannonLoaded = true;
        entryPosition = blob.GetPosition();
        this.blob = blob;

        blob.SetMovementInputEnabled(false);
        blob.SetGravity(false);
        blob.Teleport(cannonCenter);
        blob.ApplyForces(Vector3.zero, Vector3.zero, false);

        SetInteractionEnabled(false);
    }

    public void Fire() {
        if (cannonLoaded)
        {
            blob.Teleport(cannonCenter + direction);
            blob.SetGravity(true);
            blob.ApplyForces(null, firePower * direction.normalized, false);
            blob.SetMovementInputEnabled(true, 0.5f);

            blob = null;
            cannonLoaded = false;
            SetInteractionEnabled(true);
        }

        audioSource.Play();
        cannonCollider.isTrigger = false;
    }

    public void Cancel() {
        if (cannonLoaded)
        {
            blob.Teleport(entryPosition);
            blob.SetGravity(true);
            blob.ApplyForces(null, cancelPower * (entryPosition - cannonCenter).normalized, false);
            blob.SetMovementInputEnabled(true, 0.5f);

            blob = null;
            cannonLoaded = false;
            StartInteractionCooldown(3f);
        }

        cannonCollider.isTrigger = false;
    }
}
