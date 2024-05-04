using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    private Vector3 cannonBase;
    private Vector3 baseToCenter = Vector3.up * 2.3f;
    public Vector3 direction = Vector3.up*3.7f + Vector3.forward*6.5f;

    private float speed = 80;
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

        if (blob != null) {
            blob.teleport(cannonBase + baseToCenter);
        }
    }

    public void Insert(BlobController blob) {
        cannonCollider.enabled = false;

        this.blob = blob;
        blob.cannon = this;
    }

    public void Fire() {
        if (blob != null) {
            audioSource.Play();
            blob.teleport(cannonBase + baseToCenter + direction);

            blob.cannon = null;
            blob = null;
        }

        cannonCollider.enabled = true;
    }
}
