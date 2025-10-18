using UnityEngine;

/// <summary>
///     An interface for objects that can be interacted with by the blob character.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    /// <summary>
    ///     Time remaining on the interaction cooldown timer.
    /// </summary>
    protected float cooldownTime = 0f;
    /// <summary>
    ///    Interaction is disabled during the cooldown.
    /// </summary>
    private bool interactionEnabled = true;

    /// <summary>
    ///     If needed, wait for cooldown and then re-enable interaction.
    /// </summary>
    public void Update()
    {
        if (!GameInfo.GameIsPaused)
        {
            if (cooldownTime > 0)
            {
                cooldownTime -= Time.deltaTime;
                if (cooldownTime <= 0)
                {
                    interactionEnabled = true;
                    cooldownTime = 0f;
                    OnInteractionCooldownEnd();
                }
            }

            OnUpdate();  
        }
    }

    /// <summary>
    ///     Called by a blob character when it interacts with this object. Interaction only proceeds
    ///     if the cooldown is over.
    /// </summary>
    /// <param name="blob">
    ///     The blob character interacting with this object.
    /// </param>
    /// <returns>
    ///     <tt>true</tt> if the interaction occured, <tt>false</tt> otherwise.
    /// </returns>
    public bool Interact(BlobController blob)
    {
        if (interactionEnabled)
        {
            OnInteract(blob);
            return true;
        }
        return false;
    }

    /// <summary>
    ///    Disable interaction and start a cooldown timer. Re-enable interaction when it ends.
    /// </summary>
    /// <param name="duration">
    ///     The duration of the cooldown period.
    /// </param>
    protected void StartInteractionCooldown(float duration)
    {
        interactionEnabled = false;
        cooldownTime = duration;
        OnInteractionCooldownStart();
    }

    /// <summary>
    ///     Check if the cooldown timer is still running.
    /// </summary>
    /// <returns>
    ///     <tt>true</tt> if the cooldown timer is still running, <tt>false</tt> otherwise.
    /// </returns>
    public bool CoolingDown()
    {
        return cooldownTime > 0;
    }

    /// <summary>
    ///    Code to run when <tt>Update()</tt> is called. Can be overridden by the extending class.
    /// </summary>
    protected virtual void OnUpdate() { }

    /// <summary>
    ///    Code to run when <tt>Interact()</tt> is called. Can be overridden by the extending class.
    /// </summary>
    /// <param name="blob">
    ///     The blob character interacting with this object.
    /// </param>
    protected virtual void OnInteract(BlobController blob) {}

    /// <summary>
    ///    Code to run when the cooldown timer begins. Can be overridden by the extending class.
    /// </summary>
    protected virtual void OnInteractionCooldownStart() {}

    /// <summary>
    ///    Code to run when the cooldown timer is up. Can be overridden by the extending class.
    /// </summary>
    protected virtual void OnInteractionCooldownEnd() {}

    // Getters and setters
    protected void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;
    }

    public bool GetInteractionEnabled()
    {
        return interactionEnabled;
    }
}