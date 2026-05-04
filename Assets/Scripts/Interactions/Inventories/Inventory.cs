using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
///     Options for controlling how an inventory's currently selected object should be displayed.
/// </summary>
public enum DisplayMode
{
    /// <summary>
    ///     The object is always invisible.
    /// </summary>
    Invisible,
    /// <summary>
    ///     The object is visible only to UI cameras.
    /// </summary>
    UI_Only,
    /// <summary>
    ///     The object is always visible.
    /// </summary>
    Visible
}

/// <summary>
///     A class for managing the contents of an inventory.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Inventory : MonoBehaviour
{
    // ---------------------------------------------------------------------------------------------
    // PARAMETERS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The maximum number of objects / amount of burden this inventory can contain.
    /// </summary>
    public int Capacity { get; private set; } = 0;
    /// <summary>
    ///     The index of the currently selected object.
    /// </summary>
    public int Selection { get; private set; } = 0;
    /// <summary>
    ///     The amount of burden this inventory currently contains.
    /// </summary>
    public int Burden { get; private set; } = 0;
    /// <summary>
    ///     The number of objects this inventory currently contains.
    /// </summary>
    public int Count { get; private set; } = 0;
    /// <summary>
    ///     Iff <tt>false</tt>, objects cannot be removed from this inventory.
    /// </summary>
    public bool objectsCanLeave = true;
    /// <summary>
    ///     Iff <tt>false</tt>, objects cannot be added to this inventory.
    /// </summary>
    public bool objectsCanJoin = true;

    // ---------------------------------------------------------------------------------------------
    // COMPONENTS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     References to the objects contained in this inventory.
    /// </summary>
    private GameObject[] contents;
    public Rigidbody rigidBody;

    // ---------------------------------------------------------------------------------------------
    // DISPLAY
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     How the currently selected object should be displayed.
    ///     <para/>
    ///     One of <tt>DisplayMode.Invisible</tt>, <tt>DisplayMode.Visible</tt>, or
    ///     <tt>DisplayMode.UI_Only</tt> (only visible for UI cameras).
    /// </summary>
    private DisplayMode displayMode = DisplayMode.Visible;
    /// <summary>
    ///     A function that returns the position at which to display the selected object.
    /// </summary>
    private Func<Vector3> CalculateDisplayPosition = null;

    // ---------------------------------------------------------------------------------------------
    // AUDIO
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The audio clip that will play when an object is added to this inventory.
    /// </summary>
    private AudioClip addAudioClip = null;
    /// <summary>
    ///     The audio clip that will play when an object is removed from this inventory.
    /// </summary>
    private AudioClip removeAudioClip = null;
    private AudioSource audioSource;
    /// <summary>
    ///     The bounds for audio clip pitch randomization.
    /// </summary>
    private Vector2? audioPitchBounds;


    public void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    /// <summary>
    ///     Sets the inventory's capacity and initializes its contents array.
    /// </summary>
    /// <param name="capacity">
    ///     The maximum number of objects / amount of burden this inventory can contain.
    /// </param>
    public void SetCapacity(int capacity)
    {
        if (Capacity > 0 || capacity <= 0) return;

        Capacity = capacity;
        contents = new GameObject[capacity];
    }

    public void SetDisplayPositionCallback(Func<Vector3> callback)
    {
        CalculateDisplayPosition = callback;
    }

    /// <returns>
    ///     The result of calling <tt>CalculateDisplayPosition()</tt> if it is not <tt>null</tt>.
    ///     Otherwise, returns this inventory's position.
    /// </returns>
    public Vector3 GetDisplayPosition()
    {
        if (CalculateDisplayPosition == null) return transform.position;

        return CalculateDisplayPosition();
    }

    /// <summary>
    ///     Sets how the currently selected object should be displayed.
    ///     <para/>
    ///     One of <tt>DisplayMode.Invisible</tt>, <tt>DisplayMode.Visible</tt>, or
    ///     <tt>DisplayMode.UI_Only</tt> (only visible for UI cameras).
    /// </summary>
    /// <param name="mode">
    ///     One of <tt>DisplayMode.Invisible</tt>, <tt>DisplayMode.Visible</tt>, or
    ///     <tt>DisplayMode.UI_Only</tt> (only visible for UI cameras).
    /// </param>
    public void SetDisplayMode(DisplayMode mode)
    {
        displayMode = mode;
    }

    /// <returns>
    ///     The object layer corresponding to <tt>displayMode</tt>.
    /// </returns>
    public int GetDisplayLayer()
    {
        return displayMode switch
        {
            DisplayMode.Invisible => GameObjectExtensions.INVISIBLE_LAYER,
            DisplayMode.UI_Only => GameObjectExtensions.INVENTORY_UI_LAYER,
            _ => GameObjectExtensions.DEFAULT_LAYER
        };
    }

    /// <summary>
    ///     Loads and sets the audio clips that this inventory will play.
    /// </summary>
    /// <param name="addSoundResource">
    ///     The name of the audio clip that will play when an object is added to this inventory.
    /// </param>
    /// <param name="removeSoundResource">
    ///     The name of the audio clip that will play when an object is removed from this inventory.
    /// </param>
    /// <param name="audioPitchBounds">
    ///     The bounds for audio clip pitch randomization.
    /// </param>
    /// <param name="volume">
    ///     The volume to use for this inventory's audio source.
    /// </param>
    public void SetAudio(
        string addSoundResource = "", string removeSoundResource = "",
        Vector2? audioPitchBounds = null, float volume = 1f)
    {
        addAudioClip = Resources.Load<AudioClip>(FileUtilities.SOUNDS_PATH + addSoundResource);
        removeAudioClip = Resources.Load<AudioClip>(FileUtilities.SOUNDS_PATH + removeSoundResource);

        this.audioPitchBounds = audioPitchBounds;

        if (addSoundResource.Length + removeSoundResource.Length == 0) return;

        audioSource = gameObject.GetOrAddComponent<AudioSource>();
        audioSource.volume = volume;
    }
    
    /// <summary>
    ///     Select the subsequent inventory object based on what object was previously selected.
    /// </summary>
    /// <param name="forward">
    ///     Iff <tt>false</tt>, search through this inventory in reverse order.
    /// </param>
    public void SelectNextNonEmptyObject(bool forward)
    {
        if (IsEmpty()) return;

        for (int i = 1; i < Capacity; i++)
        {
            int index = (Capacity + Selection + (forward ? i : -i)) % Capacity;
            if (TrySelectObject(index)) return;
        }
    }

    /// <summary>
    ///     Sets the selected item to be the one at the given index if it is not <tt>null</tt>.
    /// </summary>
    /// <returns>
    ///     <tt>True</tt> iff the object that was selected is not <tt>null</tt>.
    /// </returns>
    public bool TrySelectObject(int i)
    {
        if (contents[i] == null) return false;

        if (contents[Selection] != null) {
            contents[Selection].SetLayer(GameObjectExtensions.INVISIBLE_LAYER);
        }
        
        contents[i].SetLayer(GetDisplayLayer());
        Selection = i;

        return true;
    }
    
    /// <param name="burden">
    ///     The burden value to check.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff this inventory can carry the additional burden.
    /// </returns>
    public bool CanFit(int burden)
    {
        return Burden + burden <= Capacity;
    }

    /// <returns>
    ///     The index of the given object in this inventory, or -1 if it is not present.
    /// </returns>
    public int IndexOf(GameObject obj)
    {
        if (IsEmpty() && obj == null) return 0;

        for (int i = 0; i < Capacity; i++) {
            if (contents[i] == obj) return i;
        }

        return -1;
    }

    /// <returns>
    ///     <tt>True</tt> iff this inventory contains the given object.
    /// </returns>
    public bool Contains(GameObject obj)
    {
        return IndexOf(obj) >= 0;
    }
    
    /// <returns>
    ///     The index of the first object in this inventory that has the given tag,
    ///     or -1 if no such object is found.
    /// </returns>
    public int IndexOfObjectWithTag(string tag)
    {
        if (IsEmpty()) return -1;

        for (int i = 0; i < Capacity; i++) {
            if (contents[i] != null && contents[i].CompareTag(tag)) return i;
        }
        
        return -1;
    }

    /// <returns>
    ///     <tt>True</tt> iff this inventory contains an object with the given tag.
    /// </returns>
    public bool ContainsObjectWithTag(string tag)
    {
        return IndexOfObjectWithTag(tag) >= 0;
    }

    /// <summary>
    ///     Attempt to add the given object to this inventory.
    ///     This can fail if objects are prevented from joining, if the object is <tt>null</tt>,
    ///     if the object does not have a <tt>Grip</tt> component, or if the object would not fit.
    /// </summary>
    /// <returns>
    ///     The index at which the given object was added, or -1 if it could not be added.
    /// </returns>
    public int TryToAdd(GameObject obj)
    {
        if (!objectsCanJoin ||
            obj == null ||
            !obj.TryGetComponent(out Grip objectGrip) ||
            !CanFit(objectGrip.burden))
        {
            return -1;
        }

        for (int i = 0; i < Capacity; i++) {
            if (contents[i] == null)
            {
                contents[i] = obj;
                Burden += objectGrip.burden;
                Count += 1;
                TrySelectObject(i);
                audioSource.PlayRandomPitchOneShot(addAudioClip, audioPitchBounds);
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    ///     Initiate the process of adding the given object to this inventory, from the object's
    ///     perspective. The object will go through the states:
    ///     <code>
    ///         [GripState.Held | GripState.Idle]
    ///         -> GripState.Grabbing
    ///         -> GripState.Held
    ///     </code>
    /// </summary>
    /// <returns>
    ///     <tt>True</tt> iff the object was successfully picked up.
    /// </returns>
    public bool TryPickUp(GameObject obj)
    {
        if (obj != null && obj.TryGetComponent(out Grip objGrip)) return objGrip.TryJoin(this);

        return false;
    }

    /// <summary>
    ///     Initiate the process of transferring the object at the given index <b>to this
    ///     inventory</b>, from the object's perspective. The object will go through the states:
    ///     <code>
    ///         GripState.Held
    ///         -> GripState.Grabbing
    ///         -> GripState.Held
    ///     </code>
    /// </summary>
    /// <param name="index">
    ///     The index of the object to take. If -1, takes the given inventory's currently selected
    ///     object.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the object was successfully transferred.
    /// </returns>
    public bool TryTakeFrom(Inventory provider, int index = -1)
    {
        return TryPickUp(provider.GetObject(index));
    }

    /// <summary>
    ///     Initiate the process of transferring the object at the given index <b>to the given
    ///     inventory</b>, from the object's perspective. The object will go through the states:
    ///     <code>
    ///         GripState.Held
    ///         -> GripState.Grabbing
    ///         -> GripState.Held
    ///     </code>
    /// </summary>
    /// <param name="index">
    ///     The index of the object to give. If -1, give this inventory's currently selected object.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the object was successfully transferred.
    /// </returns>
    public bool TryGiveTo(Inventory recipient, int index = -1)
    {
        return recipient.TryPickUp(GetObject(index));
    }

    /// <summary>
    ///     Attempt to remove the object at the given index from this inventory.
    ///     This can fail if objects are prevented from leaving, or if this inventory is empty.
    /// </summary>
    /// <param name="index">
    ///     The index of the object to remove. If -1, removes this inventory's currently selected
    ///     object.
    /// </param>
    /// <returns>
    ///     The given object that was removed, or <tt>null</tt> if nothing was removed.
    /// </returns>
    public GameObject TryToRemove(int index = -1)
    {
        if (!objectsCanLeave || IsEmpty()) return null;

        if (index < 0) index = Selection;

        if (contents[index] != null)
        {
            Burden -= contents[index].GetComponent<Grip>().burden;
            Count -= 1;
            audioSource.PlayRandomPitchOneShot(removeAudioClip, audioPitchBounds);
        }
        GameObject releasedObject = contents[index];
        contents[index] = null;
        SelectNextNonEmptyObject(true);

        return releasedObject;
    }

    /// <summary>
    ///     Attempt to remove the given object from this inventory.
    ///     This can fail if objects are prevented from leaving, of if the given object is not
    ///     present.
    /// </summary>
    /// <returns>
    ///     The given object that was removed, or <tt>null</tt> if nothing was removed.
    /// </returns>
    public GameObject TryToRemove(GameObject obj)
    {
        int i = IndexOf(obj);
    
        if (i >= 0) return TryToRemove(i);

        return null;
    }

    /// <summary>
    ///     Initiate the process of removing the object at the given index from this inventory, from
    ///     the object's perspective. The object will go through the states:
    ///     <code>
    ///         GripState.Held
    ///         -> GripState.Releasing
    ///         -> GripState.Idle
    ///     </code>
    /// </summary>
    /// <param name="index">
    ///     The index of the object to remove. If -1, removes this inventory's currently selected
    ///     object.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the object was successfully dropped.
    /// </returns>
    public bool TryDrop(int index = -1)
    {
        GameObject obj = GetObject(index);
        
        if (obj != null) return obj.GetComponent<Grip>().TryLeaveInventory();
        
        return false;
    }

    /// <summary>
    ///     Drops all objects currently in this inventory.
    /// </summary>
    public void DropAll()
    {
        for (int i = 0; i < Capacity; i++) TryDrop(i);
    }

    /// <param name="index">
    ///     The index of the object to get. If -1, gets this inventory's currently selected object.
    /// </param>
    /// <returns>
    ///     The inventory object at the given index.
    /// </returns>
    public GameObject GetObject(int index = -1)
    {
        if (IsEmpty()) return null;

        if (index < 0) index = Selection;

        return contents[index];
    }

    /// <returns>
    ///     <tt>True</tt> iff this inventory has a current burden of 0.
    /// </returns>
    public bool IsEmpty()
    {
        return Burden == 0;
    }

    /// <returns>
    ///     <tt>True</tt> iff this inventory cannot fit a burden of 1.
    /// </returns>
    public bool IsFull()
    {
        return !CanFit(1);
    }

    public override string ToString()
    {
        string inventoryString = $"(burden: {Burden})\n";
        for (int i = 0; i < Capacity; i++)
        {
            inventoryString += $"  {(Selection == i ? ">" : " ")}{i}: {(contents[i] == null ? "null" : contents[i].GetComponent<Grip>())}\n";
        }

        return inventoryString;
    }
}