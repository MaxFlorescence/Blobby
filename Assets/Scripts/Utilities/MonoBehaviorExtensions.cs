using System;
using System.Collections;
using UnityEngine;

/// <summary>
///     A class defining extensions for <tt>MonoBehavior</tt>s.
/// </summary>
public static class MonoBehaviourExtensions
{
    /// <summary>
    ///     Executes the given function after the given delay has elapsed using coroutines.
    /// </summary>
    public static void DelayedExecute(this MonoBehaviour monoBehaviour, float delay, Action action)
    {
        if (delay > 0)
        {
            monoBehaviour.StartCoroutine(DelayedExecuteHelper(monoBehaviour, delay, action));
        }
        else
        {
            action.Invoke();
        }
    }

    private static IEnumerator DelayedExecuteHelper(MonoBehaviour monoBehaviour, float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        monoBehaviour.DelayedExecute(0, action);
    }
}