using UnityEngine;

[RequireComponent(typeof(Camera))]

/// <summary>
///     An interface for cameras to broadcast their priority and usability.
///     Only one camera can be active at a time, so which one is active should depend on the
///     individual camera's priority and usability.
/// </summary>
public abstract class PriorityCamera : Controllable
{
    /// <summary>
    ///     The maximum priority this camera can have.
    /// </summary>
    private int maxPriority = 0;

    /// <summary>
    ///     The camera's current priority.
    /// </summary>
    private int priority = 0;
    protected bool isMain = false;

    /// <summary>
    ///     Set the camera's priority without exceeding the maximum priority.
    /// </summary>
    protected void SetPriority(int newPriority)
    {
        priority = (newPriority > maxPriority) ? maxPriority : newPriority;
    }

    /// <summary>
    ///     Set the camera's maximum priority and update the current priority if needed.
    /// </summary>
    protected void SetMaxPriority(int newMaxPriority)
    {
        maxPriority = newMaxPriority;
        if (priority > maxPriority)
        {
            priority = maxPriority;
        }
    }

    public int GetPriority()
    {
        return priority;
    }

    public bool IsMain()
    {
        return isMain;
    }

    private void EnableCamera(bool enable)
    {
        gameObject.GetComponent<Camera>().enabled = enable;
        gameObject.GetComponent<AudioListener>().enabled = enable;
    }

    public void Activate()
    {
        EnableCamera(true);
        controlled = true;
        GameInfo.ControlledCamera = this;
        GameInfo.ControlledCameraIsMain = isMain;
        OnActivate();
    }

    virtual protected void OnActivate() {}

    public void Deactivate()
    {
        EnableCamera(false);
        controlled = false;
        OnDeactivate();
    }

    virtual protected void OnDeactivate() {}
}