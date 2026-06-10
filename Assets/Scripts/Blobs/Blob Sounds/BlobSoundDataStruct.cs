using UnityEngine;

/// <summary>
///     A struct for holding data about a blob sound family.
/// </summary>
public readonly struct BlobSoundDataStruct
{
    /// <summary>
    ///     The default volume for all audio played by this <tt>BlobSoundController</tt>.
    /// </summary>
    private static readonly float DEFAULT_VOLUME = 1f;

    /// <summary>
    ///     The default minimum and maximum pitches that collision sounds can be played with.
    /// </summary>
    private static readonly Vector2 DEFAULT_PITCH_BOUNDS  = new(0.5f, 1.5f);

    /// <summary>
    ///     The subdirectory of <tt>FileUtilities.Sounds</tt> in which to look for sound files.
    /// </summary>
    public readonly string family;

    /// <summary>
    ///     The volume to apply to collision sounds.
    /// </summary>
    public readonly float collideVolume;

    /// <summary>
    ///     The volume to apply the background sound.
    /// </summary>
    public readonly float backgroundVolume;

    /// <summary>
    ///     The pitch boundaries to randomly choose from and apply to collision sounds.
    /// </summary>
    public readonly Vector2 collidePitchBounds;

    public BlobSoundDataStruct(string family = null, float? collideVolume = null,
                               float? backgroundVolume = null, Vector2? collidePitchBounds = null)
    {
        this.family = family;
        this.collideVolume = collideVolume ?? DEFAULT_VOLUME;
        this.backgroundVolume = backgroundVolume ?? DEFAULT_VOLUME;
        this.collidePitchBounds = collidePitchBounds ?? DEFAULT_PITCH_BOUNDS;
    }
}