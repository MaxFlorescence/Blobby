using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

/// <summary>
///     Controls effects of fires being ignited and extinguished.
/// </summary>
public class Fire : Interactable
{
    public GameObject flames;
    public ParticleSystem particles;
    public Light pointLight;
    public float fadeTime = 1;

    private float timer = 0;
    private float targetStrength = 1;
    private float lastStrength = 1;
    private FireLight fireLight;
    private Renderer[] fireRenderers;
    private AudioSource audioSource;
    private AudioClip fireIgnite, fireSizzle;

    void Start()
    {
        fireLight = pointLight.GetComponent<FireLight>();
        audioSource = GetComponent<AudioSource>();
        fireIgnite = Resources.Load("Sounds/fire_ignite", typeof(AudioClip)) as AudioClip;
        fireSizzle = Resources.Load("Sounds/fire_sizzle", typeof(AudioClip)) as AudioClip;

        fireRenderers = flames.GetComponentsInChildren<Renderer>().ToArray();
    }

    protected override void OnUpdate()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            SetFlameStrength(Mathf.Lerp(targetStrength, lastStrength, timer / fadeTime));
        }
        else if (lastStrength != targetStrength)
        {
            SetFlameStrength(targetStrength);
            timer = fadeTime;
        }

    }

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

        if (blob.canIgnite) {
            Ignite();
            interacted = true;
        }
        else if (blob.canExtinguish) 
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
        FadeStrengthTo(1);
        particles.Play();
        fireLight.IsOn = true;
        audioSource.PlayOneShot(fireIgnite);
    }

    private void Extinguish()
    {
        FadeStrengthTo(0);
        particles.Stop();
        fireLight.IsOn = false;
        audioSource.PlayOneShot(fireSizzle);
    }

    private void FadeStrengthTo(float newStrength)
    {
        targetStrength = newStrength;
        timer = fadeTime;
    }
}