using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingObject : MonoBehaviour
{
    public float pauseTime;
    public float speed;
    public Vector3 target;
    public bool relative;
    public float initialPause = 0;

    private Vector3 start;
    private Vector3 velocity;
    private int direction = 1;
    private float maxDist;
    private float timer;
    private int correction = 200;

    void Start() {
        if (relative) {
            target += transform.position;
        }

        start = transform.position;
        Vector3 diff = target - start;

        velocity = speed * diff.normalized;
        maxDist = diff.magnitude;

        timer = pauseTime - initialPause;
    }

    void Update()
    {
        if (timer >= pauseTime) {
            transform.Translate(direction * velocity * Time.timeScale * Time.deltaTime * correction, relativeTo:Space.World);

            if (maxDist <= (transform.position - start).magnitude) {
                transform.position = target;
                direction = -1;
                timer = 0;
            } else if (maxDist <= (transform.position - target).magnitude) {
                transform.position = start;
                direction = 1;
                timer = 0;
            }
        } else {
            timer += Time.deltaTime;
        }
    }
}
