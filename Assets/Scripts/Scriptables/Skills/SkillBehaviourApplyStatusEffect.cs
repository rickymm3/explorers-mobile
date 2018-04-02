using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Skill/New Status Effect Behaviour")]
public class SkillBehaviourApplyStatusEffect : SkillBehaviour {

    public StatusEffect Effect;
    [Range(0f, 1f)] public float chanceToApply = 1f;

    public override float Execute(Skill skill) {

        // Apply the effect
        List<BattleActor> targets = skill.GetTargets();

        foreach (BattleActor target in targets) {
            if (UnityEngine.Random.Range(0f, 1f) < chanceToApply) {
                target.ApplyStatusEffect(Effect);
            }
        }

        return 0;
    }
}
