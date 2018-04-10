using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Skill/New VFX Behaviour")]
public class SkillBehaviourSpawnEffect : SkillBehaviour {

    public GameObject EffectPrefab;
    public Vector3 EffectOffset;

    public override float Execute(Skill skill) {
        BattleActor actor = skill.GetActor();

        if (actor is BossActor) {
            SpawnEffect((BossActor) actor);
        } else if (actor is HeroActor) {
            SpawnEffect((HeroActor) actor);
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
