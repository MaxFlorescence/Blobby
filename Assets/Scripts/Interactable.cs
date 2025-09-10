/// <summary>
///     An interface for objects that can be interacted with by the blob character.
/// </summary>
public interface Interactable
{
    /// <summary>
    ///    Called by a blob character when it interacts with this object.
    /// </summary>
    /// <param name="blob">
    ///     The blob character interacting with this object.
    /// </param>
    public abstract void OnInteract(BlobController blob);
}