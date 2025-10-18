using UnityEngine;

public class ExperimenterScript : MonoBehaviour
{
    public Transform blob;

    void Update()
    {
        if (blob == null)
        {
            if (GameInfo.ControlledBlob != null)
            {
                blob = GameInfo.ControlledBlob.transform;
            }
        }
        else
        {
            Vector3 target = Vector3.ProjectOnPlane(blob.position - transform.position, Vector3.up);
            float angle = Vector3.SignedAngle(-transform.up, target, Vector3.up);

            transform.RotateAround(transform.position, Vector3.up, angle);
        }
    }
}
