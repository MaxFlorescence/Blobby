using UnityEngine;

public class CutsceneCameraController : MonoBehaviour
{
    private float[] pauseTimes = {
        2f,
        1.5f,
        1f,
        0.5f
    };
    private float[] moveTimes = {
        0f,
        2f,
        4f,
        2f
    };
    private Vector3[] positions = {
        new Vector3(-30, 40, -47),
        new Vector3(-40, 30, -40),
        new Vector3(47, 30, 47),
        new Vector3(47, 30, 47)
    };
    private Vector3[] directions = {
        new Vector3(0, 0, -1),
        new Vector3(-1, 0, -1),
        new Vector3(-1, 0, -1),
        new Vector3(0, -1, 0)
    };

    private int points;
    private int currentPoint = -1;
    private float t = 0;
    private bool cutsceneFinished = false;

    void Start()
    {
        points = positions.Length;

        foreach (Vector3 direction in directions)
        {
            direction.Normalize();
        }
    }

    public void BeginCutscene()
    {
        currentPoint = 0;
        transform.position = positions[0];
        transform.LookAt(positions[0] + directions[0]);
    }

    void LateUpdate()
    {
        if (currentPoint < points && currentPoint >= 0)
        {
            if (moveTimes[currentPoint] > 0 && t <= moveTimes[currentPoint])
            {
                // interpolate position linearly between the last position and the target position
                Vector3 position = Vector3.Lerp(positions[currentPoint - 1], positions[currentPoint], t / moveTimes[currentPoint]);
                // interpolate direction spherically between the last direction and the target direction
                Vector3 direction = Vector3.Slerp(directions[currentPoint - 1], directions[currentPoint], t / moveTimes[currentPoint]);

                transform.position = position;
                transform.LookAt(position + direction);
            }
            else if (t >= moveTimes[currentPoint] + pauseTimes[currentPoint])
            {
                // interpolations finished
                t = 0;
                currentPoint++;
            }

            t += Time.deltaTime;
        }
        else if (currentPoint == points)
        {
            cutsceneFinished = true;
        }
    }

    public void Reset()
    {
        cutsceneFinished = false;
        currentPoint = -1;
    }
    
    public bool Finished()
    {
        return cutsceneFinished;
    }
}
