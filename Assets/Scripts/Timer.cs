using Unity.Mathematics;

/// <summary>
///     A class for setting and querying a timer.
/// </summary>
class Timer
{
    /// <summary>
    ///     The interval in seconds that the timer is set to run for.
    /// </summary>
    public float Interval { get; private set; }
    /// <summary>
    ///     The current time on the timer.
    /// </summary>
    public float Time { get; private set; }

    /// <param name="interval">
    ///     The interval in seconds that the timer is set to run for.
    /// </param>
    public Timer(float interval = 0)
    {
        SetInterval(interval);
    }

    /// <summary>
    ///     Increments the timer's time, possibly resetting it when it exceeds the timer's interval.
    /// </summary>
    /// <param name="increment">
    ///     The amount to increment the timer's time by. If less than or equal to 0, then the
    ///     timer's time is incremented by <tt>Time.deltaTime</tt>
    /// </param>
    /// <param name="reset">
    ///     Resets the timer when its interval is exceeded iff set to <tt>true</tt>.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the timer's time exceeded its interval.
    /// </returns>
    public bool Update(float increment = 0, bool reset = true)
    {
        if (increment <= 0)
        {
            increment = UnityEngine.Time.deltaTime;
        }
        Time += increment;
        
        if (Time > Interval)
        {
            if (reset) Time = 0;
            return true;
        }

        return false;
    }

    public void Reset()
    {
        Time = 0;
    }

    public void SetInterval(float interval)
    {
        Interval = math.max(0, interval);
    }

    /// <returns>
    ///     The amount of time remaining until the timer's time exceeds its interval.
    /// </returns>
    public float RemainingTime()
    {
        return Interval - Time;
    }

    /// <returns>
    ///     The proportion of the timer's time that has progressed, relative to its interval.
    /// </returns>
    public float Progress()
    {
        return Time / Interval;
    }

    /// <returns>
    ///     The proportion of the timer's time that remains until it exceeds the timer's interval.
    /// </returns>
    public float RemainingProgress()
    {
        return 1 - Progress();
    }
}