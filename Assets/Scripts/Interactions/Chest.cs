using System.Linq;
using UnityEngine;

public enum ChestState
{
    LOCKED,
    CLOSED,
    OPEN_FULL,
    OPEN_EMPTY
}

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))]

/// <summary>
///     Controls effects of chests being opened and closed.
/// </summary>
public class Chest : Interactable
{
    public ChestState state = ChestState.CLOSED;
    public GameObject contents;

    private Animator animator;
    private AudioSource audioSource;
    private AudioClip chestOpen, chestClose, chestLoot;
    private bool opened = false;
    private bool empty = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        chestOpen = Resources.Load("Sounds/creak_open", typeof(AudioClip)) as AudioClip;
        chestClose = Resources.Load("Sounds/creak_close", typeof(AudioClip)) as AudioClip;
        chestLoot = Resources.Load("Sounds/coins", typeof(AudioClip)) as AudioClip;
    }

    // protected override void OnUpdate()
    // {
    //     if (timer > 0)
    //     {
    //         timer -= Time.deltaTime;
    //         SetFlameStrength(Mathf.Lerp(targetStrength, lastStrength, timer / fadeTime));
    //     }
    //     else if (lastStrength != targetStrength)
    //     {
    //         SetFlameStrength(targetStrength);
    //         timer = fadeTime;
    //     }

    // }

    // private void SetFlameStrength(float newStrength)
    // {
    //     foreach (Renderer renderer in fireRenderers)
    //     {
    //         renderer.material.SetFloat("_Strength", newStrength);
    //     }

    //     lastStrength = newStrength;
    // }

    /// <summary>
    ///     Put out the fire on interaction
    /// </summary>
    protected override void OnInteract(BlobController blob)
    {
        switch(state)
        {
            case ChestState.LOCKED:
                if (true /*blob has the key*/ )
                {
                    state = ChestState.CLOSED;
                }
                break;
            case ChestState.CLOSED:
                OpenCloseToggle();
                state = empty? ChestState.OPEN_EMPTY : ChestState.OPEN_FULL;
                break;
            case ChestState.OPEN_EMPTY:
                OpenCloseToggle();
                state = ChestState.CLOSED;
                break;
            case ChestState.OPEN_FULL:
                Loot();
                state = ChestState.OPEN_EMPTY;
                break;
        }
        animator.SetBool("chestOpened", opened);
        StartInteractionCooldown(1);
    }

    private void Loot()
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(chestLoot, 0.5f);
        empty = true;
        contents.SetActive(false);
    }

    private void OpenCloseToggle()
    {
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(opened ? chestClose : chestOpen);
        opened = !opened;
    }
}  