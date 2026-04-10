/// <summary>
///     A layer between the input manager and the game object that allows control to be toggled.
/// </summary>
public interface Controllable
{
    /// <summary>
    ///     Is this object controlled?
    /// </summary>
    public bool controlled { get; set; }
}