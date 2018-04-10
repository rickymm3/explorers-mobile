using UnityEngine;
using System.Collections;
using Spine.Unity;
using DG.Tweening;

public class CharacterHandler : MonoBehaviour {

    SkeletonAnimation Character_Spine_Animator;
    Animator animator;
    public GameObject Shield;
    public GameObject SkillChargingEffect;

    public Hero hero;

    public void Initialize(Hero hero) {
        this.hero = hero;

        Character_Spine_Animator = GetComponent<SkeletonAnimation>();
        animator = GetComponent<Animator>();
        /*
        GetWeaponBone weaponBone = GetComponent<GetWeaponBone>();
        if (weaponBone != null) {
            Sprite weaponSprite = Resources.Load<Sprite>("Items/Weapon/" + hero.EquipedItems[EquipmentType.Weapon].data.Sprite);
            weaponBone.Initialize(weaponSprite);
        }
        */
        SkillChargingEffect = Instantiate(Resources.Load<GameObject>("Effects/skill-charge"));
        SkillChargingEffect.transform.SetParent(this.transform);
        SkillChargingEffect.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        SkillChargingEffect.SetActive(false);
    }

    void Start() {
        animator = GetComponent<Animator>();
        Character_Spine_Animator = GetComponent<SkeletonAnimation>();
    }

    /*
    public void Attack() { animator.SetTrigger("Attack"); }
    public void Skill() { animator.SetTrigger("Skill"); }
    public void GetHit() { animator.SetTrigger("GetHit"); }
    public void Die() { animator.SetTrigger("Die"); }
    */

    public void Attack() { transform.DOPunchPosition(new Vector3(0.5f, 0f, 0f), 0.25f, 0); }
    public void Skill() { Attack(); }
    public void GetHit() { transform.DOPunchPosition(new Vector3(-0.25f, 0f, 0f), 0.15f, 0); }
    public void Die() {
        GetHit();
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