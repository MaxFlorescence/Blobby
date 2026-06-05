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
    private ParticleSystem drips;

    void Awake() {
        drips = GetComponent<ParticleSystem>();
    }
    
    /// <summary>
    ///     Change the material of each droplet particle.
    /// <param name="material">
    ///     The material to change to.
    /// </param>
    public void SetMaterial(Material material)
    {
        if (drips.TryGetComponent<ParticleSystemRenderer>(out var renderer))
        {
            renderer.material = material;
        }
    }
}