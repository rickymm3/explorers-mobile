using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Skill/New Interrupt Behaviour")]
public class SkillBehaviourSkillInterrupt : SkillBehaviour {

    public float chance = 0f;

    public GameObject InterruptEffectPrefab;
    public Vector3 EffectOffset;

    public override float Execute(Skill skill) {
        // get targets
        List<BattleActor> targets = skill.GetTargets();

        // Damage each target
        foreach (BattleActor target in targets) {
            if (chance < Random.Range(0f, 1f))
                InterruptTarget(target);
        }

        return 0f;
    }
    void InterruptTarget(BattleActor target) {
        // Deal damage and Spawn Damage Effect and FCT text
        target.InterruptSkill();//

        if (target is BossActor)
            SpawnEffect((BossActor) target);
        else if (target is HeroActor)
            SpawnEffect((HeroActor) target);
        
    }

    void SpawnEffect(BossActor target) {
        if (InterruptEffectPrefab == null) return;

        Instantiate(InterruptEffectPrefab, target.handler.transform.position + EffectOffset, Quaternion.identity);
    }

    void SpawnEffect(HeroActor target) {
        if (InterruptEffectPrefab == null) return;

        Instantiate(InterruptEffectPrefab, target.handler.transform.position + EffectOffset, Quaternion.identity);
    }
}
