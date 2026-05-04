using System;
using UnityEngine;

class Utilities : MonoBehaviour
{

    public static int CountNames<T>() where T : Enum
    {
        return Enum.GetNames(typeof(T)).Length;
    }
}