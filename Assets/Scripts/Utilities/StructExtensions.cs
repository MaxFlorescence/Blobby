/// <summary>
///     A class defining extensions for <tt>Vector3</tt>s.
/// </summary>
public static class StructExtensions
{
    /// <returns>
    ///     A clone of the given struct.
    /// </returns>
    public static T Clone<T>(this T cloneMe) where T : struct
    {
        return cloneMe;
    }
}