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
    private float[] subintervals;
    private readonly int subintervalCount;

    /// <summary>
    ///     Names to use to refer to each of the timer's subintervals.
    /// </summary>
    private string[] stageNames;
    
    private int lastStage;

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
        
        this.subintervals = subintervals;
        subintervalsCumulative ??= new float[subintervalCount + 1];
        this.stageNames ??= new string[subintervalCount + 1];

        subintervalsCumulative[0] = 0;
        this.stageNames[subintervalCount] = "Completed";

        for (int i = 1; i < subintervalCount + 1; i++)
        {
            subintervalsCumulative[i] = subintervalsCumulative[i-1] + subintervals[i-1];
            this.stageNames[i-1] = stageNames?[i-1] ?? $"Stage {i-1}";
        }

        lastStage = subintervalCount;

        GoSetInterval(subintervalsCumulative[subintervalCount]);
    }

    public override bool Update(float increment = 0, TimerMode mode = TimerMode.Repeat)
    {
        bool baseResult = base.Update(increment, mode);

        StageState state = GetState();
        lastStage = state.stage;

        return baseResult;
    }

    /// <returns>
    ///     Returns the progress through stages that the timer currently has as a
    ///     <tt>StageState</tt> object.
    /// </returns>
    public StageState GetState()
    {
        StageState state = new() {
            stage = subintervalCount,
            progress = 0
        };

        for (int i = 0; i < subintervalCount; i++)
        {
            if (Time < subintervalsCumulative[i+1])
            {
                state.stage = i;
                state.progress = (Time - subintervalsCumulative[i]) / subintervals[i];
                break;
            }
        }

        state.rolledOver = state.stage != lastStage;
        state.stageName = stageNames == null ? state.stage.ToString() : stageNames[state.stage];

        return state;
    }

    public override string ToString()
    {
        return $"StagedTimer(Interval = {Interval:F3}, Time = {Time:F3}, subintervalCount = {subintervalCount}, state = {GetState()})";
    }
}