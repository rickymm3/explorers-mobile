using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using DG.Tweening;

public class MonsterHandler : MonoBehaviour {

    SkeletonAnimation Character_Spine_Animator;
    public Animator animator;
    public GameObject SkillChargingEffect = null;

    void Start () {
        Character_Spine_Animator = GetComponent<SkeletonAnimation>();
        animator = GetComponent<Animator>();

        SkillChargingEffect = Instantiate(Resources.Load<GameObject>("Effects/skill-charge"));
        SkillChargingEffect.transform.SetParent(this.transform);
        SkillChargingEffect.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        SkillChargingEffect.transform.localScale = Vector3.one;
        SkillChargingEffect.SetActive(false);
    }

    /*public void Attack() { animator.SetTrigger("Attack"); }
    public void Die() { animator.SetTrigger("Die"); }
    public void GetHit() { animator.SetTrigger("GetHit"); }*/

    public void Attack() { transform.DOPunchPosition(new Vector3(-0.5f, 0f, 0f), 0.25f, 0); }
    public void Skill() { Attack(); }
    public void GetHit() { transform.DOPunchPosition(new Vector3(0.25f, 0f, 0f), 0.15f, 0); }
    public void Die() {
        FadeOutSprite();
    }

    bool AnimationIsPlaying(string animation) {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animation);
    }

    public float PlayAnimation(string animation) {
        animator.SetTrigger(animation);
        return 0f; // TODO get the animation time
    }

    [ContextMenu("TestOpacity")]
    public void FadeOutSprite() {
        DOTween.To(() => Character_Spine_Animator.skeleton.a, a => Character_Spine_Animator.skeleton.a = a, 0f, 1f);
    }
}
