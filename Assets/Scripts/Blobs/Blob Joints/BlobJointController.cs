using UnityEngine;

/// <summary>
///     A class defining several sizes of blobs.
/// </summary>
public static class BlobSize
{
    public static readonly float Tiny = 0.3f;
    public static readonly float Small = 0.6f;
    public static readonly float Medium = 1f;
    public static readonly float Large = 1.5f;
    public static readonly float Huge = 2f;
    public static readonly float Giant = 2.5f;
}

/// <summary>
///     A class that controls all the configurable joints for a blob.
/// </summary>
[RequireComponent(typeof(AtomCollection))]
public class BlobJointController : MonoBehaviour, IOverridable<BlobJointData>, IBusyable
{
    //----------------------------------------------------------------------------------------------
    // ATOMS
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The blob atoms that the controlled joints interconnect.
    /// </summary>
    private AtomCollection atoms;

    public BlobMeshController meshController;

    //----------------------------------------------------------------------------------------------
    // JOINTS
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The joins that are being controlled.
    /// </summary>
    private ConfigurableJoint[] joints;

    /// <summary>
    ///     The number of joints that are being controlled.
    /// </summary>
    private int jointCount;
    
    /// <summary>
    ///     The current data applied to each of the controlled joints.
    /// </summary>
    private BlobJointData Data { get; set; }
    private BlobJointData SavedData { get; set; } = null;

    /// <summary>
    ///     The default settings for controlled joints.
    ///     <code>
    ///         LengthFactor = 1,
    ///         FixedJoint = false,
    ///         SpringForce = 50,
    ///         Damping = 0.5f,
    ///         MotionLimit = 1f
    ///     </code>
    /// </summary>
    public readonly BlobJointData DEFAULT_JOINT_DATA = new(
        lengthFactor: 1,
        isFixedJoint: false,
        springForce: 50,
        damping: 0.5f
    );

    /// <summary>
    ///     The original anchor positions for each controlled joint.
    /// </summary>
    private Vector3[] connectedAnchors;
    
    public bool IsOverridden { get => SavedData != null; }
    
    public bool Busy { get => lerpTimer.Running; }

    //----------------------------------------------------------------------------------------------
    // MOTION LIMITS
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The last motion limit value that was applied to the joints.
    /// </summary>
    private float lastMotionLimit;
    /// <summary>
    ///     The motion limit value from which to start lerping.
    /// </summary>
    private float lerpMotionLimitStart;
    /// <summary>
    ///     The timer for lerping between the previous motion limit and the current motion limit.
    /// </summary>
    private readonly Timer lerpTimer = new(0.5f);

    void Awake()
    {
        DEFAULT_JOINT_DATA.CalculateMotionLimit();
        lastMotionLimit = DEFAULT_JOINT_DATA.MotionLimit.Value;
        Data = DEFAULT_JOINT_DATA;

        atoms = GetComponent<AtomCollection>();
        joints = GetComponentsInChildren<ConfigurableJoint>();
        jointCount = joints.Length;
        
        InitializeJoints();
    }

    void Start()
    {
        meshController.Rescale(Data.LengthFactor.Value);
    }

    /// <summary>
    ///     Manually sets the anchor positions of all controlled joints to be the positions of each
    ///     respective connected body. Also sets the damping factors, spring forces, and linear
    ///     motion limits of the joints.
    /// </summary>
    private void InitializeJoints()
    {
        connectedAnchors = new Vector3[jointCount];

        for (int i = 0; i < jointCount; i++)
        {
            Rigidbody from = joints[i].GetComponent<Rigidbody>();
            Rigidbody to = joints[i].connectedBody;
            
            connectedAnchors[i] = to.transform.InverseTransformPoint(from.position);
            joints[i].connectedAnchor = connectedAnchors[i];

            joints[i].SetAllDampers(Data.Damping.Value);
            joints[i].SetAllSpringForces(Data.SpringForce.Value);
            joints[i].SetLinearLimit(Data.MotionLimit.Value);
        }
    }

    void Update()
    {
        float nextMotionLimit = lastMotionLimit;

        if (lerpTimer.Update(0, TimerMode.Pulse))
        {
            lerpMotionLimitStart = Data.MotionLimit.Value;
            nextMotionLimit = lerpMotionLimitStart;
        }
        else if (lerpTimer.Running)
        {
            nextMotionLimit = lerpTimer.RemainingProgress() * lerpMotionLimitStart
                              + lerpTimer.Progress() * Data.MotionLimit.Value;
        }
        else if (lastMotionLimit != Data.MotionLimit.Value && (
                    lastMotionLimit != BlobJointData.MINIMUM_MOTION_LIMIT
                    || Data.MotionLimit.Value != BlobJointData.MINIMUM_MOTION_LIMIT
                ))
        {
            lerpTimer.Reset();
        }

        if (nextMotionLimit != lastMotionLimit) {
            foreach (ConfigurableJoint joint in joints)
            {
                joint.SetLinearLimit(nextMotionLimit);
            }
            lastMotionLimit = nextMotionLimit;
        }

        if (Data.MotionLimit.Value == BlobJointData.MINIMUM_MOTION_LIMIT
            && !lerpTimer.Running)
        {
            atoms.SetOverrides(Data.LengthFactor.Value);
        }
        else if (Data.MotionLimit.Value != BlobJointData.MINIMUM_MOTION_LIMIT
                 && atoms.AreOverridden())
        {
            atoms.ClearOverrides();
        }
    }

    /// <summary>
    ///     Modifies all controlled joints.
    /// </summary>
    /// <param name="jointData">
    ///     The joint data to apply to all joints. If <tt>null</tt>, then
    ///     <tt>BlobJointController.DEFAULT_JOINT_DATA</tt> is used.
    /// </param>
    /// <param name="snap">
    ///     Iff <tt>true</tt>, force the joint to immediately change length.
    /// </param>
    private void SetJointData(BlobJointData jointData)
    {
        if (jointData == null || Data.Approx(jointData) || lerpTimer.Running) return;

        if (jointData.LengthFactor != null && Data.LengthFactor.Value > jointData.LengthFactor)
        {
            ClipSeparatedAtoms();
        }
        
        Data.UpdateWith(jointData);

        if (Data.Snap)
        {
            lastMotionLimit = BlobJointData.MINIMUM_MOTION_LIMIT;
            lerpMotionLimitStart = BlobJointData.MINIMUM_MOTION_LIMIT;
        }
        
        for (int i = 0; i < jointCount; i++)
        {
            joints[i].connectedAnchor = Data.LengthFactor.Value * connectedAnchors[i];
            joints[i].SetAllSpringForces(Data.SpringForce.Value);
            joints[i].SetLinearLimit(lastMotionLimit);
        }

        meshController.Rescale(Data.LengthFactor.Value);
    }

    /// <summary>
    ///     Temporarily allow atoms that are separated from the center atom by an object them to
    ///     phase through objects. This gives the player a way to get unstuck in some situations.
    /// </summary>
    private void ClipSeparatedAtoms()
    {
        LayerMask layerMask = ~LayerMask.GetMask("Inventory UI", "Ignore Camera");

        for (int i = 1; i < atoms.Count; i++)
        {
            Vector3 atomPosition = atoms[i].position;
            Vector3 differenceVector = atoms.CenterTransform.position - atomPosition;
            
            bool hitSomething = Physics.Raycast(
                atomPosition,
                differenceVector.normalized,
                out RaycastHit hitInfo,
                differenceVector.magnitude,
                layerMask.value,
                QueryTriggerInteraction.Ignore
            );

            if (hitSomething)
            {
                AtomController atomController = atoms.Controllers[i];
                atomController.SetCollider(false);
                this.DelayedExecute(0.5f, () => atomController.SetCollider(true));
            }
        }
    }

    public void SetValue(BlobJointData newData)
    {
        if (SavedData == null)
        {
            SetJointData(newData);
        }
        else
        {
            SavedData.UpdateWith(newData);
        }
    }

    public void SetOverride(BlobJointData newData)
    {
        SavedData = Data.Copy();
        SetJointData(newData);
    }

    public void ClearOverride()
    {
        if (SavedData == null) return;

        SetJointData(SavedData);
        SavedData = null;
    }

    public override string ToString()
    {
        return Data.ToString();
    }
}