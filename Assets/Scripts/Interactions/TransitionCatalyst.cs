/// <summary>
///     A class that transitions a blob's material.
/// </summary>
public class TransitionCatalyst : Interactable
{
    /// <summary>
    ///     How long it takes for the transition to occur. A blob must be touching this object
    ///     for the entire time to transition.
    /// </summary>
    public float transitionDuration = 0;
    public BlobMaterialProperties transitionProperty = BlobMaterialProperties.Heat_Transition;

    /// <summary>
    ///     The blob that is being transitioned.
    /// </summary>
    private BlobController transitionTarget = null;

    /// <summary>
    ///     Begin transition for blobs.
    /// </summary>
    protected override void OnInteract(BlobController blob)
    {
        transitionTarget = blob;
        StartInteractionCooldown(transitionDuration);
    }

    /// <summary>
    ///     Interrupt transition for blobs.
    /// </summary>
    protected override void OnInteractionEnd(BlobController blob)
    {
        transitionTarget = null;
        InterruptInteractionCooldown();
    }

    /// <summary>
    ///     Complete heat transition
    /// </summary>
    protected override void OnInteractionCooldownEnd()
    {
        if (transitionTarget != null && transitionTarget.IsTouching(gameObject)) {
            transitionTarget.SetBlobMaterials(BlobMaterialExtensions.TransistionUsing(
                transitionTarget.Material, transitionProperty
            ));
        }
    }
}