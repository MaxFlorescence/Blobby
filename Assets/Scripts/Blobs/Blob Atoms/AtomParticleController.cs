using System.IO;
using UnityEngine;

/// <summary>
///     A class that controls the particles for each atom of a blob.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class AtomParticleController : MonoBehaviour
{
    /// <summary>
    ///     Particle system controlling dripping from the blob's atoms.
    /// </summary>
    private ParticleSystem atomParticles;

    void Awake() {
        atomParticles = GetComponent<ParticleSystem>();
    }

    public void SetParticles(
        (AtomParticleBehaviorStruct behavior, Material material, Mesh mesh) particleData
    )
    {
        SetMain(particleData.behavior);
        SetEmission(particleData.behavior);
        SetShape(particleData.behavior);
        SetInheritVelocity(particleData.behavior);
        SetSizeOverLifetime(particleData.behavior);
        SetCollision(particleData.behavior);
        SetRenderer(particleData.material, particleData.mesh);
    }

    private void SetMain(AtomParticleBehaviorStruct particleData)
    {
        var main = atomParticles.main;
        main.startSpeed = particleData.startSpeed;
        main.gravityModifier = particleData.gravity;
    }

    private void SetEmission(AtomParticleBehaviorStruct particleData)
    {
        var emission = atomParticles.emission;
        emission.rateOverTime = particleData.rateOverTime;
    }

    private void SetShape(AtomParticleBehaviorStruct particleData)
    {
        var shape = atomParticles.shape;
        shape.radius = particleData.radius;
        shape.alignToDirection = particleData.alignToDirection;
    }

    private void SetInheritVelocity(AtomParticleBehaviorStruct particleData)
    {
        var inheritVelocity = atomParticles.inheritVelocity;
        inheritVelocity.enabled = particleData.inheritVelocity;
    }

    private void SetSizeOverLifetime(AtomParticleBehaviorStruct particleData)
    {
        var sizeOverLifetime = atomParticles.sizeOverLifetime;
        AnimationCurve curve = new();
        curve.AddKey(0.00f, 1.00f);
        curve.AddKey(particleData.persistTime, 1.00f);
        curve.AddKey(particleData.persistTime + particleData.fadeTime, 0.00f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(particleData.initialSize, curve);
    }

    private void SetCollision(AtomParticleBehaviorStruct particleData)
    {
        var collision = atomParticles.collision;
        collision.enabled = particleData.collision;
    }

    private void SetRenderer(Material material, Mesh mesh)
    {
        if (atomParticles.TryGetComponent<ParticleSystemRenderer>(out var renderer))
        {
            bool useMesh = mesh != null;

            renderer.renderMode = useMesh
                ? ParticleSystemRenderMode.Mesh
                : ParticleSystemRenderMode.Billboard;

            renderer.alignment = useMesh
                ? ParticleSystemRenderSpace.Velocity
                : ParticleSystemRenderSpace.Facing;

            if (useMesh) renderer.mesh = mesh;

            renderer.material = material;
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