using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "StatusEffect/New Damage Reduction Effect")]
public class StatusEffectDamageReduction : StatusEffect {

    [Header("Defense Properties")]
    [Range(0f, 1f)] public float damageReductionAmount = 0.5f; // 0.25f will reduce incoming damage by 25%

    public override void Apply(BattleActor actor) {
        Debug.Log("Applying " + Name + " to " + actor.Name);
        if (actor is HeroActor) {
            // Do the status effect here
            ((HeroActor) actor).hero.DamageReduction = damageReductionAmount;
        } else if (actor is BossActor) {
            ((BossActor) actor).DamageReduction = damageReductionAmount;
        }
    }

    public override void Remove(BattleActor actor) {
        Debug.Log("Removing " + Name + " from " + actor.Name);
        if (actor is HeroActor) {
            // Do the status effect here
            ((HeroActor) actor).hero.DamageReduction = 1f;
        } else if (actor is BossActor) {
            ((BossActor) actor).DamageReduction = 1f;
        }
    }

    public override bool Trigger(BattleActor actor, StatusTriggerTime trigger) {
        if (trigger != TriggerTime) return false;

        // Do nothing for defense in the updateTrigger

        return true;
    }
}
