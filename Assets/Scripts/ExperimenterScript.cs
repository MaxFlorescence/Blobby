using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimenterScript : MonoBehaviour
{
    public Transform blob;

    void Update()
    {
        Vector3 target = Vector3.ProjectOnPlane(blob.position - transform.position, Vector3.up);
        float angle = Vector3.SignedAngle(-transform.up, target, Vector3.up);

        transform.RotateAround(transform.position, Vector3.up, angle);
    }
}
