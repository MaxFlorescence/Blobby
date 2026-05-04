using UnityEngine;

public static class FloatExtensions
{
    private const float DEFAULT_EPSILON = 1e-3f;

    /// <param name="epsilon">
    ///     The tolerance for floating-point comparisons.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the actual value is approximately equal to the expected value (within
    ///     <tt>epsilon</tt>).
    /// </returns>
    public static bool Approx(this float actual, float expected, float epsilon = DEFAULT_EPSILON)
    {
        return Mathf.Abs(actual - expected) < epsilon;
    }

    /// <param name="epsilon">
    ///     The tolerance for floating-point comparisons.
    /// </param>
    /// <returns>
    ///     The given number rounded to the nearest integer within <tt>epsilon</tt>, if it exists.
    ///     Otherwise, returns null.
    /// </returns>
    public static int? RoundIfClose(this float number, float epsilon = DEFAULT_EPSILON)
    {
        int rounded = Mathf.RoundToInt(number);
        return number.Approx(rounded, epsilon) ? rounded : null;
    }
}