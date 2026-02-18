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
///     Controls effects of chest interactions.
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

    /// <summary>
    ///     Change chest state on interaction.
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
                Loot(blob);
                state = ChestState.OPEN_EMPTY;
                break;
        }
        animator.SetBool("chestOpened", opened);
        StartInteractionCooldown(1);
    }

    /// <summary>
    ///     Add chest loot to blob's inventory.
    /// </summary>
    private void Loot(BlobController blob)
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(chestLoot, 0.5f);
        empty = true;
        contents.SetActive(false);

        // TODO: incorporate with actual inventory system.
        int goldAmount = Random.Range(1, 100);
        GameInfo.AlertSystem.Send(string.Format("Obtained {0} gold", goldAmount));
    }

    /// <summary>
    ///     Toggle chest state between open and closed.
    /// </summary>
    private void OpenCloseToggle()
    {
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(opened ? chestClose : chestOpen);
        opened = !opened;
    }
}  