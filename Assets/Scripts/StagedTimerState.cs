/// <summary>
///     A struct that describes the state of a StageTimer.
/// </summary>
public struct StagedTimerState
{
    /// <summary>
    ///     The stage the StagedTimer is currently in.
    /// </summary>
    public int stage;
    public string stageName;
    /// <summary>
    ///     The progression of the StagedTime through its current stage.
    /// </summary>
    public float progress;
    /// <summary>
    ///     <tt>True</tt> iff the previous update caused the StagedTimer to move to its next stage.
    /// </summary>
    public bool rolledOver;

    public override readonly string ToString()
    {
        return $"StageState(stage = {stage}, stageName = \"{stageName}\", progress = {progress:F3}, rolledOver = {rolledOver})";
    }
}