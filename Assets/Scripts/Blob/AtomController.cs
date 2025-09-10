using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
///    This class defines the behavior of each individual atom in the blob.
/// </summary>
public class AtomController : MonoBehaviour
{
    // Public members
    public BlobController blobController;
    public Squisher squisher;
    public Vector3 force;
    public Vector3 impulse;
    public int touchCount = 0;

    // Private members
    private Rigidbody rigidBody;
    private HashSet<GameObject> touching = new HashSet<GameObject>();

    /// <summary>
    ///     Initialize components.
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
        Assert.IsTrue(touchCount >= 0);

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
    ///     Handle activating interactable objects, grabbable objects, and tracking the touch count.
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.gameObject;

        if (NotMyAtoms(obj))
        { // do nothing special when colliding with other atoms
            squisher.squish();

            // Don't interact with boundaries or already-touched objects.
            // Boundaries do not affect the touch count to prevent the blob from moving solely by
            // touching them. This prevents players from skipping sections by moving along the boundaries.
            if (NotBounds(obj) && !touching.Contains(obj))
            {
                Interactable interactableObj = obj.GetComponent<Interactable>();
                if (interactableObj != null)
                {
                    interactableObj.OnInteract(blobController);
                }

                if (!blobController.TryToGrab(obj))
                { // don't count grabbed objects as touching
                    touching.Add(obj);
                    touchCount++;
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
        if (NotMyAtoms(obj) && NotBounds(obj) && touching.Contains(obj))
        {
            touching.Remove(obj);
            touchCount--;
        }
    }

    /// <summary>
    ///     Return true if the given object is not one of this blob's atoms.
    /// </summary>
    /// <param name="obj">
    ///     The GameObject to check.
    /// </param>
    /// <returns>
    ///     <tt>false</tt> iff the object is one of this blob's atoms.
    /// </returns>
    bool NotMyAtoms(GameObject obj)
    {
        return !blobController.blobAtoms.Contains(obj);
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
}
