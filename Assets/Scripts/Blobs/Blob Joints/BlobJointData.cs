/// <summary>
///     A class defining the data for a blob's joints.
/// </summary>
public class BlobJointData
{
    /// <summary>
    ///     The length multiplier for each of the blob's joints.
    /// </summary>
    public float? LengthFactor { get; private set; }

    /// <summary>
    ///     <tt>True</tt> if the blob's joints are fixed, <tt>false</tt> if they're springs.
    /// </summary>
    public bool? IsFixedJoint { get; private set; }

    /// <summary>
    ///     The force that the blob's joints exert to maintain the connected atom's position, if
    ///     they're springs.
    /// </summary>
    public float? SpringForce { get; private set; }

    /// <summary>
    ///     The loss of energy in the blob's joints' oscillation, if they're springs.
    /// </summary>
    public float? Damping { get; private set; }

    /// <summary>
    ///     The maximum distance that each atom of the blob can be from its connected anchor.
    /// </summary>
    public float? MotionLimit { get; set; }

    public bool Snap { get; private set; }

    public BlobJointData(float? lengthFactor = null, bool? isFixedJoint = null,
                         float? springForce = null, float? damping = null, bool snap = false)
    {
        LengthFactor = lengthFactor;
        IsFixedJoint = isFixedJoint;
        SpringForce = springForce;
        Damping = damping;
        Snap = snap;

        if (isFixedJoint == null)
        {
            MotionLimit = null;
        }
        else if (IsFixedJoint.Value)
        {
            MotionLimit = 0;
        }
        else
        {
            CalculateMotionLimit();
        }
    }

    public void CalculateMotionLimit()
    {
        MotionLimit = (LengthFactor + 1) ?? -1;
    }

    /// <summary>
    ///     Overwrite this <tt>BlobJointData</tt>'s values using the non-null values of the given
    ///     <tt>BlobJointData</tt>.
    /// </summary>
    public void UpdateWith(BlobJointData other)
    {
        if (other.LengthFactor != null) LengthFactor = other.LengthFactor;
        if (other.IsFixedJoint != null) IsFixedJoint = other.IsFixedJoint;
        if (other.SpringForce != null) SpringForce = other.SpringForce;
        if (other.Damping != null) Damping = other.Damping;
        Snap = other.Snap;
        
        if (other.MotionLimit == null) return;
        if (other.MotionLimit < 0)
        {
            CalculateMotionLimit();
        }
        else
        {
            MotionLimit = other.MotionLimit;
        }
    }

    /// <returns>
    ///     <tt>True</tt> iff all of this <tt>BlobJointData</tt>'s values are approximately equal to
    ///     the other <tt>BlobJointData</tt>'s values. <tt>Null</tt> values are ignored.
    /// </returns>
    public bool Approx(BlobJointData other)
    {
        return other != null
           && (other.IsFixedJoint == null || IsFixedJoint == other.IsFixedJoint)
           && (other.LengthFactor == null || LengthFactor.Value.Approx(other.LengthFactor))
           && (other.SpringForce == null  || SpringForce.Value.Approx(other.SpringForce))
           && (other.Damping == null      || Damping.Value.Approx(other.Damping))
           && (other.MotionLimit == null  || MotionLimit.Value.Approx(other.MotionLimit));
    }

    public BlobJointData Copy()
    {
        return new(LengthFactor, IsFixedJoint, SpringForce, Damping, Snap);
    }

    public override string ToString()
    {
        string result = $"BlobJointData(";
        if (LengthFactor.HasValue) result += $"LengthFactor={LengthFactor}, ";
        if (IsFixedJoint.HasValue) result += $"IsFixedJoint={IsFixedJoint}, ";
        if (SpringForce.HasValue) result += $"SpringForce={SpringForce}, ";
        if (Damping.HasValue) result += $"Damping={Damping}, ";
        if (MotionLimit.HasValue) result += $"MotionLimit={MotionLimit}, ";
        return result + $"snap={Snap})";
    }
}