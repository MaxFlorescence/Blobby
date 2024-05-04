using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningObject : MonoBehaviour
{
    public float speed;
    public Vector3 axis;
    public Vector3 offset;
    public bool fixOrientation = false;
    public float swing = 0;

    private Vector3 origin;
    private float maxDist;
    private Vector3 start;
    private Vector3 target;
    private int direction = 1;
    private int correction = 200;

    void Start() {
        origin = transform.position + offset;
    }

    void Update()
    {
        transform.RotateAround(origin, axis, speed * direction * Time.timeScale * Time.deltaTime * correction);

        if (swing != 0) {
            float angularDist = Vector3.SignedAngle(-offset, transform.position - origin, axis);

            if (Mathf.Abs(angularDist) > swing) {
                transform.RotateAround(origin, axis, -Mathf.Sign(angularDist) * (Mathf.Abs(angularDist) - swing + 1));
                direction *= -1;
            }
        }

        if (fixOrientation) {
            transform.Rotate(axis, -speed * direction * Time.timeScale * Time.deltaTime * correction);
        }
    }
}
