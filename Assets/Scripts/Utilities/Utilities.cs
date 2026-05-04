using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;
using IEnumerator = System.Collections.IEnumerator;
using System.IO;
using Unity.Mathematics;

public static class Extensions
{

    public static T Clone<T>(this T cloneMe) where T : struct
    {
        return cloneMe;
    }
}

class Utilities : MonoBehaviour
{

    public static int CountNames<T>() where T : Enum
    {
        return Enum.GetNames(typeof(T)).Length;
    }
}