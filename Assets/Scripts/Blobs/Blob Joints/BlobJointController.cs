using UnityEngine;

/// <summary>
///     A class defining several sizes of blobs.
/// </summary>
public static class BlobSize
{
    public static readonly float Tiny = 0.25f;
    public static readonly float Small = 0.5f;
    public static readonly float Medium = 1f;
    public static readonly float Large = 1.5f;
    public static readonly float Huge = 2f;
    public static readonly float Giant = 2.5f;
}

/// <summary>
///     A class that controls all the configurable joints for a blob.
/// </summary>
[RequireComponent(typeof(AtomCollection))]
public class BlobJointController : MonoBehaviour, IOverridable<BlobJointData>
{
    //----------------------------------------------------------------------------------------------
    // ATOMS
    //----------------------------------------------------------------------------------------------
    /// <summary>
    ///     The blob atoms that the controlled joints interconnect.
    /// </summary>
    private AtomCollection atoms;

    public BlobMeshController meshController;

    /// <summary>
    ///     How big each atom of the blob is.
    /// </summary>
    private const float ATOM_SCALE = 0.5f;

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
    ///         MotionLimit = 2f
    ///     </code>
    /// </summary>
    public readonly BlobJointData DEFAULT_JOINT_DATA = new(
        lengthFactor: 1,
        isFixedJoint: false,
        springForce: 50,
        damping: 0.5f
    );

    /// <summary>
    ///     The last motion limit
    /// </summary>
    private float previousMotionLimit;
    private readonly Timer lerpTimer = new(1);

    /// <summary>
    ///     The original anchor positions for each controlled joint.
    /// </summary>
    private Vector3[] connectedAnchors;
    private Vector3[] vertexPositions;

    void Awake()
    {
        DEFAULT_JOINT_DATA.CalculateMotionLimit();
        previousMotionLimit = DEFAULT_JOINT_DATA.MotionLimit.Value;
        Data = DEFAULT_JOINT_DATA;

        atoms = GetComponent<AtomCollection>();
        joints = GetComponentsInChildren<ConfigurableJoint>();
        jointCount = joints.Length;
        
        InitializeJoints();
    }

    void Start()
    {
        vertexPositions = new Vector3[atoms.Count];

        for (int i = 0; i < jointCount; i++)
        {
            Transform from = joints[i].transform;
            vertexPositions[atoms.IndexOf(from)] = atoms.CenterTransform.InverseTransformPoint(
                from.position
            );
        }
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
        float updateLinearLimit = -1;

        if (lerpTimer.Update(0, TimerMode.Pulse))
        {
            previousMotionLimit = Data.MotionLimit.Value;

            if (previousMotionLimit == 0) {
                for (int i = 0; i < atoms.Count; i++)
                {
                    atoms.Controllers[i].SetOverride(vertexPositions[i] * Data.LengthFactor.Value);
                }
            }
        }
        else if (lerpTimer.Running)
        {
            updateLinearLimit = lerpTimer.RemainingProgress() * previousMotionLimit
                                + lerpTimer.Progress() * Data.MotionLimit.Value;
        }
        else if (previousMotionLimit != Data.MotionLimit.Value)
        {
            lerpTimer.Reset();
        }
        else if (Data.MotionLimit.Value > 0)
        {
            atoms.ForEach(atom => atom.ClearOverride());
        }

        if (updateLinearLimit >= 0)
        {
            foreach (ConfigurableJoint joint in joints)
            {
                joint.SetLinearLimit(updateLinearLimit);
            }
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

        if (!Data.IsFixedJoint.Value || Data.Snap) previousMotionLimit = Data.Snap ? 0
            : DEFAULT_JOINT_DATA.MotionLimit.Value;
        
        for (int i = 0; i < jointCount; i++)
        {
            joints[i].connectedAnchor = Data.LengthFactor.Value * connectedAnchors[i];
            joints[i].SetAllSpringForces(Data.SpringForce.Value);
            joints[i].SetLinearLimit(previousMotionLimit);
        }

        meshController.ScaleFactor = 1f + ATOM_SCALE/Data.LengthFactor.Value;
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