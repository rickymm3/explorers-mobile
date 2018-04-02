using UnityEngine;
using System.Collections;
using Spine.Unity;

public class TapCharacterHandler : MonoBehaviour {

    SkeletonAnimation Character_Spine_Animator;
    Animator animator;

    CharacterBattleMode mode = CharacterBattleMode.Tap;
    public Hero hero;
    TapBattleController controller = null;

    bool initialized = false;

    float attackTimer = 0f;
    float randomNextAttack = 1f;

    public void Initialize(CharacterBattleMode mode, Hero hero, TapBattleController controller = null) {
        this.mode = mode;
        this.hero = hero;
        this.controller = controller;

        Character_Spine_Animator = GetComponent<SkeletonAnimation>();
        animator = GetComponent<Animator>();
        /*
        GetWeaponBone weaponBone = GetComponent<GetWeaponBone>();

        if (weaponBone != null) {
            Sprite weaponSprite = Resources.Load<Sprite>("Items/Weapon/" + hero.EquipedItems[EquipmentType.Weapon].data.Sprite);
            weaponBone.Initialize(weaponSprite);
        }
        */
        initialized = true;
    }

    void Start() {
        animator = GetComponent<Animator>();
        Character_Spine_Animator = GetComponent<SkeletonAnimation>();
    }

    void Update() {
        if (!initialized) return;

        switch (mode) {
            case CharacterBattleMode.Tap:
                // Attack anim based on hero speed
                attackTimer += Time.deltaTime;

                if (attackTimer > randomNextAttack && controller != null) {
                    if (controller.state == TapBattleState.Battle && controller.Phase == BattlePhases.Battle && AnimationIsPlaying("Idle")) {
                        // Attack Anim
                        Attack();

                        attackTimer = 0f;
                        randomNextAttack = Random.Range(0.9f, 1.5f) + Random.Range(0.9f, 1.5f);
                    }
                }
                break;
            case CharacterBattleMode.Boss:
                break;
        }
    }
    
    public void Attack() { animator.SetTrigger("Attack"); }
    public void Skill() { animator.SetTrigger("Skill"); }
    public void GetHit() { animator.SetTrigger("GetHit"); }
    
    bool AnimationIsPlaying(string animation) {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animation);
    }
}