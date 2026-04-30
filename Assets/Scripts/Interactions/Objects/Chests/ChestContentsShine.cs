using UnityEngine;

/// <summary>
///     A class for updating a chest's light component depending on its amination state.
/// </summary>
public class ChestContentsShine : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.gameObject.GetComponent<Chest>().SetShine(stateInfo.IsName("Opened"));
    }
   
}