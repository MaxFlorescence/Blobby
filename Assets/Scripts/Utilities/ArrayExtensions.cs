using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A class that defines different extensions for generic arrays.
/// </summary>
public static class ArrayExtensions
{
    /// <typeparam name="T">
    ///     The type of element in the array.
    /// </typeparam>
    /// <param name="query">
    ///     The item to check for.
    /// </param>
    /// <param name="data">
    ///     The array itself.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the given array contains the given item.
    /// </returns>
    public static bool Contains<T>(this T[] data, T query)
    {
        foreach (T item in data)
        {
            if (item.Equals(query)) return true;
        }
        return false;
    }

    /// <typeparam name="T">
    ///     The type of element in the array.
    /// </typeparam>
    /// <param name="data">
    ///     The array itself.
    /// </param>
    /// <param name="i">
    ///     The integer index of the desired element.
    /// </param>
    /// <returns>
    ///     The item of the array that corresponds to the given index, modulo the array's length.
    /// </returns>
    public static T ModularGet<T>(this T[] data, int i)
    {
        return data[i.Modulo(data.Length)];
    }

    /// <summary>
    ///     Sets the element of the array at the given index, modulo the array's length, to be the
    ///     given value.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of element in the array.
    /// </typeparam>
    /// <param name="data">
    ///     The array itself.
    /// </param>
    /// <param name="i">
    ///     The integer index of the desired element.
    /// </param>
    public static void ModularSet<T>(this T[] data, int i, T value)
    {
        data[i.Modulo(data.Length)] = value;
    }

    /// <typeparam name="T">
    ///     The type of element in the array.
    /// </typeparam>
    /// <param name="data">
    ///     The array itself.
    /// </param>
    /// <returns>
    ///     A uniform random element of the given array.
    /// </returns>
    public static T SelectRandom<T>(T[] data)
    {
        return data[Random.Range(0, data.Length)];
    }

    /// <typeparam name="T">
    ///     The type of element in the array.
    /// </typeparam>
    /// <param name="data">
    ///     The array itself.
    /// </param>
    /// <returns>
    ///     An <tt>IEnumerable</tt> over a random permutation of the given array.
    /// </returns>
    public static IEnumerable<T> Shuffled<T>(this T[] data)
    {
        foreach (int i in ArgShuffle(data))
        {
            yield return data[i];
        }

        yield break;
    }

    /// <typeparam name="T">
    ///     The type of element in the array.
    /// </typeparam>
    /// <param name="data">
    ///     The array itself.
    /// </param>
    /// <returns>
    ///     A random permutation of the integers from <tt>0</tt> to <tt>data.length - 1</tt>.
    /// </returns>
    private static int[] ArgShuffle<T>(T[] data)
    {
        int n = data.Length;
        int[] indices = new int[n];
        int zeroIndex = 0;

        for (int i = n-1; i > 0; i--)
        {
            int k = Random.Range(0, i-1);

            indices[i] = (indices[k] == 0) ? k+1 : indices[k];
            if (indices[i] == n) zeroIndex = i;

            indices[k] = i+1;
        }
        if (n > 0) indices[zeroIndex] = 0;

        return indices;
    }
}