using UnityEngine;

/// <summary>
///     Triggers the win condition when a blob holding the flag interacts with it.
/// </summary>
public class WinCondition : Interactable
{
    /// <summary>
    ///     Audio to play when the win condition is met.
    /// </summary>
    public AudioSource fanfare;
    // TODO: move functionality to menus
    private FinishMenu finishMenu;

    public void Start()
    {
        finishMenu = GameObject.FindGameObjectWithTag("FinishMenu").GetComponent<FinishMenu>();
    }

    /// <summary>
    ///     Win the game if the blob interacts while holding a flag.
    /// </summary>
    protected override void OnInteract(BlobController blob)
    {
        if (blob.HoldingObjectWithTag("Flag"))
        {
            fanfare.Play();
            finishMenu.hasWon = true;
            finishMenu.ShowMenu();
        }
    }
}