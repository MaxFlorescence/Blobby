using System;

/// <summary>
///     An abstract class to apply changes to blob stats when in contact.
/// </summary>
public abstract class StatChanger<T> : Interactable where T : struct, IComparable
{
    public string statName = "";
    public T initialDelta = default;
    public T contactDelta = default;
    public float contactInterval = 0;
    public bool respectBounds = false;

    protected BlobController targetBlob;

    /// <summary>
    ///     Apply a stat delta to the given blob.
    /// </summary>
    protected abstract void ApplyDelta(T delta); 

    /// <summary>
    ///     Apply initial delta if it's non-zero and start the contact cooldown.
    /// </summary>
    protected override void OnInteract(BlobController blob)
    {
        targetBlob = blob;

        if (initialDelta.CompareTo(default) != 0) ApplyDelta(initialDelta);
        if (contactDelta.CompareTo(default) != 0) StartInteractionCooldown(contactInterval);
    }

    /// <summary>
    ///     If contact delta is non-zero, apply it and restart the contact cooldown.
    /// </summary>
    protected override void OnInteractionCooldownEnd()
    {
        if (contactDelta.CompareTo(default) == 0) return;
        
        ApplyDelta(contactDelta);
        StartInteractionCooldown(contactInterval);
    }

    /// <summary>
    ///     Interrupt the contact cooldown.
    /// </summary>
    protected override void OnInteractionEnd(BlobController blob)
    {
        InterruptInteractionCooldown();
        targetBlob = null;
    }
}