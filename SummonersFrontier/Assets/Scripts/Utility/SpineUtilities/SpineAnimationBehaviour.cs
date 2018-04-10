using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SpineAnimationBehaviour : StateMachineBehaviour {

    public int trackIndex = 0;
    public string animation = "";
    public bool looping = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        CharacterHandler chandler = animator.gameObject.GetComponent<CharacterHandler>();
        MonsterHandler mhandler = animator.gameObject.GetComponent<MonsterHandler>();


        switch (animation) {
            case "Idle":
                animator.GetComponent<SkeletonAnimation>().state.SetAnimation(trackIndex, animation, looping);
                animator.GetComponent<SkeletonAnimation>().timeScale = 0.7f;
                break;
            case "Attack":
            case "Skill":
            case "Skill2":
                //Debug.Log("Offensive Anim on " + animator.gameObject.name);
                if (chandler != null)
                    chandler.Attack();
                else if(mhandler != null)
                    mhandler.Attack();
                break;
            case "GetHit":
            case "Die":
                //Debug.Log("Hit Anim on " + animator.gameObject.name);
                if (chandler != null)
                    chandler.Die();
                else if (mhandler != null)
                    mhandler.Die();
                break;
            default:
                //Debug.Log("State not handled: " + animation);
                break;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
