using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
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

    // Sticking
    private SpringJoint stickyJoint;
    /// <summary>
    ///     Spring constant for when the atom sticks to an object.
    /// </summary>
    private const float STICKY_STRENGTH = 1000;
    /// <summary>
    ///     Force needed to break the joint between a sticky atom and an object.
    /// </summary>
    private const float BREAK_FORCE = 500;

    // Particles
    private ParticleSystem drips;
    private const string DROPLET_MATERIAL = "Materials/Blob Materials/JellyTexture";
    private ParticleSystem.EmissionModule dripsEmission;
    /// <summary>
    ///     <tt>true</tt> iff this atom is a center atom. This disables drip particles.
    /// </summary>
    private bool centerAtom = false;

    /// <summary>
    ///     Initialize rigidbody and audio.
    /// </summary>
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        squisher = blobController.GetComponent<Squisher>();

        SetupDripParticles();
    }

    /// <summary>
    ///     Add and configre the particle system component for dripping.
    /// </summary>
    private void SetupDripParticles()
    {
        if (centerAtom) return;

        drips = gameObject.AddComponent<ParticleSystem>();
        drips.Stop();

        dripsEmission = drips.emission;
        dripsEmission.rateOverTime = 0.25f;
        dripsEmission.enabled = true;

        var main = drips.main;
        main.startLifetime = 2;
        main.loop = true;
        main.duration = 4;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0;
        main.gravityModifier = 1;
        main.startDelay = Random.Range(0f, 4f);

        var collision = drips.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.collidesWith = LayerMask.GetMask("Default");
        collision.dampen = 1;
        collision.bounce = 0;

        var sizeOverLifetime = drips.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new();
        curve.AddKey(0.00f, 1.00f);
        curve.AddKey(1.00f, 0.00f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1, curve);

        if (drips.TryGetComponent<ParticleSystemRenderer>(out var renderer))
        {
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = GetComponent<MeshFilter>().mesh;
            renderer.alignment = ParticleSystemRenderSpace.Velocity;
            renderer.material = blobController.GetMaterial();
        }

        var inheritVelocity = drips.inheritVelocity;
        inheritVelocity.enabled = true;
        inheritVelocity.mode = ParticleSystemInheritVelocityMode.Initial;
        inheritVelocity.curveMultiplier = 1.5f;

        drips.Play();
    }

    public void SetDropletMaterial(Material material)
    {
        if (drips.TryGetComponent<ParticleSystemRenderer>(out var renderer))
        {
            renderer.material = material;
        }
    }

    /// <summary>
    ///     Enables/disables drip particle emission if this atom is/isn't on the underside of the
    ///     blob.
    /// </summary>
    private void UpdateDripParticles()
    {
        if (centerAtom) return;

        Vector3 direction = transform.position - blobController.transform.position;
        dripsEmission.enabled = Vector3.Dot(direction, Vector3.down) > 0;
    }

    /// <summary>
    ///     Apply forces to the atom. Impulse forces reset immediately.
    /// </summary>
    void FixedUpdate()
    {
        rigidBody.AddForce(force, ForceMode.Force);
        rigidBody.AddForce(impulse, ForceMode.Impulse);
        impulse = Vector3.zero;

        // UpdateDripParticles();
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
    ///     Handle activating interactable objects, grabbable objects, sticking to objects,
    ///     and tracking the touch count.
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.gameObject;

        if (!blobController.IsAtom(obj))
        { // do nothing special when colliding with other atoms
            squisher.squish();

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

                if (interactableObj == null || interactableObj.GetInteractionEnabled())
                {
                    blobController.TrySticking(gameObject, obj);
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

    /// <summary>
    ///     Create a new spring joint to stick this atom to the <tt>obj</tt>.
    /// </summary>
    /// <param name="obj">
    ///     The object to stick this atom to.
    /// </param>
    public void StickTo(Rigidbody obj)
    {
        stickyJoint = gameObject.AddComponent<SpringJoint>();
        stickyJoint.connectedBody = obj;

        stickyJoint.enableCollision = true;
        stickyJoint.spring = STICKY_STRENGTH;
        stickyJoint.breakForce = BREAK_FORCE;

        // manually set anchor positions
        stickyJoint.autoConfigureConnectedAnchor = false;
        stickyJoint.anchor = Vector3.zero;
        stickyJoint.connectedAnchor = obj.transform.InverseTransformPoint(transform.position);
    }

    /// <summary>
    ///     Destroy the spring joint sticking this atom to an object.
    /// </summary>
    public void Unstick()
    {
        if (stickyJoint != null)
        {
            Destroy(stickyJoint);
        }
    }

    void OnJointBreak(float breakForce)
    {
        blobController.Unstick(gameObject);
    }

    // Getters and setters
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
    private void SetVisible(bool visible)
    {
        GetComponent<MeshRenderer>().enabled = visible;
    }
    public void SetCenterAtom(bool isCenterAtom)
    {
        centerAtom = isCenterAtom;
    }
}
