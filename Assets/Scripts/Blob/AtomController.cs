using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class AtomController : MonoBehaviour
{
    private Rigidbody rigidBody;
    private HashSet<GameObject> touching = new HashSet<GameObject>();
    public BlobController blobController;
    public int touchCount = 0;

    public Vector3 moveForce;
    public Vector3 jumpForce;
    public Squisher squisher;

    void Start()
    {
        // contactPoints = new ArrayList(10);
        rigidBody = GetComponent<Rigidbody>();
        squisher = blobController.GetComponent<Squisher>();
    }

    void FixedUpdate()
    {
        Assert.IsTrue(touchCount >= 0);
        
        rigidBody.AddForce(moveForce, ForceMode.Force);
        rigidBody.AddForce(jumpForce, ForceMode.Impulse);
    }

    public void useGravity(bool use) {
        if (rigidBody != null)
            rigidBody.useGravity = use;
    }

    // void ApplyJump() {
    //     Vector3 jumpDirection = Vector3.zero;

    //     // foreach (ContactPoint contact in contactPoints)
    //     //     jumpDirection += contact.normal;
        
    // }

    void OnCollisionEnter(Collision collision) {
        GameObject obj = collision.gameObject;

        if(NotAtom(obj)) {
            squisher.squish();

            if(NotBounds(obj) && !touching.Contains(obj)) {
                touchedHazard(obj);
                touchedGoal(obj);
                touchedCannon(obj);

                if (!touchedFlag(obj)) {
                    touching.Add(obj);
                    touchCount++;
                }
            }
        }
        // for (int i = 0; i < collision.contactCount; i++)
        //     contactPoints.Add(collision.GetContact(i));
    }

    void touchedHazard(GameObject obj) {
        if (obj.tag == "Hazard") {
            blobController.lose();
        }
    }

    bool touchedFlag(GameObject obj) {
        if (obj.tag == "Flag") {
            blobController.grabFlag(obj);
            return true;
        }

        return false;
    }

    void touchedCannon(GameObject obj) {
        if (obj.tag == "Cannon") {
            obj.GetComponent<Cannon>().Insert(blobController);
        }
    }

    void touchedGoal(GameObject obj) {
        if (obj.tag == "Start Platform") {
            blobController.win(obj);
        }
    }

    void OnCollisionExit(Collision collision) {
        GameObject obj = collision.gameObject;
        if(NotAtom(obj) && NotBounds(obj) && touching.Contains(obj)) {
            touching.Remove(obj);
            touchCount--;
        }
        // for (int i = 0; i < collision.contactCount; i++)
        //     contactPoints.Remove(collision.GetContact(i));
    }

    bool NotAtom(GameObject obj) {
        // return obj.tag != "Atom";
        return !blobController.blobAtoms.Contains(obj);
    }

    bool NotBounds(GameObject obj) {
        return obj.tag != "Bounds";
    }
}
