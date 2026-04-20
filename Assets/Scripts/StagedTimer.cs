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
}

/// <summary>
///     A class for setting and querying a timer with stages.
/// </summary>
public class StagedTimer : Timer
{
    /// <summary>
    ///     A cumulative span of <tt>subintervals</tt>.
    /// </summary>
    private readonly float[] subintervalsCumulative;
    /// <summary>
    ///     The sub-intervals that the timer's main interval is split into.
    /// </summary>
    private readonly float[] subintervals;
    private readonly int subintervalCount;
    
    /// <summary>
    ///     Names to use to refer to each of the timer's subintervals.
    /// </summary>
    private readonly string[] stageNames;
    
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
        this.subintervals = subintervals;
        subintervalCount = subintervals.Length;

        if (stageNames != null)
            Assert.AreEqual(subintervalCount, stageNames.Length);
        this.stageNames = stageNames;

        subintervalsCumulative = new float[subintervalCount + 1];
        lastStage = subintervalCount;

        subintervalsCumulative[0] = 0;
        for (int i = 1; i < subintervalCount + 1; i++)
        {
            subintervalsCumulative[i] = subintervalsCumulative[i-1] + subintervals[i-1];
        }

        SetInterval(subintervalsCumulative[subintervalCount]);
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
}