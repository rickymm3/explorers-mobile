using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "StatusEffect/New Stun Effect")]
public class StatusEffectStun : StatusEffect {

    [Header("Stun Properties")]
    [Range(0f, 1f)] public float chance = 1f;
    
    public override void Apply(BattleActor actor) {
        Debug.Log("Applying " + Name + " to " + actor.Name);
    }

    public override void Remove(BattleActor actor) {
        Debug.Log("Removing " + Name + " from " + actor.Name);
    }

    public override bool Trigger(BattleActor actor, StatusTriggerTime trigger) {
        if (TriggerTime != trigger) return false;
        return true;
    }
}
