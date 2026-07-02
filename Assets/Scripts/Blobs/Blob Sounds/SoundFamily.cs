using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
///     A class for holding data about a sound family.
/// </summary>
public class SoundFamily
{
    /// <summary>
    ///     A sound family that has no sounds.
    /// </summary>
    public static readonly SoundFamily NONE = new();

    /// <summary>
    ///     The default volume for a sound.
    /// </summary>
    private static readonly float DEFAULT_VOLUME = 1f;

    /// <summary>
    ///     The default minimum and maximum pitches for a sound.
    /// </summary>
    private static readonly Vector2 DEFAULT_PITCH_BOUNDS  = new(1f, 1f);

    /// <summary>
    ///     The name of the sound family.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     The list of audio clips for this sound family.
    /// </summary>
    private AudioClip[] clips;

    /// <summary>
    ///     A random clip of this sound family.
    /// </summary>
    public AudioClip RandomClip => IsNone ? null
         : IsFamily ? ArrayExtensions.SelectRandom(clips)
         : clips[0];

    /// <summary>
    ///     The volume to apply to sounds.
    /// </summary>
    public float Volume { get; private set; }

    /// <summary>
    ///     The pitch boundaries to randomly choose from and apply to sounds.
    /// </summary>
    public Vector2 PitchBounds { get; private set; }
    
    /// <summary>
    ///     <tt>True</tt> iff this sound family has no sounds.
    /// </summary>
    public bool IsNone => Name == "NONE";

    /// <summary>
    ///     <tt>False</tt> iff this sound family has only one sound.
    /// </summary>
    public bool IsFamily { get; private set; } = true;

    public SoundFamily(string name = "NONE", float? volume = null, Vector2? pitchBounds = null, bool blobSounds = true)
    {
        Name = name;
        Volume = volume ?? DEFAULT_VOLUME;
        PitchBounds = pitchBounds ?? DEFAULT_PITCH_BOUNDS;

        if (!IsNone) LoadClips(blobSounds);
    }

    /// <summary>
    ///     Loads the audio clips specified by this sound family's parameters.
    /// </summary>
    private void LoadClips(bool blobSounds) {
        string resourcePath = Path.Combine(
            blobSounds ? FileUtilities.BLOB_SOUNDS : FileUtilities.SOUNDS,
            Name
        );
        IsFamily = FileUtilities.IsDirectory(resourcePath);

        if (!IsFamily)
        {
            clips = new AudioClip[1] {Resources.Load<AudioClip>(resourcePath)};
            return;
        }

        clips = FileUtilities.GetResources(resourcePath)
                             .Select(resource => Resources.Load<AudioClip>(resource))
                             .ToArray();

        if (clips.Length == 1) IsFamily = false;
    }
}