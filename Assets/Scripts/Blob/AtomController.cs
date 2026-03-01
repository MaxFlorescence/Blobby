using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
/// <summary>
///    This class defines the behavior of each individual atom in the blob.
/// </summary>
public class AtomController : MonoBehaviour
{
    //----------------------------------------------------------------------------------------------
    // PUBLIC MEMBERS
    //----------------------------------------------------------------------------------------------
    public BlobController blobController;

    //----------------------------------------------------------------------------------------------
    // PRIVATE MEMBERS
    //----------------------------------------------------------------------------------------------
    private Rigidbody atomRigidBody;
    private Collider atomCollider;
    /// <summary>
    ///     Makes squishy noises on collisions.
    /// </summary>
    private Squisher squisher;
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

    //----------------------------------------------------------------------------------------------
    // Sticking
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     Dynamically created joint to allow this atom to be stuck to an object.
    /// </summary>
    private SpringJoint stickyJoint;
    /// <summary>
    ///     Spring constant for when the atom sticks to an object.
    /// </summary>
    private const float STICKY_STRENGTH = 1000;
    /// <summary>
    ///     Force needed to break the joint between a sticky atom and an object.
    /// </summary>
    private const float BREAK_FORCE = 500;

    //----------------------------------------------------------------------------------------------
    // Particles
    //----------------------------------------------------------------------------------------------
    private ParticleSystem drips;
    private ParticleSystem.EmissionModule dripsEmission;
    /// <summary>
    ///     <tt>true</tt> iff this atom is a center atom. This disables drip particles.
    /// </summary>
    private bool centerAtom = false;

    //----------------------------------------------------------------------------------------------
    // Debug Mode
    //----------------------------------------------------------------------------------------------
    private MeshRenderer atomMeshRenderer;
    private Material[] atomMaterials;
    private Material[] stickyMaterials;

    void Awake() {
        drips = gameObject.AddComponent<ParticleSystem>();
        drips.Stop();

        atomMaterials = new Material[] {Resources.Load("Materials/Blob Materials/EyeSclera", typeof(Material)) as Material};
        stickyMaterials = new Material[] {Resources.Load("Materials/Blob Materials/Highlighted", typeof(Material)) as Material};
    }

    void Start()
    {
        atomRigidBody = GetComponent<Rigidbody>();
        atomCollider = GetComponent<Collider>();
        atomMeshRenderer = GetComponent<MeshRenderer>();
        atomMeshRenderer.materials = atomMaterials;
        squisher = blobController.GetComponent<Squisher>();

        SetupDripParticles();
        SetVisible(false);
    }

    /// <summary>
    ///     Add and configure the particle system component for dripping.
    /// </summary>
    private void SetupDripParticles()
    {
        if (centerAtom) return;

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
            renderer.mesh = blobController.dropletMesh;
            renderer.alignment = ParticleSystemRenderSpace.Velocity;
        }

        var inheritVelocity = drips.inheritVelocity;
        inheritVelocity.enabled = true;
        inheritVelocity.mode = ParticleSystemInheritVelocityMode.Initial;
        inheritVelocity.curveMultiplier = 1.5f;

        drips.Play();
    }

    /// <summary>
    ///     Apply forces to the atom. Impulse forces reset immediately.
    /// </summary>
    void FixedUpdate()
    {
        atomRigidBody.AddForce(force, ForceMode.Force);
        atomRigidBody.AddForce(impulse, ForceMode.Impulse);
        impulse = Vector3.zero;

        // UpdateDripParticles(); // Disabled to increase particles
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
        if (!blobController.IsTouching(obj) && obj.TryGetComponent<Interactable>(out var interactableObj))
        {
            interactableObj.InteractionEnd(blobController);
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
    ///     Is this atom touching an object?
    /// </summary>
    /// <param name="obj">
    ///     The object to test for. If null, test if the atom is touching anything.
    /// </param>
    /// <returns>
    ///     (If object is not null) True iff atom is touching the object.<br/>
    ///     (If object is null) True iff atom is touching anything.
    /// </returns>
    public bool IsTouching(GameObject obj = null)
    {
        if (obj == null)
        {
            return touching.Count > 0;
        }

        return touching.Contains(obj);
    }

    /// <summary>
    ///     Create a new spring joint to stick this atom to the <tt>obj</tt>.
    /// </summary>
    /// <param name="obj">
    ///     The object to stick this atom to.
    /// </param>
    public void StickTo(Rigidbody obj)
    {
        atomMeshRenderer.materials = stickyMaterials;

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

    public bool IsSticking()
    {
        return stickyJoint != null;
    }

    /// <summary>
    ///     Destroy the spring joint sticking this atom to an object.
    /// </summary>
    public void Unstick()
    {
        if (IsSticking())
        {
            atomMeshRenderer.materials = atomMaterials;
            Destroy(stickyJoint);
        }
    }

    void OnJointBreak(float breakForce)
    {
        atomMeshRenderer.materials = atomMaterials;
        blobController.Unstick(gameObject);
    }

    //----------------------------------------------------------------------------------------------
    // Getters
    //----------------------------------------------------------------------------------------------
    public BlobController GetBlobController()
    {
        return blobController;
    }
    //----------------------------------------------------------------------------------------------
    // Setters
    //----------------------------------------------------------------------------------------------
    public void SetGravity(bool gravity)
    {
        atomRigidBody.useGravity = gravity;
    }
    public void SetVelocity(Vector3 velocity)
    {
        atomRigidBody.velocity = velocity;
    }
    public void SetForce(Vector3 force)
    {
        this.force = force;
    }
    public void SetImpulse(Vector3 impulse)
    {
        this.impulse = impulse;
    }
    public void SetVisible(bool visible)
    {
        atomMeshRenderer.enabled = visible;
    }
    public void SetAsCenterAtom(bool isCenterAtom)
    {
        centerAtom = isCenterAtom;
    }
    /// <summary>
    ///     Change the material of each droplet particle.
    /// <param name="material">
    ///     The material to change to.
    /// </param>
    public void SetDropletMaterial(Material material)
    {
        if (centerAtom) return;

        if (drips.TryGetComponent<ParticleSystemRenderer>(out var renderer))
        {
            renderer.material = material;
        }
    }
    /// <summary>
    ///     Enables/disables the atom's collider. On disabling, the atom stops touching everything.
    /// </summary>
    /// <param name="enabled">
    ///     The new state for the atom's collider. If false, the atom to stops touching everything.
    /// </param>
    public void SetCollider(bool enabled)
    {
        atomCollider.enabled = enabled;

        if (!enabled) {
            touching.Clear(); 
        }
    }
}
