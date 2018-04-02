using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "StatusEffect/New Effect On Hit")]
public class StatusEffectOnHit : StatusEffect {

    [Header("Status Properties")]
    public StatusEffect Effect = null;

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
