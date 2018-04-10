using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "StatusEffect/New Block Attacks")]
public class StatusEffectBlockAttacks : StatusEffect {

    [Header("Block Properties")]
    public int HitsToBlock = 1;

    public override void Apply(BattleActor actor) {
        Debug.Log("Applying " + Name + " to " + actor.Name);
    }

    public override void Remove(BattleActor actor) {
        Debug.Log("Removing " + Name + " from " + actor.Name);
    }

    public override bool Trigger(BattleActor actor, StatusTriggerTime trigger) {
        if (trigger != TriggerTime) return false;

        // Do nothing, it is handled in the battle controller
        return true;
    }
}
