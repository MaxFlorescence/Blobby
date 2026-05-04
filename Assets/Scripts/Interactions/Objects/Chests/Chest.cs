using System.IO;
using UnityEngine;

/// <summary>
///     The different states that a chest can be in.
/// </summary>
public enum ChestState
{
    Locked, Closed, Open_Full, Open_Empty
}

/// <summary>
///     Controls effects of chest interactions.
/// </summary>
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))]
public class Chest : Grip
{
    // ---------------------------------------------------------------------------------------------
    // OPENING & CLOSING
    // ---------------------------------------------------------------------------------------------
    public ChestState chestState = ChestState.Closed;
    /// <summary>
    ///     Animates the chest opening and closing.
    /// </summary>
    private Animator animator;
    /// <summary>
    ///     The audio clip to play when the chest opens.
    /// </summary>
    private AudioClip chestOpen;
    private const string CHEST_OPEN_SOUND = "creak_open";
    /// <summary>
    ///     The audio clip to play when the chest closes.
    /// </summary>
    private AudioClip chestClose;
    private const string CHEST_CLOSE_SOUND = "creak_close";
    /// <summary>
    ///     <tt>True</tt> iff the chest is open.
    /// </summary>
    private bool opened = false;

    // ---------------------------------------------------------------------------------------------
    // CONTENTS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The contents to show/hide depending on the chest's fullness state.
    /// </summary>
    public GameObject contents;
    /// <summary>
    ///     The burden of the chest's contents.
    /// </summary>
    public int contentsBurden = 1;
    /// <summary>
    ///     The audio clip to play when the chest gets looted.
    /// </summary>
    private AudioClip chestLoot;
    private const string CHEST_LOOT_SOUND = "coins";
    /// <summary>
    ///     <tt>True</tt> iff the chest is empty.
    /// </summary>
    private bool empty = false;
    /// <summary>
    ///     The light source that illuminates the chest's contents.
    /// </summary>
    private Light contentsLight;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        chestOpen = Resources.Load<AudioClip>(Path.Combine(FileUtilities.SOUNDS, CHEST_OPEN_SOUND));
        chestClose = Resources.Load<AudioClip>(Path.Combine(FileUtilities.SOUNDS, CHEST_CLOSE_SOUND));
        chestLoot = Resources.Load<AudioClip>(Path.Combine(FileUtilities.SOUNDS, CHEST_LOOT_SOUND));

        contentsLight = contents.GetComponentInChildren<Light>();
    }

    /// <summary>
    ///     Change chest state on interaction.
    /// </summary>
    protected override void OnInteract(BlobController blob)
    {
        if (blob.IsSticky() && chestState != ChestState.Open_Full)
        {
            base.OnInteract(blob);
        }
        else
        {
            switch(chestState)
            {
                case ChestState.Locked:
                    if (true) // TODO: implement chest locking
                    {
                        chestState = ChestState.Closed;
                    }
                    break;
                case ChestState.Closed:
                    OpenCloseToggle();
                    chestState = empty? ChestState.Open_Empty : ChestState.Open_Full;
                    break;
                case ChestState.Open_Full:
                    if (blob.IsSticky()) {
                        Loot(blob);
                        chestState = ChestState.Open_Empty;
                        break;
                    }
                    goto case ChestState.Open_Empty;
                case ChestState.Open_Empty:
                    OpenCloseToggle();
                    chestState = ChestState.Closed;
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
        burden -= contentsBurden;

        // TODO: incorporate with actual inventory system.
        int goldAmount = Random.Range(1, 100);
        GameInfo.AlertSystem.Send(string.Format("Obtained {0} gold", goldAmount));
    }

    /// <summary>
    ///     Illuminates or un-illuminates the chest's contents.
    /// </summary>
    /// <param name="shine">
    ///     If <tt>true</tt>, the chest's contents will be illuminated.
    /// </param>
    public void SetShine(bool shine)
    {
        contentsLight.enabled = shine;
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

    public override string ToString()
    {
        string baseString = base.ToString()[..^1];
        return string.Format("{0}, ChestState: {1})", baseString, chestState.ToString());
    }
}  