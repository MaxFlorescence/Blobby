using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneController : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject cutsceneCamera; // attach controller to this
    public GameObject cutsceneInfo;
    
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
    private int currentPoint = 0;
    private float t = 0;

    void Start() {
        // this flag is only true when starting the game from the main menu
        if (LevelStartupInfo.StartCutscene) {
            mainCamera.SetActive(false);
            cutsceneInfo.SetActive(true);

            points = positions.Length;
            transform.position = positions[0];
            transform.LookAt(positions[0] + directions[0]);

            foreach (Vector3 direction in directions) {
                direction.Normalize();
            }
        } else {
            endCutscene();
        }
    }

    void Update() {
        // press space to skip cutscene
        if (LevelStartupInfo.StartCutscene && Input.GetButtonUp("Jump")) {
            endCutscene();
        }
    }

    void LateUpdate() {
        if (currentPoint < points) {
            if (moveTimes[currentPoint] > 0 && t <= moveTimes[currentPoint]) {
                // interpolate position linearly between the last position and the target position
                Vector3 position = Vector3.Lerp(positions[currentPoint-1], positions[currentPoint], t / moveTimes[currentPoint]);
                // interpolate direction spherically between the last direction and the target direction
                Vector3 direction = Vector3.Slerp(directions[currentPoint-1], directions[currentPoint], t / moveTimes[currentPoint]);

                transform.position = position;
                transform.LookAt(position + direction);
            } else if (t >= moveTimes[currentPoint] + pauseTimes[currentPoint]) {
                // interpolations finished
                t = 0;
                currentPoint++;
            }

            t += Time.deltaTime;
        } else if (cutsceneCamera.activeSelf) {
            endCutscene();
        }
    }

    void endCutscene() {
        mainCamera.SetActive(true);
        cutsceneCamera.SetActive(false);
        cutsceneInfo.SetActive(false);
        LevelStartupInfo.StartCutscene = false;
    }
}
