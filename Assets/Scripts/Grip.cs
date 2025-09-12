using UnityEngine;
using UnityEngine.UIElements;

public enum GripState
{
    Idle, Grabbing, Releasing, Held
}

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Grip : Interactable
{
    public float finalScale = 0.5f;
    public AudioSource gripSound;

    private float movementFactor = 0.1f;
    private float grabbingDistance = 0.2f;
    private float initialDistance = -1;
    private float currentScale = 1f;
    private float maxCooldown = 3f;
    private float cooldownScaleDuration = 0.5f;
    private GripState state = GripState.Idle;
    private Vector3 initialScale;
    private Collider gripCollider;
    private Rigidbody gripRigidbody;
    private BlobController grabbedBy;

    void Start()
    {
        gripCollider = GetComponent<Collider>();
        gripRigidbody = GetComponent<Rigidbody>();
        initialScale = transform.localScale;
    }

    public void FixedUpdate()
    {
        if (state == GripState.Grabbing)
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
                state = GripState.Held;
            }
        }
        else if (state == GripState.Held)
        {
            transform.position = grabbedBy.transform.position;
        }
    }

    protected override void OnUpdate()
    {
        if (state == GripState.Releasing) // && CoolingDown() // iff GripState.Releasing
        {
            GrowByCooldown();
        }
    }

    protected override void OnInteract(BlobController blob)
    {
        if (blob.TryToGrab(gameObject))
        {
            initialDistance = (blob.transform.position - transform.position).magnitude;
            grabbedBy = blob;

            gripCollider.isTrigger = true;
            gripRigidbody.useGravity = false;

            state = GripState.Grabbing;

            gripSound.Play();
            SetInteractionEnabled(false);
        }
    }

    public void Release()
    {
        initialDistance = -1;
        gripCollider.isTrigger = false;
        gripRigidbody.useGravity = true;
        grabbedBy = null;
        StartInteractionCooldown(maxCooldown);
    }

    protected override void OnInteractionCooldownStart()
    {
        IgnoreAtomCollisions(true);
        state = GripState.Releasing;
    }

    protected override void OnInteractionCooldownEnd()
    {
        IgnoreAtomCollisions(false);
        state = GripState.Idle;
    }

    private void IgnoreAtomCollisions(bool ignore)
    {
        GameObject[] atoms = GameObject.FindGameObjectsWithTag("Atom");
        foreach (GameObject atom in atoms)
        {
            Physics.IgnoreCollision(gripCollider, atom.GetComponent<Collider>(), ignore);
        }
    }

    private void ShrinkByDistance(float dist)
    {
        float scale = dist * (1 - finalScale) / initialDistance + finalScale;
        if (scale < 0)
        {
            scale = 0;
        }
        if (scale > 1)
        {
            scale = 1;
        }

        if (currentScale > scale)
        {
            transform.localScale = scale * initialScale;
            currentScale = scale;
        }
    }

    private void GrowByCooldown()
    {
        float scale = finalScale + (cooldownTime - maxCooldown) * (finalScale - 1) / cooldownScaleDuration;
        if (scale < 0)
        {
            scale = 0;
        }
        if (scale > 1)
        {
            scale = 1;
        }
        
        if (currentScale < scale)
        {
            transform.localScale = scale * initialScale;
            currentScale = scale;
        }
    }
}
