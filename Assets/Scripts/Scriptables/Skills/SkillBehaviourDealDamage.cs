using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Skill/New Damage Behaviour")]
public class SkillBehaviourDealDamage : SkillBehaviour {
    // Effect to spawn with the damage
    public GameObject DamageEffectPrefab;
    public Vector3 EffectOffset;
    // Sound Effect to play
    public SFX_Attacks SFX;

    public override float Execute(Skill skill) {
        // get targets
        List<BattleActor> targets = skill.GetTargets();

        // Damage each target
        foreach (BattleActor target in targets) {
            DamageTarget(target, skill);

            if (skill.GetActor() is HeroActor && skill.GetActor().HasOnHitStatusEffectOnGear()) {
                List<StatusEffect> effects = skill.GetActor().GetOnHitStatusEffects();

                foreach(StatusEffect effect in effects) {
                    target.ApplyStatusEffect(effect);
                }
            }

        }

        return 0f;
    }

    void DamageTarget(BattleActor target, Skill skill) {

        // Play hit sound effect
        //AudioManager.Instance.Play(SFX);

        // Deal damage and Spawn Damage Effect and FCT text
        Debug.Log("[Skill Behaviour] Damage: " + skill.GetDamage());
        target.TakeDamage(skill.GetDamage());

        if (target is BossActor) {
            SpawnEffect((BossActor) target);
            //skill.FCTInterface.SpawnText(skill.GetDamage().ToString("0"), Camera.main.WorldToScreenPoint(((BossActor) target).handler.transform.position - FTCOffset), 92f);
        } else if (target is HeroActor) {
            SpawnEffect((HeroActor) target);
            //skill.FCTInterface.SpawnText(skill.GetDamage().ToString("0"), Camera.main.WorldToScreenPoint(((HeroActor) target).handler.transform.position - FTCOffset), 92f);
        }
    }

    void SpawnEffect(BossActor target) {
        if (DamageEffectPrefab == null) return;

        Instantiate(DamageEffectPrefab, target.handler.transform.position + EffectOffset, Quaternion.identity);
    }

    void SpawnEffect(HeroActor target) {
        if (DamageEffectPrefab == null) return;

        Instantiate(DamageEffectPrefab, target.handler.transform.position + EffectOffset, Quaternion.identity);
    }
}
