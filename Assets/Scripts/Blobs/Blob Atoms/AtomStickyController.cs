using System;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
///     A class that controls stickiness for the atoms of a blob.
/// </summary>
[RequireComponent(typeof(AtomCollection))]
public class AtomStickyController : MonoBehaviour, IOverridable<bool>
{
    //----------------------------------------------------------------------------------------------
    // STICKY JOINTS
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     How many atoms can be sticky at once.
    /// </summary>
    private int stickyCount = 2;

    /// <summary>
    ///     Force needed to break the joint between a sticky atom and an object.
    /// </summary>
    private const float BREAK_FORCE = 1000;

    /// <summary>
    ///     Index of the last sticky atom, or null if fewer than <tt>STICKY_COUNT</tt> atoms are
    ///     currently sticky.
    /// </summary>
    private int stickyHead = 0;

    private AtomCollection atoms;

    /// <summary>
    ///     Circular buffer of capacity <tt>STICKY_COUNT</tt> for holding sticky atoms.
    /// </summary>
    private AtomController[] atomStickies;

    /// <summary>
    ///     The current motion type of the sticky joints. One of 
    ///     <tt>ConfigurableJointMotion.Locked</tt> or <tt>ConfigurableJointMotion.Limited</tt>.
    /// </summary>

    private ConfigurableJointMotion currentMotionType = ConfigurableJointMotion.Limited;

    //----------------------------------------------------------------------------------------------
    // ENABLING/DISABLING
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     Indicates if atoms can become sticky. If <tt>false</tt>, no atoms are sticky.
    /// </summary>
    public bool Sticky { get; private set; } = false;

    /// <summary>
    ///     Stores a saved sticky state during overrides.
    /// </summary>
    private bool? savedSticky = null;

    public bool IsOverridden { get => savedSticky != null; }

    void Awake()
    {
        atoms = GetComponent<AtomCollection>();
        atomStickies = new AtomController[atoms.Count - 1];
    }

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
        if (Sticky && StickyIndex(atom) == -1 && rigidbody != null
            && !rigidbody.gameObject.CompareTag("No Sticky"))
        {
            Unstick(stickyHead);
            atom.Highlight(true);

            atomStickies[stickyHead] = atom;
            stickyHead = (stickyHead + 1) % stickyCount;

            atom.StickyJoint = atom.gameObject.AddComponent<ConfigurableJoint>();
            atom.StickyJoint.connectedBody = rigidbody;

            atom.StickyJoint.enableCollision = true;
            atom.StickyJoint.SetLinearLimit(0.1f);
            atom.StickyJoint.SetAllMotionConstraints(currentMotionType);
            atom.StickyJoint.SetAllAngularMotionConstraints(ConfigurableJointMotion.Free);
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
    private void Unstick(int i, bool andCompact = true)
    {
        if (0 <= i && i < stickyCount)
        {
            if (atomStickies[i] != null) {
                atomStickies[i].Highlight(false);
                Destroy(atomStickies[i].StickyJoint);
            }
            atomStickies[i] = null;
        }

        if (i != stickyHead && andCompact) Compact();
    }

    private void Compact()
    {
        int? k = null;

        for (int i = 0; i < stickyCount; i++)
        {
            int ii = (stickyHead - 1 - i + stickyCount) % stickyCount;
            
            if (atomStickies[ii] == null)
            {
                k ??= ii;
            }
            else
            {
                if (k == null) continue;

                atomStickies[k.Value] = atomStickies[ii];
                atomStickies[ii] = null;
                k = (k - 1 + stickyCount) % stickyCount;
            }
        }
    }

    private void BulkUnstick(int count, bool andCompact = true)
    {
        count = count.Clamp(0, stickyCount);
        if (count == stickyCount) {
            UnstickAll();
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Unstick((stickyHead + i) % stickyCount, false);
        }

        if (andCompact) Compact();
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
        for (int i = 0; i < stickyCount; i++)
        {
            Unstick(i, false);
        }

        stickyHead = 0;
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
        for (int i = 0; i < stickyCount; i++)
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
        
        if (atomStickies[(stickyHead - 1 + stickyCount) % stickyCount] != null) return true;

        return false;
    }

    public void Resize(int newSize)
    {
        Assert.IsFalse(
            newSize.OutOfBounds(0, atomStickies.Length-1),
            $"New atom sticky size ({newSize}) out of bounds (0 - {atomStickies.Length})!"
        );

        int delta = stickyCount - newSize;

        if (delta > 0)
        {
            BulkUnstick(delta);
            int newStart = (stickyHead + delta) % stickyCount;
            int shift = (newStart < stickyHead) ? newStart : (delta % stickyCount);

            if (shift > 0) {
                for (int i = newStart; i < stickyCount; i++)
                {
                    atomStickies[i - shift] = atomStickies[i];
                    atomStickies[i] = null;
                }
            }

            if (newStart <= stickyHead) stickyHead = 0;
            stickyCount = newSize;
        }
        else if (delta < 0)
        {
            stickyCount = newSize;
            Compact();
        }
    }

    /// <summary>
    ///     Updates the motion type of the sticky joints.
    /// </summary>
    /// <param name="locked">
    ///     If <tt>true</tt>, lock the motion of sticky atoms. If <tt>false</tt>, set their motion
    ///     to be limited instead.
    /// </param>
    public void SetMotionLock(bool locked)
    {
        currentMotionType = locked ? ConfigurableJointMotion.Locked
                                   : ConfigurableJointMotion.Limited;
                                   
        ForEach(joint => joint.SetAllMotionConstraints(currentMotionType));
    }

    public void ForEach(Action<ConfigurableJoint> action)
    {
        for (int i = 1; i <= stickyCount; i++)
        {
            int ii = (stickyHead - i + stickyCount) % stickyCount;
            if (atomStickies[ii] == null) break;

            action.Invoke(atomStickies[ii].StickyJoint);
        }
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