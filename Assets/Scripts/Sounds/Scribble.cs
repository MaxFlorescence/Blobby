using UnityEngine;

/// <summary>
///     A class for playing an experimenter's scribble noise depending on its amination state.
/// </summary>
public class Scribble : StateMachineBehaviour{

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.gameObject.GetComponent<AudioSource>().Play();
    }
   
}