using UnityEngine;

public class ChestContentsShine : StateMachineBehaviour{

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.gameObject.GetComponent<Chest>().SetShine(stateInfo.IsName("Opened"));
    }
   
}