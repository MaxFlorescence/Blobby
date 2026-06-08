using System.IO;
using UnityEngine;

/// <summary>
///     A class that controls the particles for each atom of a blob.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class AtomParticleController : MonoBehaviour
{
    public static readonly AtomParticleDataStruct Missing = new()
    {
        useGravity = true,
        startSpeed = 0,
        rateOverTime = 0.25f,
        radius = 1,
        rotation = Vector3.zero,
        alignToDirection = false,
        inheritVelocity = true,
        initialSize = 50,
        fadeTime = 1,
        collision = true,
        useMesh = true,
        mesh = "icosahedron",
        material = "MISSING"
    };
    public static readonly AtomParticleDataStruct Water = new()
    {
        useGravity = true,
        startSpeed = 0,
        rateOverTime = 0.25f,
        radius = 1,
        rotation = Vector3.zero,
        alignToDirection = false,
        inheritVelocity = true,
        initialSize = 50,
        fadeTime = 1,
        collision = true,
        useMesh = true,
        mesh = "icosahedron",
        material = "WaterJelly"
    };

    public static readonly AtomParticleDataStruct Lava = new()
    {
        useGravity = true,
        startSpeed = 0,
        rateOverTime = 0.25f,
        radius = 1,
        rotation = Vector3.zero,
        alignToDirection = false,
        inheritVelocity = true,
        initialSize = 50,
        fadeTime = 1,
        collision = true,
        useMesh = true,
        mesh = "icosahedron",
        material = "LavaJelly"
    };

    /// <summary>
    ///     Particle system controlling dripping from the blob's atoms.
    /// </summary>
    private ParticleSystem atomParticles;

    void Awake() {
        atomParticles = GetComponent<ParticleSystem>();
    }

    public void SetParticles(AtomParticleDataStruct particleData)
    {
        SetMain(particleData);
        SetEmission(particleData);
        SetShape(particleData);
        SetInheritVelocity(particleData);
        SetSizeOverLifetime(particleData);
        SetCollision(particleData);
        SetRenderer(particleData);
    }

    private void SetMain(AtomParticleDataStruct particleData)
    {
        var main = atomParticles.main;
        main.startSpeed = particleData.startSpeed;
        main.gravityModifier = particleData.useGravity ? 1 : 0;
    }

    private void SetEmission(AtomParticleDataStruct particleData)
    {
        var emission = atomParticles.emission;
        emission.rateOverTime = particleData.rateOverTime;
    }

    private void SetShape(AtomParticleDataStruct particleData)
    {
        var shape = atomParticles.shape;
        shape.radius = particleData.radius;
        shape.rotation = particleData.rotation;
        shape.alignToDirection = particleData.alignToDirection;
    }

    private void SetInheritVelocity(AtomParticleDataStruct particleData)
    {
        var inheritVelocity = atomParticles.inheritVelocity;
        inheritVelocity.enabled = particleData.inheritVelocity;
    }

    private void SetSizeOverLifetime(AtomParticleDataStruct particleData)
    {
        var sizeOverLifetime = atomParticles.sizeOverLifetime;
        AnimationCurve curve = new();
        curve.AddKey(0.00f, 1.00f);
        curve.AddKey(particleData.fadeTime, 0.00f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(particleData.initialSize, curve);
    }

    private void SetCollision(AtomParticleDataStruct particleData)
    {
        var collision = atomParticles.collision;
        collision.enabled = particleData.collision;
    }

    private void SetRenderer(AtomParticleDataStruct particleData)
    {
        if (atomParticles.TryGetComponent<ParticleSystemRenderer>(out var renderer))
        {
            renderer.renderMode = particleData.useMesh
                ? ParticleSystemRenderMode.Mesh
                : ParticleSystemRenderMode.Billboard;
            if (particleData.useMesh) renderer.mesh = Resources.Load<GameObject>(
                Path.Combine(FileUtilities.CUSTOM_OBJECTS, particleData.mesh)
            ).GetComponent<MeshFilter>().sharedMesh;
            renderer.material = Resources.Load<Material>(
                Path.Combine(FileUtilities.BLOB_MATERIALS, particleData.material)
            );
        }
    }
    
    /// <summary>
    ///     Change the material of each droplet particle.
    /// <param name="material">
    ///     The material to change to.
    /// </param>
    public void SetMaterial(Material material)
    {
        if (atomParticles.TryGetComponent<ParticleSystemRenderer>(out var renderer))
        {
            renderer.material = material;
        }
    }
}