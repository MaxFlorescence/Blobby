/// <summary>
///     A struct for storing a blob's sound families together.
/// </summary>
public readonly struct BlobSoundFamiliesStruct
{
    /// <summary>
    ///     The sound family that should play when the blob touches something.
    /// </summary>
    public readonly SoundFamily collision;

    /// <summary>
    ///     The sound family that should play on loop.
    /// </summary>
    public readonly SoundFamily background;
    
    /// <summary>
    ///     The sound family that should play when the blob starts using this collection.
    /// </summary>
    public readonly SoundFamily toTransition;
    
    /// <summary>
    ///     The sound family that should play when the blob stops using this collection.
    /// </summary>
    public readonly SoundFamily fromTransition;
    
    /// <summary>
    ///     The sound family that should play when the blob takes damage.
    /// </summary>
    public readonly SoundFamily damage;
    
    /// <summary>
    ///     The sound family that should play when the blob dies.
    /// </summary>
    public readonly SoundFamily death;

    public BlobSoundFamiliesStruct(SoundFamily collision      = null,
                                   SoundFamily background     = null,
                                   SoundFamily toTransition   = null,
                                   SoundFamily fromTransition = null,
                                   SoundFamily damage         = null,
                                   SoundFamily death          = null)
    {
        this.collision = collision ?? SoundFamily.NONE;
        this.background = background ?? SoundFamily.NONE;
        this.toTransition = toTransition ?? SoundFamily.NONE;
        this.fromTransition = fromTransition ?? SoundFamily.NONE;
        this.damage = damage ?? SoundFamily.NONE;
        this.death = death ?? SoundFamily.NONE;
    }
}