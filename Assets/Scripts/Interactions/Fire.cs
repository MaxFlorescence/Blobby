using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
/// <summary>
///     Controls effects of fires being ignited and extinguished.
/// </summary>
public class Fire : Interactable
{
    // ---------------------------------------------------------------------------------------------
    // TIMER
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The amount of time (seconds) to spend fading between material strengths.
    /// </summary>
    public float fadeTime = 1;
    private Timer timer;

    // ---------------------------------------------------------------------------------------------
    // LIGHT
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The point light controlled by <tt>fireLight</tt>.
    /// </summary>
    private FireLight fireLight;
    private ParticleSystem fireParticles;

    // ---------------------------------------------------------------------------------------------
    // MATERIAL
    // ---------------------------------------------------------------------------------------------
    private float targetStrength = 1;
    private float lastStrength = 1;
    /// <summary>
    ///     The gameobject with the <tt>fireRenderers</tt> materials.
    /// </summary>
    public GameObject flames;
    /// <summary>
    ///     The materials whose strengths will be faded.
    /// </summary>
    private Renderer[] fireRenderers;

    // ---------------------------------------------------------------------------------------------
    // AUDIO
    // ---------------------------------------------------------------------------------------------
    private AudioSource audioSource;
    private AudioClip fireIgnite;
    private AudioClip fireSizzle;

    void Start()
    {
        fireLight = GetComponentsInChildren<FireLight>()[0];
        fireParticles = GetComponentsInChildren<ParticleSystem>()[0];

        audioSource = GetComponent<AudioSource>();
        fireIgnite = Resources.Load<AudioClip>(FileUtilities.SOUNDS_PATH + "fire_ignite");
        fireSizzle = Resources.Load<AudioClip>(FileUtilities.SOUNDS_PATH + "fire_sizzle");

        fireRenderers = flames.GetComponentsInChildren<Renderer>().ToArray();
        timer = new(fadeTime);
    }

    protected override void OnUpdate()
    {
        SetFlameStrength(Mathf.Lerp(targetStrength, lastStrength, timer.RemainingProgress()));

        if (timer.Update() && lastStrength != targetStrength) SetFlameStrength(targetStrength);
    }

    /// <summary>
    ///     Sets the flame's renderer material's strength property. Zero corresponds to completely
    ///     black. One corresponds to unmodified.
    /// </summary>
    private void SetFlameStrength(float newStrength)
    {
        foreach (Renderer renderer in fireRenderers)
        {
            renderer.material.SetFloat("_Strength", newStrength);
        }

        lastStrength = newStrength;
    }

    /// <summary>
    ///     Put out the fire on interaction
    /// </summary>
    protected override void OnInteract(BlobController blob)
    {
        bool interacted = false;

        if (blob.BlobMaterialsHas(BlobMaterialProperties.Can_Ignite)) {
            Ignite();
            interacted = true;
        }
        else if (blob.BlobMaterialsHas(BlobMaterialProperties.Can_Extinguish)) 
        {
            Extinguish();
            interacted = true;
        }

        if (interacted)
        {
            StartInteractionCooldown(10);   
        }
    }

    private void Ignite()
    {
        SetTargetMaterialStrength(1);
        fireParticles.Play();
        fireLight.IsOn = true;
        audioSource.PlayOneShot(fireIgnite);
    }

    private void Extinguish()
    {
        SetTargetMaterialStrength(0);
        fireParticles.Stop();
        fireLight.IsOn = false;
        audioSource.PlayOneShot(fireSizzle);
    }

    private void SetTargetMaterialStrength(float newStrength)
    {
        targetStrength = newStrength;
        timer.Reset();
    }
}