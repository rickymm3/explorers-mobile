using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "StatusEffect/New Reduce Damage")]
public class StatusEffectReducedDamage : StatusEffect {

    [Header("Reduce Damage Properties")]
    [Tooltip("1.0 = normal damage, 0.5 = 50% damage, 2.0 = 200% damage")][Range(0f, 5f)] public float damageScale = 1f;

    public override void Apply(BattleActor actor) {
        Debug.Log("Applying " + Name + " to " + actor.Name);
        /*if (actor is HeroActor) {
            ((HeroActor) actor).TurnDelay = this;
        }*/
    }

    public override void Remove(BattleActor actor) {
        Debug.Log("Removing " + Name + " from " + actor.Name);
        /*if (actor is HeroActor) {
            ((HeroActor) actor).TurnDelay = null;
        }*/
    }

    public override bool Trigger(BattleActor actor, StatusTriggerTime trigger) {
        if (trigger != TriggerTime) return false;

        // Do nothing, it is handled in the battle controller

        return true;
    }
}
