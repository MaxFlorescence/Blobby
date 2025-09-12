using UnityEngine;

public class Hazard : Interactable
{
    private FinishMenu finishMenu;

    public void Start()
    {
        finishMenu = GameObject.FindGameObjectWithTag("FinishMenu").GetComponent<FinishMenu>();
    }

    protected override void OnInteract(BlobController blob)
    {
        finishMenu.hasWon = false;
        finishMenu.ShowMenu();
    }
}