using System;
using Unity.VisualScripting;
using UnityEngine;

public enum DisplayMode
{
    INVISIBLE, UI, VISIBLE
}

[RequireComponent(typeof(Rigidbody))]
public class Inventory : MonoBehaviour
{
    private GameObject[] contents;
    public int capacity { get; private set; } = 0;
    public int selection { get; private set; } = 0;
    public int burden { get; private set; } = 0;
    public int objectCount { get; private set; } = 0;
    public DisplayMode displayMode { get; private set;} = DisplayMode.VISIBLE;
    private Func<Vector3> CalculateDisplayPosition = null;

    public Rigidbody rigidBody;

    private AudioClip addAudioClip = null;
    private AudioClip removeAudioClip = null;
    private AudioSource audioSource;
    private Vector2? audioPitchBounds;

    public bool objectsCanLeave = true;
    public bool objectsCanJoin = true;

    public void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public void SetCapacity(int capacity)
    {
        if (this.capacity > 0 || capacity <= 0) return;

        this.capacity = capacity;
        contents = new GameObject[capacity];
    }

    public void SetDisplayPositionCallback(Func<Vector3> callback)
    {
        CalculateDisplayPosition = callback;
    }

    public Vector3 GetDisplayPosition()
    {
        if (CalculateDisplayPosition == null) return transform.position;

        return CalculateDisplayPosition();
    }

    public void SetDisplayMode(DisplayMode mode)
    {
        displayMode = mode;
    }

    public int GetDisplayLayer()
    {
        return displayMode switch
        {
            DisplayMode.INVISIBLE => Utilities.INVISIBLE_LAYER,
            DisplayMode.UI => Utilities.INVENTORY_UI_LAYER,
            _ => Utilities.DEFAULT_LAYER
        };
    }

    public void SetAudio(
        string addSoundResource = "", string removeSoundResource = "",
        Vector2? audioPitchBounds = null, float volume = 1f)
    {
        addAudioClip = Resources.Load<AudioClip>(Utilities.SOUNDS_PATH + addSoundResource);
        removeAudioClip = Resources.Load<AudioClip>(Utilities.SOUNDS_PATH + removeSoundResource);

        this.audioPitchBounds = audioPitchBounds;

        if (addSoundResource.Length + removeSoundResource.Length == 0) return;

        audioSource = gameObject.GetOrAddComponent<AudioSource>();
        audioSource.volume = volume;
    }
    
    /// <summary>
    ///     Select the subsequent inventory object based on what object was previously selected.
    /// </summary>
    /// <param name="forward">
    ///     If <tt>True</tt>, select the first index found by 
    /// </param>
    public void SelectNextNonEmptyObject(bool forward)
    {
        if (IsEmpty()) return;

        for (int i = 1; i < capacity; i++)
        {
            int index = (capacity + selection + (forward ? i : -i)) % capacity;
            if (TrySelectObject(index)) return;
        }
    }

    /// <summary>
    ///     Set the blob's selected item to be the one at the given index.
    /// </summary>
    /// <param name="i">
    ///     The index in the blob's inventory to select.
    /// </param>
    public bool TrySelectObject(int i)
    {
        if (contents[i] == null) return false;

        if (contents[selection] != null) {
            contents[selection].SetLayer(Utilities.INVISIBLE_LAYER);
        }
        
        contents[i].SetLayer(GetDisplayLayer());
        selection = i;

        return true;
    }

    /// <summary>
    ///     Determine if the blob has a high enough carrying capacity to add the given item to its
    ///     inventory.
    /// </summary>
    /// <param name="burden">
    ///     The burden of the item to add to the blob's inventory.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the blob can carry the additional burden.
    /// </returns>
    public bool CanFit(int burden)
    {
        return this.burden + burden <= capacity;
    }

    /// <summary>
    ///     Tests if the blob has the given object in its inventory.
    /// </summary>
    /// <param name="obj">
    ///     The object to test for.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the blob has the object.
    /// </returns>
    public int IndexOf(GameObject obj)
    {
        if (IsEmpty() && obj == null) return 0;

        for (int i = 0; i < capacity; i++) {
            if (contents[i] == obj) return i;
        }

        return -1;
    }

    public bool Contains(GameObject obj)
    {
        return IndexOf(obj) >= 0;
    }

    /// <summary>
    ///    Return a boolean indicating if the blob character is holding an object with the specified
    ///     tag.
    /// </summary>
    /// <param name="tag">
    ///     The tag to check for.
    /// </param>
    /// <returns>
    ///     <tt>true</tt> iff the held object has the tag.
    /// </returns>
    public int IndexOfObjectWithTag(string tag)
    {
        if (IsEmpty()) return -1;

        for (int i = 0; i < capacity; i++) {
            if (contents[i] != null && contents[i].CompareTag(tag)) return i;
        }
        
        return -1;
    }

    public bool ContainsObjectWithTag(string tag)
    {
        return IndexOfObjectWithTag(tag) >= 0;
    }

    /// <summary>
    ///     Attempt to grab the game object and keep it held by the blob character.
    ///     This can fail if another object is being held, if the object refuses to be grabbed,
    ///     or if the object does not have a Grip component.
    /// </summary>
    /// <param name="obj">
    ///     The GameObject that the blob character will try to grab.
    /// </param>
    /// <returns>
    ///     <tt>true</tt> iff the object was successfully grabbed.
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

        for (int i = 0; i < capacity; i++) {
            if (contents[i] == null)
            {
                contents[i] = obj;
                burden += objectGrip.burden;
                objectCount += 1;
                TrySelectObject(i);
                audioSource.PlayRandomPitchOneShot(addAudioClip, audioPitchBounds);
                return i;
            }
        }

        return -1;
    }

    public bool TryPickUp(GameObject obj)
    {
        if (obj != null && obj.TryGetComponent(out Grip objGrip)) return objGrip.TryJoin(this);

        return false;
    }

    public bool TryTakeFrom(Inventory provider, int index = -1)
    {
        return TryPickUp(provider.GetObject(index));
    }

    public bool TryGiveTo(Inventory recipient, int index = -1)
    {
        return recipient.TryPickUp(GetObject(index));
    }

    /// <summary>
    ///     Release and return the currently selected inventory object.
    /// </summary>
    public GameObject TryToRemove(int index = -1)
    {
        if (!objectsCanLeave || IsEmpty()) return null;

        if (index < 0) index = selection;

        if (contents[index] != null)
        {
            burden -= contents[index].GetComponent<Grip>().burden;
            objectCount -= 1;
            audioSource.PlayRandomPitchOneShot(removeAudioClip, audioPitchBounds);
        }
        GameObject releasedObject = contents[index];
        contents[index] = null;
        SelectNextNonEmptyObject(true);

        return releasedObject;
    }

    public GameObject TryToRemove(GameObject obj)
    {
        int i = IndexOf(obj);
    
        if (i >= 0) return TryToRemove(i);

        return null;
    }

    public bool TryDrop(int index = -1)
    {
        GameObject obj = GetObject(index);
        
        if (obj != null) return obj.GetComponent<Grip>().TryLeaveInventory();
        
        return false;
    }

    public void DropAll()
    {
        for (int i = 0; i < capacity; i++) TryDrop(i);
    }

    /// <summary>
    ///     Return the currently selected inventory object.
    /// </summary>
    public GameObject GetObject(int index = -1)
    {
        if (IsEmpty()) return null;

        if (index < 0) index = selection;

        return contents[index];
    }

    // public bool TryGiveObject(Inventory recipient, int index = -1)
    // {
    //     if (IsEmpty()) return false;

    //     Grip objGrip = GetObject(index).GetComponent<Grip>();

    //     if (recipient.CanFit(objGrip.burden)) objGrip.Transfer(recipient);

    //     return false;
    // }

    // public bool TryTakeObject(Inventory provider, int index = -1)
    // {
    //     return provider.TryGiveObject(this, index);
    // }

    public bool IsEmpty()
    {
        return burden == 0;
    }

    public bool IsFull()
    {
        return !CanFit(1);
    }

    public override string ToString()
    {
        string inventoryString = $"(burden: {burden})\n";
        for (int i = 0; i < capacity; i++)
        {
            inventoryString += $"  {(selection == i ? ">" : " ")}{i}: {(contents[i] == null ? "null" : contents[i].GetComponent<Grip>())}\n";
        }

        return inventoryString;
    }
}