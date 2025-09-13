using UnityEngine;

public class WinCondition : Interactable
{
    public AudioSource fanfare;
    
    private FinishMenu finishMenu;

    public void Start()
    {
        finishMenu = GameObject.FindGameObjectWithTag("FinishMenu").GetComponent<FinishMenu>();
    }

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