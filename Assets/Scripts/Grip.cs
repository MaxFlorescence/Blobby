using UnityEngine;

public enum GripState
{
    Idle, Grabbing, Releasing, Held
}

public class Grip : MonoBehaviour
{
    public float finalScale = 0.5f;
    public AudioSource gripSound;

    private float movementFactor = 0.04f;
    private float finishingDistance = 0.05f;
    private float initialDistance = -1;
    private float currentScale = 1f;
    private float cooldown = 3f;
    private float maxCooldown = 3f;
    private float finishCooldownScale = 0.5f;
    private GripState state = GripState.Idle;
    private Vector3 initialScale;
    private Collider gripCollider;
    private Rigidbody gripRigidbody;
    private GameObject grabbedBy;

    void Start()
    {
        gripCollider = GetComponent<Collider>();
        gripRigidbody = GetComponent<Rigidbody>();
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (state == GripState.Grabbing)
        {
            Vector3 translation = grabbedBy.transform.position - transform.position;

            if (translation.magnitude <= finishingDistance)
            {
                transform.position = grabbedBy.transform.position;
                transform.rotation = grabbedBy.transform.rotation;
                state = GripState.Held;
            }
            else
            {
                ScaleByDistance(translation);
                transform.Translate(movementFactor * translation.normalized, relativeTo: Space.World);
            }
        }
        else if (state == GripState.Releasing)
        {
            if (cooldown >= maxCooldown)
            {
                IgnoreAtomCollisions(false);
                state = GripState.Idle;
            }
            else
            {
                ScaleByCooldown();
                cooldown += Time.deltaTime;
            }
        }
        else if (state == GripState.Held)
        {
            transform.position = grabbedBy.transform.position;
        }
    }

    public bool GetGrabbed(GameObject obj)
    {
        if (cooldown >= maxCooldown)
        {
            initialDistance = (obj.transform.position - transform.position).magnitude;
            grabbedBy = obj;

            gripCollider.isTrigger = true;
            gripRigidbody.useGravity = false;

            state = GripState.Grabbing;

            gripSound.Play();

            return true;
        }
        return false;
    }

    public void GetReleased()
    {
        initialDistance = -1;
        gripCollider.isTrigger = false;
        gripRigidbody.useGravity = true;
        grabbedBy = null;
        cooldown = 0;
        IgnoreAtomCollisions(true);

        state = GripState.Releasing;
    }

    private void IgnoreAtomCollisions(bool ignore)
    {
        GameObject[] atoms = GameObject.FindGameObjectsWithTag("Atom");
        foreach (GameObject atom in atoms)
        {
            Physics.IgnoreCollision(gripCollider, atom.GetComponent<Collider>(), ignore);
        }
    }

    private void ScaleByDistance(Vector3 diff)
    {
        float scale = diff.magnitude * (1 - finalScale) / initialDistance + finalScale;
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

    private void ScaleByCooldown()
    {
        float scale = cooldown * (1 - finalScale)/finishCooldownScale + finalScale;
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
