using UnityEngine;

/// <summary>
///     A class for controlling the light of a fire source.
/// </summary>
[RequireComponent(typeof(Light))]
class FireLight : MonoBehaviour
{
    // ---------------------------------------------------------------------------------------------
    // LIGHT
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     Indicates if the fire light is on or off.
    /// </summary>
    public bool IsOn { get; set; } = true;
    /// <summary>
    ///     The light component controlled by this fire light.
    /// </summary>
    private Light lightComponent;

    // ---------------------------------------------------------------------------------------------
    // INTENSITY
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The minimum and maximum intensities that the fire light can attain.
    /// </summary>
    public Vector2 intensityRange = new(0.5f, 1f);
    /// <summary>
    ///     The intensity for the fire light to target during the current timer interval.
    /// </summary>
    private float targetIntensity = 1;
    /// <summary>
    ///     The intensity of the fire light at the end of the last timer interval.
    /// </summary>
    private float lastIntensity = 1;

    // ---------------------------------------------------------------------------------------------
    // INTERVAL
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The minimum and maximum intervals that the fire light's timer can last for.
    /// </summary>
    public Vector2 intervalRange = new(0.1f, 1f);
    private Timer timer = new(1);

    // ---------------------------------------------------------------------------------------------
    // DISPLACEMENT
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The maximum displacement from the origin that the fire light can attain.
    /// </summary>
    public float maxDisplacement = 0.25f;
    /// <summary>
    ///     The position to move the fire light toward during the current timer interval.
    /// </summary>
    private Vector3 targetPosition;
    /// <summary>
    ///     The position of the fire light at the end of the last timer interval.
    /// </summary>
    private Vector3 lastPosition;
    /// <summary>
    ///     The starting position of the fire light.
    /// </summary>
    private Vector3 origin;

    private void Start()
    {
        lightComponent = GetComponent<Light>();
        origin = transform.position;
        lastPosition = origin;
        targetIntensity = NextIntensity();
    }

    void Update()
    {
        if (timer.Update())
        {
            lastIntensity = lightComponent.intensity;
            targetIntensity = NextIntensity();
            timer.SetInterval(Random.Range(intervalRange.x, intervalRange.y));

            lastPosition = lightComponent.transform.position;
            targetPosition = NextPosition();
        }

        // interpolate the fire light to the next intensity and position
        lightComponent.intensity = Mathf.Lerp(lastIntensity, targetIntensity, timer.Progress());
        lightComponent.transform.position = Vector3.Lerp(lastPosition, targetPosition, timer.Progress());
    }

    /// <summary>
    ///     Selects a random intensity for the fire light to target during its next interval.
    /// </summary>
    /// <returns>
    ///     If the light is on, then a random float in the range defined by <tt>intensityRange</tt>.
    ///     Otherwise 0.
    /// </returns>
    public float NextIntensity()
    {
        if (IsOn)
            return Random.Range(intensityRange.x, intensityRange.y);

        return 0;
    }

    /// <summary>
    ///     Selects a random position for the fire light to target during its next interval.
    /// </summary>
    /// <returns>
    ///     If the light is on, then a random point at most <tt>maxDisplacement</tt> away from the
    ///     light's <tt>origin</tt>. Otherwise the <tt>origin</tt>.
    /// </returns>
    public Vector3 NextPosition()
    {
        if (IsOn)
            return origin + Random.insideUnitSphere * maxDisplacement;

        return origin;
    }
}