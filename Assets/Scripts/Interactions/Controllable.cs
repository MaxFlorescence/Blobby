using UnityEngine;

/// <summary>
///     A layer between the input manager and the game object that allows control to be toggled.
/// </summary>
public abstract class Controllable : MonoBehaviour
{
    /// <summary>
    ///     Is this game object controlled?
    /// </summary>
    public bool Controlled = false;
}