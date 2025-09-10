using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
///    This class defines the behavior of each individual atom in the blob.
/// </summary>
public class AtomController : MonoBehaviour
{
    public BlobController blobController;
    public Squisher squisher;
    public Vector3 force;
    public Vector3 impulse;
    public int touchCount = 0;

    private Rigidbody rigidBody;
    private HashSet<GameObject> touching = new HashSet<GameObject>();

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        squisher = blobController.GetComponent<Squisher>();
    }

    void FixedUpdate()
    {
        Assert.IsTrue(touchCount >= 0);

        rigidBody.AddForce(force, ForceMode.Force);
        rigidBody.AddForce(impulse, ForceMode.Impulse);
        impulse = Vector3.zero;
    }

    public void useGravity(bool use)
    {
        if (rigidBody != null)
            rigidBody.useGravity = use;
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.gameObject;

        if (NotAtom(obj))
        {
            squisher.squish();

            if (NotBounds(obj) && !touching.Contains(obj))
            {
                touchedHazard(obj);
                touchedGoal(obj);
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

    void touchedHazard(GameObject obj)
    {
        if (obj.tag == "Hazard")
        {
            blobController.Lose();
        }
    }

    void touchedGoal(GameObject obj)
    {
        if (obj.tag == "Start Platform")
        {
            blobController.Win(obj);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        GameObject obj = collision.gameObject;
        if (NotAtom(obj) && NotBounds(obj) && touching.Contains(obj))
        {
            touching.Remove(obj);
            touchCount--;
        }
    }

    bool NotAtom(GameObject obj)
    {
        return !blobController.blobAtoms.Contains(obj);
    }

    bool NotBounds(GameObject obj)
    {
        return obj.tag != "Bounds";
    }
}
