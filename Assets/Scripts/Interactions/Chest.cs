using UnityEngine;

public enum ChestState
{
    Locked, Closed, OpenFull, OpenEmpty
}

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))]

/// <summary>
///     Controls effects of chest interactions.
/// </summary>
public class Chest : Grip
{
    public ChestState chestState = ChestState.Closed;
    public GameObject contents;

    private Animator animator;
    private AudioClip chestOpen, chestClose, chestLoot;
    private bool opened = false;
    private bool empty = false;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        chestOpen = Resources.Load("Sounds/creak_open", typeof(AudioClip)) as AudioClip;
        chestClose = Resources.Load("Sounds/creak_close", typeof(AudioClip)) as AudioClip;
        chestLoot = Resources.Load("Sounds/coins", typeof(AudioClip)) as AudioClip;
    }

    /// <summary>
    ///     Change chest state on interaction.
    /// </summary>
    protected override void OnInteract(BlobController blob)
    {
        if (blob.IsSticky())
        {
            base.OnInteract(blob);
        }
        else
        {
            switch(chestState)
            {
                case ChestState.Locked:
                    if (true /*blob has the key*/ )
                    {
                        chestState = ChestState.Closed;
                    }
                    break;
                case ChestState.Closed:
                    OpenCloseToggle();
                    chestState = empty? ChestState.OpenEmpty : ChestState.OpenFull;
                    break;
                case ChestState.OpenEmpty:
                    OpenCloseToggle();
                    chestState = ChestState.Closed;
                    break;
                case ChestState.OpenFull:
                    Loot(blob);
                    chestState = ChestState.OpenEmpty;
                    break;
            }
            animator.SetBool("chestOpened", opened);
            StartInteractionCooldown(1, false);
        }
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