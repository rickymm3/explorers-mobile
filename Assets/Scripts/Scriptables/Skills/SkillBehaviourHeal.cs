using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Skill/New Heal Behaviour")]
public class SkillBehaviourHeal : SkillBehaviour {
    // Effect to spawn with the damage
    public bool CanResurrect = false;
    public GameObject HealEffectPrefab;
    public Vector3 EffectOffset;
    // Sound Effect to play
    public SFX_Attacks SFX; // should move this out as a behaviour

    public override float Execute(Skill skill) {
        // get targets
        List<BattleActor> targets = skill.GetTargets();

        // Damage each target
        foreach (BattleActor target in targets) {
            HealTarget(target, skill);

            if (skill.GetActor() is HeroActor && skill.GetActor().HasOnHitStatusEffectOnGear()) {
                List<StatusEffect> effects = skill.GetActor().GetOnHitStatusEffects();

                foreach(StatusEffect effect in effects) {
                    target.ApplyStatusEffect(effect);
                }
            }

        }

        return 0f;
    }

    void HealTarget(BattleActor target, Skill skill) {
        Debug.Log("Healing Target " + target.Name + " with " + skill.Name);
        // Play hit sound effect
        //AudioManager.Instance.Play(SFX);

        // Deal damage and Spawn Damage Effect and FCT text
        Debug.Log("[Skill Behaviour] Heal: " + skill.GetDamage());
        target.Heal(skill.GetDamage(), CanResurrect);

        if (target is BossActor) {
            SpawnEffect((BossActor) target);
            //skill.FCTInterface.SpawnText(skill.GetDamage().ToString("0"), Camera.main.WorldToScreenPoint(((BossActor) target).handler.transform.position - FTCOffset), 92f);
        } else if (target is HeroActor) {
            SpawnEffect((HeroActor) target);
            //skill.FCTInterface.SpawnText(skill.GetDamage().ToString("0"), Camera.main.WorldToScreenPoint(((HeroActor) target).handler.transform.position - FTCOffset), 92f);
        }
    }

    void SpawnEffect(BossActor target) {
        if (HealEffectPrefab == null) return;

        Instantiate(HealEffectPrefab, target.handler.transform.position + EffectOffset, Quaternion.identity);
    }

    void SpawnEffect(HeroActor target) {
        if (HealEffectPrefab == null) return;

        Instantiate(HealEffectPrefab, target.handler.transform.position + EffectOffset, Quaternion.identity);
    }
}
