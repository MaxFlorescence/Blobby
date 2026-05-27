using UnityEngine;

/// <summary>
///     A class that controls stickiness for the atoms of a blob.
/// </summary>
public class AtomStickyController : MonoBehaviour, IOverridable<bool>
{
    //----------------------------------------------------------------------------------------------
    // STICKING
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     How many atoms can be sticky at once.
    /// </summary>
    private const int STICKY_COUNT = 2;
    /// <summary>
    ///     Spring constant for when the atom sticks to an object.
    /// </summary>
    private const float STICKY_STRENGTH = 1000;
    /// <summary>
    ///     Force needed to break the joint between a sticky atom and an object.
    /// </summary>
    private const float BREAK_FORCE = 500;
    /// <summary>
    ///     Index of the last sticky atom, or null if fewer than <tt>STICKY_COUNT</tt> atoms are
    ///     currently sticky.
    /// </summary>
    private int stickyHead = 0;
    /// <summary>
    ///     Circular buffer of capacity <tt>STICKY_COUNT</tt> for holding sticky atoms.
    /// </summary>
    private readonly AtomController[] atomStickies = new AtomController[STICKY_COUNT];
    /// <summary>
    ///     Indicates if atoms can become sticky. If <tt>false</tt>, no atoms are sticky.
    /// </summary>
    public bool Sticky { get; private set; } = false;
    private bool? savedSticky = null;

    /// <summary>
    ///     Try making the <tt>atom</tt> stick to the <tt>obj</tt>. If the <tt>atomStickies</tt>
    ///     buffer is at capacity, replace the oldest sticky atom.
    /// </summary>
    /// <param name="atom">
    ///     The atom to make sticky.
    /// </param>
    /// <param name="obj">
    ///     The game object to stick the <tt>atom</tt> to.
    /// </param>
    /// <returns>
    ///     <tt>true</tt> if successful, <tt>false</tt> otherwise.
    /// </returns>
    public bool TrySticking(AtomController atom, Rigidbody rigidbody)
    {
        if (Sticky && StickyIndex(atom) == -1
            && !rigidbody.gameObject.CompareTag("No Sticky"))
        {
            Unstick(stickyHead);
            atom.Highlight(true);

            atomStickies[stickyHead] = atom;
            stickyHead = (stickyHead + 1) % STICKY_COUNT;

            atom.StickyJoint = atom.gameObject.AddComponent<SpringJoint>();
            atom.StickyJoint.connectedBody = rigidbody;

            atom.StickyJoint.enableCollision = true;
            atom.StickyJoint.spring = STICKY_STRENGTH;
            atom.StickyJoint.breakForce = BREAK_FORCE;

            // manually set anchor positions
            atom.StickyJoint.autoConfigureConnectedAnchor = false;
            atom.StickyJoint.anchor = Vector3.zero;
            atom.StickyJoint.connectedAnchor = rigidbody.transform.InverseTransformPoint(
                atom.transform.position
            );

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Unstick the atom at index <tt>i</tt>, if present.
    /// </summary>
    /// <param name="i">
    ///     The index to remove.
    /// </param>
    private void Unstick(int i)
    {
        if (0 <= i && i < STICKY_COUNT)
        {
            if (atomStickies[i] != null) {
                atomStickies[i].Highlight(false);
                Destroy(atomStickies[i].StickyJoint);
            }
            atomStickies[i] = null;
        }
    }

    /// <summary>
    ///     Unstick the given <tt>atom</tt>, if it's sticky.
    /// </summary>
    /// <param name="atom">
    ///     The atom to remove.
    /// </param>
    public void Unstick(AtomController atom)
    {
        int index = StickyIndex(atom);
        if (index != -1) Unstick(index);
    }

    /// <summary>
    ///     Unstick all currently sticky <tt>atom</tt>s.
    /// </summary>
    public void UnstickAll() {
        for (int i = 0; i < STICKY_COUNT; i++)
        {
            Unstick(i);
        }
    }

    /// <summary>
    ///     Returns the index of the given <tt>atom</tt> in the <tt>atomStickies</tt> buffer.
    /// </summary>
    /// <param name="atom">
    ///     The atom to search for.
    /// </param>
    /// <returns>
    ///     The index of the <tt>atom</tt>, if present, -1 if not.
    /// </returns>
    private int StickyIndex(AtomController atom)
    {
        for (int i = 0; i < STICKY_COUNT; i++)
        {
            if (atom == atomStickies[i]) return i;
        }
        return -1;
    }
    
    /// <summary>
    ///     Change the sticky mode of the blob.
    /// </summary>
    /// <param name="enable">
    ///     Enables sicky mode iff this is <tt>True</tt>.
    /// </param>
    private void SetStickyMode(bool enable)
    {
        Sticky = enable;
        if (!Sticky) UnstickAll();
    }

    /// <returns>
    ///     <tt>True</tt> iff the blob is sticking to something.
    /// </returns>
    public bool IsSticking()
    {
        if (!Sticky) return false;
        
        for (int i = 0; i < STICKY_COUNT; i++)
        {
            if (atomStickies[i] != null) return true;
        }

        return false;
    }

    public void SetValue(bool newValue)
    {
        if (savedSticky == null)
        {
            SetStickyMode(newValue);
        }
        else
        {
            savedSticky = newValue;
        }
    }

    public void SetOverride(bool newOverride)
    {
        savedSticky = Sticky;
        Sticky = newOverride;
    }

    public void ClearOverride()
    {
        if (savedSticky == null) return;

        SetStickyMode(savedSticky.Value);
        savedSticky = null;
    }
}