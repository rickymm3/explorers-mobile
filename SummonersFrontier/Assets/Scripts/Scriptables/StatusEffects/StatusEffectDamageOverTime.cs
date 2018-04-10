using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "StatusEffect/New Damage Over Time Effect")]
public class StatusEffectDamageOverTime : StatusEffect {
    [Header("Damage Over Time Properties")]
    public float damage = 50f;

    public override void Apply(BattleActor actor) {
        Debug.Log("Applying " + Name + " to " + actor.Name);
        actor.FCTInterface.SpawnText("Poison", actor.GetScreenPosition(), textColor: Color.green);
    }

    public override void Remove(BattleActor actor) {
        Debug.Log("Removing " + Name + " from " + actor.Name);
    }

    public override bool Trigger(BattleActor actor, StatusTriggerTime trigger) {
        if (trigger != TriggerTime) return false;
        
        if (actor is HeroActor) {
            ((HeroActor) actor).TakeDamage(damage);
        } else if (actor is BossActor) {
            ((BossActor) actor).TakeDamage(damage);
        }
        return true;
    }
}
