using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
///    This class defines the behavior of each individual atom in the blob.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class AtomController : MonoBehaviour, IOverridable<Vector3>
{
    //----------------------------------------------------------------------------------------------
    // COMPONENTS
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The controller of this atom's blob.
    /// </summary>
    public BlobController blobController;
    public AtomCollection atoms;
    /// <summary>
    ///     The controller for how this atom's blob sticks to things.
    /// </summary>
    public AtomStickyController stickyController;
    /// <summary>
    ///     The rigidbody of this atom.
    /// </summary>
    private Rigidbody atomRigidBody;
    /// <summary>
    ///     The collider of this atom.
    /// </summary>
    private Collider atomCollider;
    private readonly Timer collideSoundTimer = new(1);

    //----------------------------------------------------------------------------------------------
    // MOVEMENT
    //----------------------------------------------------------------------------------------------
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
    public HashSet<GameObject> touching = new();

    //----------------------------------------------------------------------------------------------
    // MESH
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The location of this atom's vertex. If null, then use the atom's position instead.
    /// </summary>
    private Vector3? overrideVertex = null;

    /// <summary>
    ///     The previously calculated vertex for this atom, if it exists.
    /// </summary>
    private Vector3? vertexCache = null;

    /// <summary>
    ///     The vertex calculated before the vertexCache was, if it exists.
    /// </summary>
    private Vector3? lastVertexCache = null;

    public bool IsOverridden { get => overrideVertex != null; }

    //----------------------------------------------------------------------------------------------
    // STICKING
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     Dynamically created joint to allow this atom to be stuck to an object.
    /// </summary>
    public SpringJoint StickyJoint { get; set; }

    //----------------------------------------------------------------------------------------------
    // PARTICLES
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     <tt>True</tt> iff this atom is the center atom of its blob.
    /// </summary>
    public bool centerAtom = false;
    public AtomParticleController ParticleController;

    //----------------------------------------------------------------------------------------------
    // DEBUG MODE
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The renderer for the atom's mesh.
    /// </summary>
    private MeshRenderer meshRenderer;
    /// <summary>
    ///     The materials for the atom's mesh when its not sticking to anything.
    /// </summary>
    public Material[] dullMaterials;
    /// <summary>
    ///     The materials for the atom's mesh when its sticking to something.
    /// </summary>
    public Material[] neonMaterials;

    void Awake() {
        gameObject.TryGetComponent(out ParticleController);
    }

    void Start()
    {
        atomRigidBody = GetComponent<Rigidbody>();
        atomCollider = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        Highlight(false);

        SetVisible(false);
    }

    void Update()
    {
        collideSoundTimer.Update(mode: TimerMode.Toggle);
    }

    /// <summary>
    ///     Apply forces to the atom. Impulse forces reset immediately.
    /// </summary>
    void FixedUpdate()
    {
        atomRigidBody.AddForce(force, ForceMode.Force);
        atomRigidBody.AddForce(impulse, ForceMode.Impulse);
        impulse = Vector3.zero;
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.gameObject;

        if (!blobController.atoms.Contains(obj))
        { // do nothing special when colliding with other atoms
            if (!collideSoundTimer.Running) {
                blobController.SoundController.CollideSound();
                collideSoundTimer.Reset();
            }

            // Boundaries do not affect the touch count to prevent the blob from moving solely by
            // touching them. This prevents players from skipping sections by moving along the boundaries.
            if (NotBounds(obj) && !touching.Contains(obj))
            {
                // keep track of the touch count
                if (!blobController.Inventory.Contains(obj) && atomCollider.enabled)
                { // don't count grabbed objects as touching
                    touching.Add(obj);
                }

                if (obj.TryGetComponent<Interactable>(out var interactableObj))
                {
                    interactableObj.Interact(blobController);
                }
                
                if (interactableObj == null || interactableObj.GetInteractionEnabled())
                {
                    stickyController.TrySticking(this, collision.rigidbody);
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        GameObject obj = collision.gameObject;
    
        // keep track of the touch count
        if (!blobController.atoms.Contains(obj) && NotBounds(obj) && touching.Contains(obj))
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
    private bool NotBounds(GameObject obj)
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
    ///     (If object is not null) True iff atom is touching the object.
    ///     <br/>
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
    ///     Makes sure the blob controller unsticks if the sticky joint for this atom breaks.
    /// </summary>
    void OnJointBreak(float breakForce)
    {
        stickyController.Unstick(this);
    }

    //----------------------------------------------------------------------------------------------
    // Getters & Setters
    //----------------------------------------------------------------------------------------------
    
    /// <returns>
    ///     The ideal world location for this atom. The blob mesh should use this value to deform
    ///     itself.
    /// </returns>
    public Vector3 GetVertex() {
        if (overrideVertex == null) return transform.position;

        if (vertexCache == null)
        {
            vertexCache = atoms.CenterTransform.TransformPoint(overrideVertex.Value);
            if (lastVertexCache != null)
            {
                vertexCache = (lastVertexCache + vertexCache) / 2;
            }
        }
        
        return vertexCache.Value;
    }

    /// <summary>
    ///     Clears the previously calculated vertex.
    /// </summary>
    public void ClearVertexCache()
    {
        lastVertexCache = vertexCache;
        vertexCache = null;
    }
    
    public void Translate(Vector3 translation)
    {
        transform.position += translation;
        if (atomRigidBody.interpolation != RigidbodyInterpolation.None) Physics.SyncTransforms();
    }
    
    public void SetGravity(bool gravity)
    {
        atomRigidBody.useGravity = gravity;
    }
    
    public void SetVelocity(Vector3 velocity)
    {
        atomRigidBody.velocity = velocity;
    }
    
    /// <summary>
    ///     Sets the force and impulse values that will affect this atom's rigidbody on the next
    ///     fixed update.
    /// </summary>
    public void SetForces(Vector3? force, Vector3? impulse)
    {
        if (force.HasValue) this.force = force.Value;
        if (impulse.HasValue) this.impulse = impulse.Value;
    }
    
    public void SetVisible(bool visible)
    {
        meshRenderer.enabled = visible;
    }

    /// <summary>
    ///     Highlight or unhighlight the atom. This is only visible in debug mode.
    /// </summary>
    public void Highlight(bool highlight)
    {
        meshRenderer.materials = highlight ? neonMaterials : dullMaterials;
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

        if (!enabled) touching.Clear();
    }

    /// <summary>
    ///     Not Implemented.
    /// </summary>
    public void SetValue(Vector3 newValue)
    {
        throw new NotImplementedException();
    }

    public void SetOverride(Vector3 newOverride)
    {
        overrideVertex = newOverride;
    }

    public void ClearOverride()
    {
        overrideVertex = null;
    }
}
