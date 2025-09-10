using UnityEngine;

public class Hazard : MonoBehaviour, Interactable
{
    private FinishMenu finishMenu;

    public void Start()
    {
        finishMenu = GameObject.FindGameObjectWithTag("FinishMenu").GetComponent<FinishMenu>();
    }

    public void OnInteract(BlobController blob)
    {
        finishMenu.hasWon = false;
        finishMenu.ShowMenu();
    }
}