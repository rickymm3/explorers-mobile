using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Skill/New Turn Delay Behaviour")]
public class SkillBehaviourTurnDelay : SkillBehaviour {

    public float chance = 1f;

    public GameObject DelayEffectPrefab;
    public Vector3 EffectOffset;
    public bool RequiredOpposingElement;

    public override float Execute(Skill skill) {
        // get targets
        List<BattleActor> targets = skill.GetTargets();

        // Damage each target
        foreach (BattleActor target in targets) {
            if (Random.Range(0f, 1f) < chance) {
                if (RequiredOpposingElement) {
                    if (target is HeroActor) {
                        if (GameManager.IsElementACounter(skill.elementalType, ((HeroActor) target).hero.data.ElementalType)) {
                            DelayTargetTurn(target, skill);
                        }
                    } else if (target is BossActor) {
                        if (GameManager.IsElementACounter(skill.elementalType, ((BossActor) target).boss.elementalType)) {
                            DelayTargetTurn(target, skill);
                        }
                    }

                } else {
                    DelayTargetTurn(target, skill);
                }
            }
        }

        return 0f;
    }
    void DelayTargetTurn(BattleActor target, Skill skill) {
        target.CurrentSpeedOffset += skill.TurnDelayScale * target.AverageSpeed;

        if (target is BossActor)
            SpawnEffect((BossActor) target);
        else if (target is HeroActor)
            SpawnEffect((HeroActor) target);
        
    }

    void SpawnEffect(BossActor target) {
        if (DelayEffectPrefab == null) return;

        Instantiate(DelayEffectPrefab, target.handler.transform.position + EffectOffset, Quaternion.identity);
    }

    void SpawnEffect(HeroActor target) {
        if (DelayEffectPrefab == null) return;

        Instantiate(DelayEffectPrefab, target.handler.transform.position + EffectOffset, Quaternion.identity);
    }
}
