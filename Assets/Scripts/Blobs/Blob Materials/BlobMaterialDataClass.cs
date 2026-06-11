using System.IO;
using UnityEngine;

/// <summary>
///     A class for holding data related to a single blob material.
/// </summary>
public abstract class BlobMaterialDataClass
{
    // ---------------------------------------------------------------------------------------------
    // BODY MATERIAL
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The blob material to use if a requested material doesn't exist.
    /// </summary>
    public static readonly MissingBlobMaterial MISSING_BLOB_MATERIAL = new();

    /// <summary>
    ///     The material that is applied to the blob's body.
    /// </summary>
    public Material BodyMaterial { get; private set; }

    /// <summary>
    ///     The properties of this blob material.
    /// </summary>
    public virtual BlobMaterialProperties Properties { get; } = BlobMaterialProperties.None;

    /// <summary>
    ///     The list of (<tt>properties</tt>, <tt>material</tt>) transition pairs, where if given
    ///     any of the corresponding <tt>properties</tt>, then the blob is transitioned to
    ///     <tt>material</tt>.
    /// </summary>
    public virtual (BlobMaterialProperties, BlobMaterial)[] Transitions { get; protected set; } = {};

    // ---------------------------------------------------------------------------------------------
    // SOUNDS
    // ---------------------------------------------------------------------------------------------
    protected static readonly SoundFamily SQUISH_FAMILY = new(
        name: "Squish",
        volume: 0.05f,
        pitchBounds: new(0.5f, 1.5f)
    );
    protected static readonly SoundFamily DEEP_SQUISH_FAMILY = new(
        name: "Deep Squish",
        volume: 0.05f,
        pitchBounds: new(0.5f, 1.5f)
    );
    protected static readonly SoundFamily RUMBLE_FAMILY = new(
        name: "Rumble",
        volume: 0.25f,
        pitchBounds: new(0.25f, 1f)
    );
    protected static readonly SoundFamily CLINK_CRUNCH_FAMILY = new(
        name: "Clink Crunch",
        volume: 0.8f,
        pitchBounds: new(0.5f, 1.5f)
    );
    protected static readonly SoundFamily WET_CRUNCH_FAMILY = new(
        name: "Wet Crunch",
        volume: 0.8f,
        pitchBounds: new(0.5f, 1.5f)
    );
    protected static readonly SoundFamily DRY_CRUNCH_FAMILY = new(
        name: "Dry Crunch",
        volume: 0.8f,
        pitchBounds: new(0.5f, 1.5f)
    );
    protected static readonly SoundFamily LAVA_SOUND = new(
        name: "Lava",
        volume: 0.5f
    );
    protected static readonly SoundFamily BLAZE_SOUND = new(
        name: "Blaze",
        volume: 0.1f
    );
    protected static readonly SoundFamily IGNITE_SOUND = new(
        name: "Fire Ignite",
        volume: 0.3f
    );
    protected static readonly SoundFamily EXTINGUISH_SOUND = new(
        name: "Fire Extinguish",
        volume: 0.3f
    );
    protected static readonly SoundFamily FIZZING_SOUND = new(
        name: "Fizzing",
        volume: 2f
    );
    protected static readonly SoundFamily BUBBLES_FIZZING_SOUND = new(
        name: "Bubbles Fizzing",
        volume: 0.5f
    );
    
    public virtual BlobSoundFamiliesStruct SoundFamilies { get; } = new(SoundFamily.NONE);
    
    // ---------------------------------------------------------------------------------------------
    // PARTICLES
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     An icosahedron mesh.
    /// </summary>
    private static readonly Mesh ICOSAHEDRON;

    /// <summary>
    ///     A cube mesh with beveled corners.
    /// </summary>
    private static readonly Mesh SOFT_CUBE;

    /// <summary>
    ///     The particle behavior for liquid droplets.
    /// </summary>
    protected static readonly AtomParticleBehaviorStruct DROPLET_BEHAVIOR = new() {
        emission = true,
        gravity = 1,
        startSpeed = 0,
        rateOverTime = 0.25f,
        radius = 1,
        alignToDirection = false,
        inheritVelocity = true,
        initialSize = 50,
        persistTime = 0,
        fadeTime = 1,
        collision = true
    };

    /// <summary>
    ///     The particle behavior for dust clouds.
    /// </summary>
    protected static readonly AtomParticleBehaviorStruct DUST_BEHAVIOR = new() {
        emission = true,
        gravity = 1,
        startSpeed = 1,
        rateOverTime = 0.25f,
        radius = 2,
        alignToDirection = false,
        inheritVelocity = false,
        initialSize = 1,
        persistTime = 0.5f,
        fadeTime = 0.5f,
        collision = true
    };
    
    /// <summary>
    ///     The particle behavior for fluttering particles.
    /// </summary>
    protected static readonly AtomParticleBehaviorStruct FLUTTER_BEHAVIOR = new() {
        emission = true,
        gravity = 0,
        startSpeed = 1f,
        rateOverTime = 0.25f,
        radius = 1,
        alignToDirection = true,
        inheritVelocity = false,
        initialSize = 1,
        persistTime = 0,
        fadeTime = 1,
        collision = true
    };
    
    /// <summary>
    ///     The particle behavior for sparkles.
    /// </summary>
    protected static readonly AtomParticleBehaviorStruct SPARKLE_BEHAVIOR = new() {
        emission = true,
        gravity = 0,
        startSpeed = 1f,
        rateOverTime = 1f,
        radius = 1,
        alignToDirection = false,
        inheritVelocity = false,
        initialSize = 0.5f,
        persistTime = 0,
        fadeTime = 1,
        collision = true
    };

    /// <summary>
    ///     The particle behavior for flames.
    /// </summary>
    protected static readonly AtomParticleBehaviorStruct FLAME_BEHAVIOR = new() {
        emission = true,
        gravity = -0.1f,
        startSpeed = 0,
        rateOverTime = 4f,
        radius = 0.4f,
        alignToDirection = true,
        inheritVelocity = false,
        initialSize = 25,
        persistTime = 0.67f,
        fadeTime = 0.33f,
        collision = true
    };

    /// <summary>
    ///     The particle behavior for having no particles.
    /// </summary>
    protected static readonly AtomParticleBehaviorStruct NONE_BEHAVIOR = new() {emission = false};

    /// <summary>
    ///     The behavior that is applied to the blob's particles.
    /// </summary>
    public virtual AtomParticleBehaviorStruct ParticleBehavior => NONE_BEHAVIOR;

    /// <summary>
    ///     The material that is applied to the blob's particles.
    /// </summary>
    public Material ParticleMaterial { get; private set; }
    
    /// <summary>
    ///     The mesh that is applied (if any) to the blob's particles.
    /// </summary>
    public Mesh ParticleMesh { get; private set; }

    /// <summary>
    ///     Load static mesh resources before any instances of this class are made.
    /// </summary>
    static BlobMaterialDataClass()
    {
        ICOSAHEDRON = Resources.Load<GameObject>(
            Path.Combine(FileUtilities.CUSTOM_OBJECTS, "icosahedron")
        ).GetComponent<MeshFilter>().sharedMesh;
        
        SOFT_CUBE = Resources.Load<GameObject>(
            Path.Combine(FileUtilities.CUSTOM_OBJECTS, "soft_cube")
        ).GetComponent<MeshFilter>().sharedMesh;
    }

    /// <summary>
    ///     Load the materials and meshes associated with this blob material.
    /// </summary>
    /// <param name="bodyMaterial">
    ///     The name of the material to use for the blob's body. Defaults to 
    ///     <tt>FileUtilities.MISSING_MATERIAL</tt>.
    /// </param>
    /// <param name="bodyDirectory">
    ///     The directory in which to find the <tt>bodyMaterial</tt>. Defaults to
    ///     <tt>FileUtilities.BLOB_MATERIALS</tt>.
    /// </param>
    /// <param name="particleMaterial">
    ///     The name of the material to use for the blob's particles. Defaults to match
    ///     <tt>bodyMaterial</tt>.
    /// </param>
    /// <param name="particleDirectory">
    ///     The directory in which to find the <tt>particleMaterial</tt>. Defaults to match
    ///     <tt>bodyDirectory</tt> if <tt>particleMaterial</tt> defaulted, otherwise defaults to
    ///     <tt>FileUtilities.BLOB_MATERIALS</tt>.
    /// </param>
    /// <param name="particleMesh">
    ///     The name of the mesh to use for the blob's particles. Defaults to the particles having
    ///     no mesh (being billboard particles).
    /// </param>
    public BlobMaterialDataClass(string bodyMaterial = null, string bodyDirectory = null,
                                 string particleMaterial = null, string particleDirectory = null,
                                 string particleMesh = null)
    {
        string bodyMaterialPath = (bodyMaterial == null)
            ? FileUtilities.MISSING_MATERIAL
            : Path.Combine(bodyDirectory ?? FileUtilities.BLOB_MATERIALS, bodyMaterial);
        BodyMaterial = Resources.Load<Material>(bodyMaterialPath);
        
        string particleMaterialPath = (particleMaterial == null)
            ? bodyMaterialPath
            : Path.Combine(particleDirectory ?? FileUtilities.BLOB_MATERIALS, particleMaterial);
        ParticleMaterial = Resources.Load<Material>(particleMaterialPath);

        ParticleMesh = particleMesh switch
        {
            "icosahedron" => ICOSAHEDRON,
            "soft_cube" => SOFT_CUBE,
            _ => null
        };
    }
}