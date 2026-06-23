/// <summary>
///     A class defining extensions for <tt>string</tt>s.
/// </summary>
public static class StringExtensions
{
    /// <returns>
    ///     <tt>True</tt> iff the given string is a prefix of this string.
    /// </returns>
    public static bool HasPrefix(this string str, string other)
    {
        if (other.Length > str.Length) return false;

        for (int i = 0; i < other.Length; i++)
        {
            if (str[i] != other[i]) return false;
        }

        return true;
    }
}