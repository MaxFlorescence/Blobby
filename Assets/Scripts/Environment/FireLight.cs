using UnityEngine;

[RequireComponent(typeof(Light))]
class FireLight : MonoBehaviour
{
    public Vector2 intervalRange = new(0.1f, 1f);
    public Vector2 intensityRange = new(0.5f, 1f);
    public float maxDisplacement = 0.25f;

    public bool IsOn { get; set; } = true;

    private Light fireLight;
    float targetIntensity;
    float lastIntensity;
    float interval = 1;
    float timer = 1;

    Vector3 targetPosition;
    Vector3 lastPosition;
    Vector3 origin;

    private void Start()
    {
        fireLight = GetComponent<Light>();
        origin = transform.position;
        lastPosition = origin;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > interval)
        {
            lastIntensity = fireLight.intensity;
            targetIntensity = NextIntensity();
            timer = 0;
            interval = Random.Range(intervalRange.x, intervalRange.y);

            lastPosition = fireLight.transform.position;
            targetPosition = NextPosition();
        }

        fireLight.intensity = Mathf.Lerp(lastIntensity, targetIntensity, timer / interval);
        fireLight.transform.position = Vector3.Lerp(lastPosition, targetPosition, timer / interval);
    }

    public float NextIntensity()
    {
        if (IsOn)
            return Random.Range(intensityRange.x, intensityRange.y);

        return 0;
    }

    public Vector3 NextPosition()
    {
        if (IsOn)
            return origin + Random.insideUnitSphere * maxDisplacement;

        return origin;
    }
}