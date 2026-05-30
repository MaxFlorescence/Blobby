using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
///     A class for controlling a collection of atoms.
/// </summary>
public class AtomCollection : MonoBehaviour, IEnumerable
{
    /// <summary>
    ///     The atoms belonging to this collection.
    /// </summary>
    public Transform[] Transforms;

    public Transform this[int i] {
        get => Transforms[i];
    }

    /// <summary>
    ///     The number of atoms in this collection.
    /// </summary>
    public int Count => Transforms.Length;

    // ---------------------------------------------------------------------------------------------
    // COMPONENTS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The atom controllers of each atom in this collection.
    /// </summary>
    public AtomController[] Controllers { get; private set; }

    /// <summary>
    ///     The rigidbodies of each atom in this collection.
    /// </summary>
    public Rigidbody[] Rigidbodies { get; private set; }

    // ---------------------------------------------------------------------------------------------
    // CENTER ATOM
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The atom at the center of the blob.
    /// </summary>
    public Transform CenterTransform => Transforms[0];

    /// <summary>
    ///     The atom controller of this collection's center atom.
    /// </summary>
    public AtomController CenterController => Controllers[0];

    /// <summary>
    ///     The rigidbody of this collection's center atom.
    /// </summary>
    public Rigidbody CenterRigidbody => Rigidbodies[0];

    void Awake()
    {
        Controllers = (from a in Transforms select a.GetComponent<AtomController>()).ToArray();
        Rigidbodies = (from a in Transforms select a.GetComponent<Rigidbody>()).ToArray();
    }

    void Update()
    {
        SetAllVisible(GameInfo.DebugMode);
    }

    public IEnumerator GetEnumerator()
    {
        foreach (Transform atom in Transforms) yield return atom;
    }

    /// <param name="obj">
    ///     The GameObject to check.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the given object is one of this blob's atoms.
    /// </returns>
    public bool Contains(GameObject obj) {
        if (!obj.CompareTag("Atom")) return false;

        return Contains(obj.transform);
    }

    /// <param name="trans">
    ///     The Transform to check.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the given transform is one of this blob's atoms.
    /// </returns>
    public bool Contains(Transform trans)
    {
        return Transforms.Contains(trans);
    }
    
    /// <summary>
    ///     Freeze the blob's center atom's position or unfreeze it.
    /// </summary>
    /// <param name="freeze">
    ///     <tt>true</tt> to freeze the center atom, <tt>false</tt> to unfreeze it.
    /// </param>
    public void FreezeCenter(bool freeze)
    {
        CenterRigidbody.constraints = freeze ?
            RigidbodyConstraints.FreezePosition :
            RigidbodyConstraints.None;
    }
    
    /// <summary>
    ///     Are any of the blob's atoms touching an object?
    /// </summary>
    /// <param name="obj">
    ///     The object to test for. If <tt>null</tt>, tests if anything at all is being touched.
    /// </param>
    /// <returns>
    ///     (If object is not <tt>null</tt>) <tt>True</tt> iff the blob is touching the object.<br/>
    ///     (If object is <tt>null</tt>) <tt>True</tt> iff the blob is touching something.
    /// </returns>
    public bool AreAnyTouching(GameObject obj = null)
    {
        foreach (AtomController atom in Controllers)
        {
            if (atom.IsTouching(obj)) return true;
        }

        return false;
    }

    /// <typeparam name="T">
    ///     One of <tt>Transform</tt>, <tt>Rigidbody</tt>, or <tt>AtomController</tt>.
    /// </typeparam>
    /// <param name="atom"></param>
    /// <returns>
    ///     The index of the atom with the specified component.
    /// </returns>
    public int IndexOf<T>(T atom)
    {
        return atom switch
        {
            Transform => Array.IndexOf(Transforms, atom),
            Rigidbody => Array.IndexOf(Rigidbodies, atom),
            AtomController => Array.IndexOf(Controllers, atom),
            _ => -1
        };
    }

    public void ForEach(Action<AtomController> action)
    {
        foreach (AtomController atom in Controllers)
        {
            action(atom);
        }
    }

    public void ForEach(Action<Rigidbody> action)
    {
        foreach (Rigidbody atom in Rigidbodies)
        {
            action(atom);
        }
    }

    public void ForEach(Action<Transform> action)
    {
        foreach (Transform atom in Transforms)
        {
            action(atom);
        }
    }

    public void SetAllGravity(bool gravity)
    {
        ForEach(atom => atom.SetGravity(gravity));
    }

    public void SetVelocities(Vector3 velocity)
    {
        ForEach(atom => atom.SetVelocity(velocity));
    }

    public void TranslateAll(Vector3 translation)
    {
        ForEach((AtomController atom) => atom.Translate(translation));
    }

    public void SetAllForces(Vector3? force, Vector3? impulse)
    {
        ForEach(atom => atom.SetForces(force, impulse));
    }

    public void SetAllVisible(bool visible)
    {
        ForEach(atom => atom.SetVisible(visible));
    }

    public void SetDropletMaterials(Material material)
    {
        ForEach(atom => atom.SetDropletMaterial(material));
    }

    public void SetColliders(bool enabled)
    {
        ForEach(atom => atom.SetCollider(enabled));
    }

    public void ClearAllVertexCaches()
    {
        ForEach(atom => atom.ClearVertexCache());
    }
}