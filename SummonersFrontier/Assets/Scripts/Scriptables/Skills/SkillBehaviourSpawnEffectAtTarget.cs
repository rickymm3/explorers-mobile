using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Skill/New VFX Target Behaviour")]
public class SkillBehaviourSpawnEffectAtTarget : SkillBehaviour {

    public GameObject EffectPrefab;
    public Vector3 EffectOffset;

    public override float Execute(Skill skill) {
        List <BattleActor> targets = skill.GetTargets();

        foreach (BattleActor target in targets) {
            if (target is BossActor) {
                SpawnEffect((BossActor) target);
            } else if (target is HeroActor) {
                SpawnEffect((HeroActor) target);
            }
        }

        return 0;
    }

    void SpawnEffect(BossActor target) {
        if (EffectPrefab == null) return;

        Instantiate(EffectPrefab, target.handler.transform.position + EffectOffset, Quaternion.identity);
    }

    void SpawnEffect(HeroActor target) {
        if (EffectPrefab == null) return;

        Instantiate(EffectPrefab, target.handler.transform.position + EffectOffset, Quaternion.identity);
    }
}
