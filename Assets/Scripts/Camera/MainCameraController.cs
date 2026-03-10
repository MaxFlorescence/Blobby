using UnityEngine;

/// <summary>
///     A class for controlling the movement of the main camera.
/// </summary>
public class MainCameraController : PriorityCamera
{
    /// <summary>
    ///     The ideal position of the camera.
    /// </summary>
    private Vector3 targetOffset;
    /// <summary>
    ///     The length of <tt>offset</tt>.
    /// </summary>
    public float trackingDistance = 10f;
    /// <summary>
    ///     The object that the camera is tracking.
    /// </summary>
    private Transform trackedTransform = null;
    /// <summary>
    ///     The position that the camera was at in the last frame.
    /// </summary>
    private Vector3 lastPosition;
    /// <summary>
    ///     Custom epsilon to determine if the last position is close to the current position.
    /// </summary>
    private const float EPSILON = 1E-2f;
    /// <summary>
    ///     At what distance to begin the raycast check for the camera's line of sight.
    /// </summary>
    private float beginRaycastDistance = 0f;

    void Awake()
    {
        SetMaxPriority(1);
        SetPriority(1);
        isMain = true;
        
        GameInfo.ControlledCamera = this;
        GameInfo.ControlledCameraIsMain = isMain;
    }

    /// <summary>
    ///     Have the main camera look at the controlled blob and track it from <tt>offset</tt>.
    /// </summary>
    void LateUpdate()
    {
        if (trackedTransform != GameInfo.ControlledBlob.gameObject.transform)
        {
            TrackObject(GameInfo.ControlledBlob.gameObject, trackingDistance);
        }
        else
        {
            float deltaX = 0;
            float deltaY = 0;

            if (controlled)
            {
                deltaX = XSensitivity();
                deltaY = YSensitivity();
            }

            // rotate about axis perpendicular to mouse movement by angle proportional to mouse speed
            Vector3 axis = 10 * (transform.up * deltaX - transform.right * deltaY);
            float angle = axis.magnitude * GameInfo.MouseSensitivity * Time.deltaTime * Mathf.Rad2Deg;

            targetOffset = Quaternion.AngleAxis(angle, axis) * targetOffset;

            MoveCamera();
        }
    }

    /// <summary>
    ///     Update the tracked transform and the offset distance of the main camera.
    /// </summary>
    /// <param name="obj">
    ///     The object to track.
    /// </param>
    /// <param name="distance">
    ///     The distance to maintain between the tracked object and the camera.
    /// </param>
    public void TrackObject(GameObject obj, float distance)
    {
        trackedTransform = obj.transform;
        targetOffset = distance * Vector3.left;
        lastPosition = CollideCamera() + trackedTransform.position;

        MoveCamera();
    }

    /// <summary>
    ///     Offset camera from the tracked object's position, then look at it.
    /// </summary>
    private void MoveCamera() {
        if (trackedTransform == null)
            return;

        transform.position = CollideCamera() + trackedTransform.position;
        if ((transform.position - lastPosition).magnitude < EPSILON)
        {
            // reduce shaking if the camera is not moving much
            transform.position = (transform.position + lastPosition) /  2;
        }
        lastPosition = transform.position;

        transform.LookAt(trackedTransform);
    }

    /// <summary>
    ///     Performs a raycast from the tracked object along <tt>offset</tt>, so that the camera
    ///     doesn't clip any objects.
    /// </summary>
    /// <returns>
    ///     The position that the camera will have to be at (along <tt>offset</tt>) for there to be
    ///     a clear line of sight to the tracked object.
    /// </returns>
    private Vector3 CollideCamera() {
        if (trackedTransform == null)
            return targetOffset;

        RaycastHit hitInfo;
        LayerMask layerMask = ~LayerMask.GetMask("Ignore Camera");

        Vector3 rayDirection = targetOffset.normalized;
        bool hitSomething = Physics.Raycast(
            trackedTransform.position + beginRaycastDistance * rayDirection,
            rayDirection,
            out hitInfo,
            targetOffset.magnitude,
            layerMask.value,
            QueryTriggerInteraction.Ignore
        );

        if (hitSomething)
            return 0.9f * hitInfo.distance * targetOffset.normalized;
        else
            return targetOffset;
    }

    /// <returns>
    ///     The X component of the mouse input, scaled down if closer to <tt>Vector3.up</tt> or
    ///     <tt>Vector3.down</tt>.
    /// </returns>
    float XSensitivity()
    {
        // x component of rotation is less sensitive closer to the poles
        float mouse_X = Input.GetAxis("Mouse X") * Mathf.Abs(AngleToVertical(targetOffset) / 90);
        return mouse_X;
    }

    /// <returns>
    ///     The Y component of the mouse input, clamped to not go within 5 degrees of
    ///     <tt>Vector3.up</tt> or <tt>Vector3.down</tt>.
    /// </returns>
    float YSensitivity()
    {
        // y component of rotation cannot go within 5 degrees of the poles
        float mouse_Y = Input.GetAxis("Mouse Y");
        float atv = AngleToVertical(targetOffset);
        if (Mathf.Abs(atv) < 5 && Mathf.Sign(mouse_Y) != Mathf.Sign(atv))
            mouse_Y = 0;

        return mouse_Y;
    }

    /// <param name="v">
    ///     The vector to test.
    /// </param>
    /// <returns>
    ///     The smallest angle (in degrees) between the given vector and one of <tt>Vector3.up</tt>
    ///     or <tt>Vector3.down</tt>.
    /// </returns>
    float AngleToVertical(Vector3 v) {
        float angle = Vector3.Angle(v, Vector3.up);

        if (angle <= 90)
            return angle;
        else
            return angle - 180;
    }
}
