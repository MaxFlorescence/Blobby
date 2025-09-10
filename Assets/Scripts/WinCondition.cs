using UnityEngine;

public class WinCondition : MonoBehaviour, Interactable
{
    public AudioSource fanfare;
    
    private FinishMenu finishMenu;

    public void Start()
    {
        finishMenu = GameObject.FindGameObjectWithTag("FinishMenu").GetComponent<FinishMenu>();
    }

    public void OnInteract(BlobController blob)
    {
        if (blob.HoldingObjectWithTag("Flag"))
        {
            fanfare.Play();
            finishMenu.hasWon = true;
            finishMenu.ShowMenu();
        }
    }
}