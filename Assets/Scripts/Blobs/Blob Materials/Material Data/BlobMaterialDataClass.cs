using System.IO;
using UnityEngine;

public abstract class BlobMaterialDataClass
{
    private static readonly Mesh ICOSAHEDRON;
    private static readonly Mesh SOFT_CUBE;
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
    protected static readonly AtomParticleBehaviorStruct SPARKLE_BEHAVIOR = new() {
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
    protected static readonly AtomParticleBehaviorStruct NONE_BEHAVIOR = new() {emission = false};

    /// <summary>
    ///     The properties of this material.
    /// </summary>
    public virtual BlobMaterialProperties Properties { get; }

    /// <summary>
    ///     The material that is applied to the blob's droplets.
    /// </summary>
    public virtual AtomParticleBehaviorStruct ParticleBehavior => DROPLET_BEHAVIOR;

    public virtual (BlobMaterialProperties, BlobMaterial)[] Transitions { get; protected set; } = {};

    public Material ParticleMaterial { get; private set; }
    public Mesh ParticleMesh { get; private set; }

    /// <summary>
    ///     The material that is applied to the blob's body.
    /// </summary>
    public Material BodyMaterial { get; private set; }

    static BlobMaterialDataClass()
    {
        ICOSAHEDRON = Resources.Load<GameObject>(
            Path.Combine(FileUtilities.CUSTOM_OBJECTS, "icosahedron")
        ).GetComponent<MeshFilter>().sharedMesh;
        
        SOFT_CUBE = Resources.Load<GameObject>(
            Path.Combine(FileUtilities.CUSTOM_OBJECTS, "soft_cube")
        ).GetComponent<MeshFilter>().sharedMesh;
    }

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