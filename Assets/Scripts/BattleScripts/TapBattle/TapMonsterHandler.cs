using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapMonsterHandler : MonoBehaviour {

    public Animator animator;
    
    void Start () {
        animator = GetComponent<Animator>();
    }
	
    public void Attack() { animator.SetTrigger("Attack"); }
    public void Die() { animator.SetTrigger("Die"); }
    public void GetHit() { animator.SetTrigger("GetHit"); }

    bool AnimationIsPlaying(string animation) {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animation);
    }
}
