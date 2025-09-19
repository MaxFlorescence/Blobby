using UnityEngine;

/// <summary>
///     Triggers the losing condition when a blob interacts with it.
/// </summary>
public class Hazard : Interactable
{
    private FinishMenu finishMenu;

    public void Start()
    {
        finishMenu = GameObject.FindGameObjectWithTag("FinishMenu").GetComponent<FinishMenu>();
    }

    /// <summary>
    ///     Lose the game if the blob interacts.
    /// </summary>
    protected override void OnInteract(BlobController blob)
    {
        finishMenu.hasWon = false;
        finishMenu.ShowMenu();
    }
}