using System;
using UnityEngine.Assertions;

public struct StageState
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

/// <summary>
///     A class for setting and querying a timer with stages.
/// </summary>
public class StagedTimer : Timer
{
    /// <summary>
    ///     A cumulative span of <tt>subintervals</tt>.
    /// </summary>
    private float[] subintervalsCumulative;
    /// <summary>
    ///     The sub-intervals that the timer's main interval is split into.
    /// </summary>
    public float[] Subintervals { get; private set; }

    private readonly int subintervalCount;

    /// <summary>
    ///     Names to use to refer to each of the timer's subintervals.
    /// </summary>
    private string[] stageNames;
    
    /// <summary>
    ///     The current state of the StagedTimer.
    /// </summary>
    public StageState State;

    /// <param name="subintervals">
    ///     The sub-intervals that the timer's main interval is split into. For example:
    ///     <code>
    ///         Sub-intervals: {1, 3, 2, 3}
    ///         Main interval: 1+3+2+3 = 9
    ///         Stages:   0111223334
    ///                   ++--+-+--+->
    ///         Timestep: 0123456789
    ///     </code>
    /// </param>
    /// <param name="stageNames">
    ///     Optional names by which to refer to each stage.
    /// </param>
    public StagedTimer(float[] subintervals, string[] stageNames = null)
    {
        subintervalCount = subintervals.Length;
        SetIntervals(subintervals, stageNames);
    }

    public override void SetInterval(float interval)
    {
        throw new InvalidOperationException("Cannot manually set the main interval of a StagedTimer!");   
    }

    public void SetIntervals(float[] subintervals, string[] stageNames = null)
    {
        Assert.AreEqual(subintervalCount, subintervals.Length);
        if (stageNames != null)
            Assert.AreEqual(subintervalCount, stageNames.Length);

        foreach (float subinterval in subintervals)
            Assert.IsTrue(subinterval > 0);
        
        Subintervals = subintervals;
        subintervalsCumulative ??= new float[subintervalCount + 1];
        this.stageNames ??= new string[subintervalCount + 1];

        subintervalsCumulative[0] = 0;
        this.stageNames[subintervalCount] = "Completed";

        for (int i = 1; i < subintervalCount + 1; i++)
        {
            subintervalsCumulative[i] = subintervalsCumulative[i-1] + subintervals[i-1];
            this.stageNames[i-1] = stageNames?[i-1] ?? $"Stage {i-1}";
        }

        State = new() {
            stage = 0,
            stageName = stageNames[0],
            progress = 0,
            rolledOver = false
        };

        GoSetInterval(subintervalsCumulative[subintervalCount]);
    }

    public override bool Update(float increment = 0, TimerMode mode = TimerMode.Repeat)
    {
        bool baseResult = base.Update(increment, mode);

        UpdateState(baseResult, mode);

        return baseResult;
    }

    /// <summary>
    ///     Updates the timer's <tt>StageState</tt> incrementally. This assumes each update's time
    ///     increment is short enough to not roll over through more than one state at a time.
    /// </summary>
    private void UpdateState(bool baseResult, TimerMode mode)
    {
        if (baseResult && mode == TimerMode.Repeat)
        {
            State.stage = 0;
            State.rolledOver = true;
        }
        else if (State.stage < subintervalCount-1 && Time >= subintervalsCumulative[State.stage + 1]) {
            State.stage++;
            State.rolledOver = true;
        }

        State.progress = (Time - subintervalsCumulative[State.stage]) / Subintervals[State.stage];
        State.stageName = stageNames[State.stage];
    }

    /// <returns>
    ///     The sub-intervals of the timer's main interval that corresponds to the given stage.
    ///     <br/>    
    ///     If no stage matches the given stage name, returns 0.
    /// </returns>
    public float GetSubinterval(string stageName)
    {
        int index = Array.IndexOf(stageNames, stageName);
        return index < 0 ? 0 : Subintervals[index];
    }

    /// <summary>
    ///     Moves the timer's time to the start of the stage with the given stage name. If no such
    ///     stage exists, do nothing.
    /// </summary>
    /// <param name="stageName">
    ///     The name of the stage to move the timer to.
    /// </param>
    public void GoToStage(string stageName)
    {
        int stage = Array.IndexOf(stageNames, stageName);
        if (stage >= 0) GoToStage(stage);
    }

    /// <summary>
    ///     Moves the timer's time to the start of the given stage. If the stage index is out of
    ///     bounds, do nothing.
    /// </summary>
    public void GoToStage(int stage)
    {
        if (stage >= 0 && stage < subintervalCount)
            Time = subintervalsCumulative[stage];
    }

    public override string ToString()
    {
        return $"StagedTimer(Interval = {Interval:F3}, Time = {Time:F3}, subintervalCount = {subintervalCount}, state = {State})";
    }
}