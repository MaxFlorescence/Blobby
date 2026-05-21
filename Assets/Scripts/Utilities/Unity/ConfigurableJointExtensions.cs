using UnityEngine;

public static class ConfigurableJointExtensions
{
    /// <summary>
    ///     Sets the constraints for the configurable joint's x, y, and z motion.
    /// </summary>
    public static void SetAllMotionConstraints(this ConfigurableJoint joint, ConfigurableJointMotion constraint)
    {
        joint.xMotion = constraint;
        joint.yMotion = constraint;
        joint.zMotion = constraint;
    }

    /// <summary>
    ///     Sets the constraints for the configurable joint's x, y, and z angular motion.
    /// </summary>
    public static void SetAllAngularMotionConstraints(this ConfigurableJoint joint, ConfigurableJointMotion constraint)
    {
        joint.angularXMotion = constraint;
        joint.angularYMotion = constraint;
        joint.angularZMotion = constraint;
    }

    /// <summary>
    ///     Sets the position spring force for the configurable joint's x, y, and z drives.
    /// </summary>
    public static void SetAllSpringForces(this ConfigurableJoint joint, float springForce)
    {
        var drive = joint.xDrive;
        drive.positionSpring = springForce;
        joint.xDrive = drive;

        drive = joint.yDrive;
        drive.positionSpring = springForce;
        joint.yDrive = drive;

        drive = joint.zDrive;
        drive.positionSpring = springForce;
        joint.zDrive = drive;
    }

    /// <summary>
    ///     Sets the position spring force for the configurable joint's x, y, and z drives.
    /// </summary>
    public static void SetAllDampers(this ConfigurableJoint joint, float damper)
    {
        var drive = joint.xDrive;
        drive.positionDamper = damper;
        joint.xDrive = drive;

        drive = joint.yDrive;
        drive.positionDamper = damper;
        joint.yDrive = drive;

        drive = joint.zDrive;
        drive.positionDamper = damper;
        joint.zDrive = drive;
    }

    /// <summary>
    ///     Sets the linear limit for the configurable joint.
    /// </summary>
    public static void SetLinearLimit(this ConfigurableJoint joint, float limit)
    {
        var linearLimit = joint.linearLimit;
        linearLimit.limit = limit;
        joint.linearLimit = linearLimit;
    }
}