using UnityEngine;

public class MainCameraController : Controllable
{
    private Vector3 offset;
    private Transform trackedTransform = null;
    public float trackingDistance = 10f;

    void Start()
    {
        if (GameInfo.ControlledCamera == null)
        {
            GameInfo.SetControlledCamera(this);
        }
    }

    void LateUpdate()
    {
        if (trackedTransform == null)
        {
            if (GameInfo.ControlledBlob != null)
            {
                TrackObject(GameInfo.ControlledBlob.gameObject, trackingDistance);
            }
        }
        else
        {
            float deltaX = 0;
            float deltaY = 0;

            if (!GameInfo.StartCutscene)
            {
                deltaX = XSensitivity();
                deltaY = YSensitivity();
            }

            // rotate about axis perpendicular to mouse movement by angle proportional to mouse speed
            Vector3 axis = 10 * (transform.up * deltaX - transform.right * deltaY);
            float angle = axis.magnitude * GameInfo.MouseSensitivity * Time.deltaTime * Mathf.Rad2Deg;

            offset = Quaternion.AngleAxis(angle, axis) * offset;

            MoveCamera();
        }
    }

    public void TrackObject(GameObject obj, float distance)
    {
        trackedTransform = obj.transform;
        offset = distance * Vector3.left;

        MoveCamera();
    }

    private void MoveCamera() {
        if (trackedTransform == null)
            return;

        // Offset camera from the tracked object's position, then look at it.

        transform.position = CollideCamera() + trackedTransform.position;
        transform.LookAt(trackedTransform);
    }

    private Vector3 CollideCamera() {
        if (trackedTransform == null)
            return offset;

        RaycastHit hitInfo;
        LayerMask layerMask = ~LayerMask.GetMask("Ignore Camera");

        bool hitSomething = Physics.Raycast(
            trackedTransform.position,
            offset.normalized,
            out hitInfo,
            offset.magnitude,
            layerMask.value,
            QueryTriggerInteraction.Ignore
        );

        if (hitSomething)
            return 0.9f * hitInfo.distance * offset.normalized;
        else
            return offset;
    }

    float XSensitivity()
    {
        // x component of rotation is less sensitive closer to the poles
        float mouse_X = Input.GetAxis("Mouse X") * Mathf.Abs(AngleToVertical(offset) / 90);
        return mouse_X;
    }

    float YSensitivity()
    {
        // y component of rotation cannot go within 5 degrees of the poles
        float mouse_Y = Input.GetAxis("Mouse Y");
        float atv = AngleToVertical(offset);
        if (Mathf.Abs(atv) < 5 && Mathf.Sign(mouse_Y) != Mathf.Sign(atv))
            mouse_Y = 0;

        return mouse_Y;
    }

    float AngleToVertical(Vector3 v) {
        float angle = Vector3.Angle(v, Vector3.up);

        if (angle <= 90)
            return angle;
        else
            return angle - 180;
    }
}
