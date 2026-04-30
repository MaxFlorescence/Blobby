using Unity.Mathematics;

/// <summary>
///     Options for controlling a timer's behavior upon exceeding its interval.
/// </summary>
public enum TimerMode
{
    /// <summary>
    ///     On the update that the timer exceeded its interval, and only on that update, restart and
    ///     return true.
    /// </summary>
    Repeat,
    /// <summary>
    ///     On the update that the timer exceeded its interval, and only on that update, return
    ///     true.
    /// </summary>
    Pulse,
    /// <summary>
    ///     On the update that the timer exceeded its interval, and on all subsequent updates
    ///     (unless restarted), return true.
    /// </summary>
    Toggle
}

/// <summary>
///     A class for setting and querying a timer.
/// </summary>
public class Timer
{
    /// <summary>
    ///     The interval in seconds that the timer is set to run for.
    /// </summary>
    public float Interval { get; protected set; }
    /// <summary>
    ///     The current time on the timer.
    /// </summary>
    public float Time { get; protected set; } = 0;
    /// <summary>
    ///     <tt>True</tt> iff the timer's time has not exceeded its set interval.
    /// </summary>
    public bool Running { get; protected set; } = false;

    /// <param name="interval">
    ///     The interval in seconds that the timer is set to run for.
    /// </param>
    public Timer(float interval = 0)
    {
        GoSetInterval(interval);
    }

    /// <summary>
    ///     Increments the timer's time, possibly resetting it when it exceeds the timer's interval.
    /// </summary>
    /// <param name="increment">
    ///     The amount to increment the timer's time by. If less than or equal to 0, then the
    ///     timer's time is incremented by <tt>Time.deltaTime</tt>
    /// </param>
    /// <param name="mode">
    ///     Defines the timer's behavior when it exceeds its set interval.
    ///     <para/>
    ///     On the update that the timer exceeded its interval...
    ///     <br/>
    ///     <tt>TimerMode.Repeat</tt> ...and only on that update, restart and return true.
    ///     <br/>
    ///     <tt>TimerMode.Pulse</tt> ...and only on that update, return true.
    ///     <br/>
    ///     <tt>TimerMode.Toggle</tt> ...and on all subsequent updates (unless restarted), return true.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> when timer's time exceeds its interval. The <tt>mode</tt> argument
    ///     determines additional return behavior.
    ///     <para/>
    ///     On the update that the timer exceeded its interval...
    ///     <br/>
    ///     <tt>mode = TimerMode.Repeat</tt> ...and only on that update, restart and return true.
    ///     <br/>
    ///     <tt>mode = TimerMode.Pulse</tt> ...and only on that update, return true.
    ///     <br/>
    ///     <tt>mode = TimerMode.Toggle</tt> ...and on all subsequent updates (unless restarted), return true.
    /// </returns>
    public virtual bool Update(float increment = 0, TimerMode mode = TimerMode.Repeat)
    {
        bool wasRunning = Running;

        if (increment <= 0)
        {
            increment = UnityEngine.Time.deltaTime;
        }
        Time += increment;
        
        if (Time > Interval)
        {
            if (mode == TimerMode.Repeat) Reset();
            else Running = false;

            return mode != TimerMode.Pulse || wasRunning;
        }

        return false;
    }

    /// <summary>
    ///     Restart the timer. If the given interval is positive, set the timer's interval to it
    ///     before restarting.
    /// </summary>
    /// <param name="interval">
    ///     The time interval to use. If non-positive, then use the previous interval.
    /// </param>
    public void Reset(float interval = 0)
    {
        if (interval > 0) SetInterval(interval);

        Time = 0;
        Running = true;
    }

    public virtual void SetInterval(float interval)
    {
        GoSetInterval(interval);
    }
    
    protected void GoSetInterval(float interval)
    {
        Interval = math.max(0, interval);

        if (Interval == 0) Running = false;
    }

    /// <returns>
    ///     The amount of time remaining until the timer's time exceeds its interval.
    /// </returns>
    public float RemainingTime()
    {
        return math.max(0, Interval - Time);
    }

    /// <returns>
    ///     The proportion of the timer's time that has progressed, relative to its interval.
    /// </returns>
    public float Progress()
    {
        return math.min(1, Time / Interval);
    }

    /// <returns>
    ///     The proportion of the timer's time that remains until it exceeds the timer's interval.
    /// </returns>
    public float RemainingProgress()
    {
        return 1 - Progress();
    }

    /// <returns>
    ///     <tt>True</tt> iff the timer is complete.
    /// </returns>
    public bool Complete()
    {
        return Time >= Interval;
    }

    public override string ToString()
    {
        return $"Timer(Interval = {Interval:F3}, Time = {Time:F3})";
    }
}