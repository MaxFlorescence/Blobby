/// <summary>
///     An interface for classes that can be busy with a task.
/// </summary>
public interface IBusyable
{
    /// <summary>
    ///     <tt>True</tt> iff this <tt>IBusyable</tt> is currently busy.
    /// </summary>
    public abstract bool Busy { get; }
}