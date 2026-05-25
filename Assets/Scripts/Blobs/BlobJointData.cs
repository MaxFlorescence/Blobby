public class BlobJointData
{
    public float? lengthFactor { get; private set; }
    public float LengthFactor => (float)lengthFactor;

    public bool? fixedJoint { get; private set; }
    public bool FixedJoint => (bool)fixedJoint;

    public float? springForce { get; private set; }
    public float SpringForce => (float)springForce;

    public float? damping { get; private set; }
    public float Damping => (float)damping;

    public float? motionLimit { get; set; }
    public float MotionLimit => (float)motionLimit;

    public BlobJointData(float? lengthFactor = null, bool? fixedJoint = null,
                         float? springForce = null, float? damping = null)
    {
        this.lengthFactor = lengthFactor;
        this.fixedJoint = fixedJoint;
        this.springForce = springForce;
        this.damping = damping;

        if (fixedJoint == null)
        {
            motionLimit = null;
        }
        else if (FixedJoint)
        {
            motionLimit = 0;
        }
        else
        {
            CalculateMotionLimit();
        }
        
    }

    public void CalculateMotionLimit()
    {
        motionLimit = (lengthFactor + 1) ?? null;
    }

    public void UpdateWith(BlobJointData other)
    {
        if (other.lengthFactor != null) lengthFactor = other.lengthFactor;
        if (other.fixedJoint != null) fixedJoint = other.fixedJoint;
        if (other.springForce != null) springForce = other.springForce;
        if (other.damping != null) damping = other.damping;
        if (other.motionLimit != null) motionLimit = other.motionLimit;
    }

    public bool Approx(BlobJointData other)
    {
        return other != null
            && (other.fixedJoint == null || fixedJoint == other.fixedJoint)
            && (other.lengthFactor == null || (lengthFactor?.Approx(other.lengthFactor) ?? false))
            && (other.springForce == null || (springForce?.Approx(other.springForce) ?? false))
            && (other.damping == null || (damping?.Approx(other.damping) ?? false))
            && (other.motionLimit == null || (motionLimit?.Approx(other.motionLimit) ?? false));
    }

    public override string ToString()
    {
        return $"BlobJointDataStruct(lengthFactor={lengthFactor}, fixedJoint={fixedJoint}, springForce={springForce}, damping={damping}, motionLimit={motionLimit})";
    }
}