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

    /// <summary>
    ///     The property to use when determining the material to transition the target to. Ignored
    ///     if <tt>forceMaterial</tt> is not <tt>BlobMaterial.None</tt>.
    /// </summary>
    public BlobMaterialProperties transitionProperty = BlobMaterialProperties.None;

    /// <summary>
    ///     The material to forcibly transition the target to. If this is not
    ///     <tt>BlobMaterial.None</tt>, then <tt>transitionProperty</tt> will be ignored.
    /// </summary>
    public BlobMaterial forceMaterial = BlobMaterial.None;

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
    ///     Complete the transition
    /// </summary>
    protected override void OnInteractionCooldownEnd()
    {
        if (transitionTarget != null && transitionTarget.IsTouching(gameObject)) {
            transitionTarget.SetBlobMaterials(forceMaterial != BlobMaterial.None
                ? forceMaterial
                : BlobMaterialExtensions.TransistionUsing(
                    transitionTarget.Material, transitionProperty
                )
            );
        }
    }
}