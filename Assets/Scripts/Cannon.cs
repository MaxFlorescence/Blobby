using UnityEngine;

public class Cannon : MonoBehaviour, Interactable
{
    private Vector3 cannonBase;
    private Vector3 baseToCenter = Vector3.up * 2.3f;
    public Vector3 direction = Vector3.up*3.7f + Vector3.forward*6.5f;

    private float speed = 80;
    private float power = 32f;
    private bool cannonLoaded = false;
    private BlobController blob = null;
    private AudioSource audioSource;
    private SphereCollider cannonCollider;

    void Start(){
        cannonBase = transform.position;
        audioSource = GetComponent<AudioSource>();
        cannonCollider = GetComponent<SphereCollider>();
    }

    void Update(){
        float angle = speed * Time.deltaTime;
        transform.Rotate(Vector3.up, angle);
        direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;

        if (cannonLoaded && Input.GetButtonDown("Jump")) {
            Fire();
        }
    }

    public void OnInteract(BlobController blob)
    { // Insert the blob
        cannonCollider.isTrigger = true;

        this.blob = blob;
        cannonLoaded = true;
        blob.SetInputEnabled(false);
        blob.SetGravity(false);
        blob.Teleport(cannonBase + baseToCenter);
        blob.ApplyForces(Vector3.zero, Vector3.zero, false);
    }

    public void Fire() {
        if (cannonLoaded)
        {
            blob.Teleport(cannonBase + baseToCenter + direction);
            blob.SetGravity(true);
            blob.ApplyForces(null, power * direction.normalized, false);
            blob.SetInputEnabled(true, 0.5f);

            blob = null;
            cannonLoaded = false;
        }

        audioSource.Play();
        cannonCollider.isTrigger = false;
    }
}
