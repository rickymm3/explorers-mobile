using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "StatusEffect/New Reduce Turn Delay")]
public class StatusEffectReduceTurnDelay : StatusEffect {

    [Header("Delay Turn Properties")]
    [Range(0f, 1f)] public float chance = 0f;
    [Range(0f, 1f)] public float delayWeight = 0.1f;

    public override void Apply(BattleActor actor) {
        Debug.Log("Applying " + Name + " to " + actor.Name);
        if (actor is HeroActor) {
            ((HeroActor) actor).TurnDelay = this;
        }
    }

    public override void Remove(BattleActor actor) {
        Debug.Log("Removing " + Name + " from " + actor.Name);
        if (actor is HeroActor) {
            ((HeroActor) actor).TurnDelay = null;
        }
    }

    public override bool Trigger(BattleActor actor, StatusTriggerTime trigger) {
        if (trigger != TriggerTime) return false;

        // Do nothing, it is handled in the battle controller

        return true;
    }
}
