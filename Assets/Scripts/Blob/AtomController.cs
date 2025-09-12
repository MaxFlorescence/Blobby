using System.Collections.Generic;
using UnityEngine;

/// <summary>
///    This class defines the behavior of each individual atom in the blob.
/// </summary>
public class AtomController : MonoBehaviour
{
    // PUBLIC MEMBERS
    public BlobController blobController;

    // PRIVATE MEMBERS
    /// <summary>
    ///     Makes squishy noises on collisions.
    /// </summary>
    private Squisher squisher;
    private Rigidbody rigidBody;
    /// <summary>
    ///     Typical force vector to apply every fixed update.
    /// </summary>
    private Vector3 force;
    /// <summary>
    ///     Impulse force vector to apply every fixed update. Set to zero at the end of each one.
    /// </summary>
    private Vector3 impulse;
    /// <summary>
    ///     Set of objects that this atom is currently colliding with. Movement input only registers 
    ///     if this is non-empty.
    /// </summary>
    private HashSet<GameObject> touching = new HashSet<GameObject>();

    /// <summary>
    ///     Initialize rigidbody and audio.
    /// </summary>
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        squisher = blobController.GetComponent<Squisher>();
    }

    /// <summary>
    ///     Apply forces to the atom. Impulse forces reset immediately.
    /// </summary>
    void FixedUpdate()
    {
        rigidBody.AddForce(force, ForceMode.Force);
        rigidBody.AddForce(impulse, ForceMode.Impulse);
        impulse = Vector3.zero;
    }

    /// <summary>
    ///    Enable or disable gravity for this atom.
    /// </summary>
    /// <param name="gravity">
    ///     <tt>true</tt> enables gravity, <tt>false</tt> disables it.
    /// </param>
    public void SetGravity(bool gravity)
    {
        rigidBody.useGravity = gravity;
    }

    /// <summary>
    ///    Set the velocity of this atom.
    /// </summary>
    /// <param name="velocity">
    ///     The new velocity for the atom.
    /// </param>
    public void SetVelocity(Vector3 velocity)
    {
        rigidBody.velocity = velocity;
    }

    /// <summary>
    ///     Handle activating interactable objects, grabbable objects, and tracking the touch count.
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.gameObject;

        if (!blobController.IsAtom(obj))
        { // do nothing special when colliding with other atoms
            squisher.squish();

            // Don't interact with boundaries or already-touched objects.
            // Boundaries do not affect the touch count to prevent the blob from moving solely by
            // touching them. This prevents players from skipping sections by moving along the boundaries.
            if (NotBounds(obj) && !touching.Contains(obj))
            {
                if (obj.TryGetComponent<Interactable>(out var interactableObj))
                {
                    interactableObj.Interact(blobController);
                }

                if (!blobController.IsHolding(obj))
                { // don't count grabbed objects as touching
                    touching.Add(obj);
                }
            }
        }
    }

    /// <summary>
    ///     Keep track of the touch count.
    /// </summary>
    void OnCollisionExit(Collision collision)
    {
        GameObject obj = collision.gameObject;
        if (!blobController.IsAtom(obj) && NotBounds(obj) && touching.Contains(obj))
        {
            touching.Remove(obj);
        }
    }

    /// <summary>
    ///    Return true if the given object is not a boundary object.
    /// </summary>
    /// <param name="obj">
    ///     The GameObject to check.
    /// </param>
    /// <returns>
    ///     <tt>false</tt> iff the object is a boundary object.
    /// </returns>
    bool NotBounds(GameObject obj)
    {
        return !obj.CompareTag("Bounds");
    }

    public void SetForce(Vector3 force)
    {
        this.force = force;
    }
    public void SetImpulse(Vector3 impulse)
    {
        this.impulse = impulse;
    }
    public int GetTouchCount()
    {
        return touching.Count;
    }
}
