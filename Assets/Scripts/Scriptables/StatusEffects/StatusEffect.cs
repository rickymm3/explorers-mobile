using System;
using UnityEngine;
using UnityEngine.UI;

public enum StatusType { Buff, Debuff }
public enum StatusTriggerTime { StartOfTurn, EndOfTurn, ApplyOrRemove, StartOfBattle, OnHit, OnAttack }
public enum StatusEffectType { HealingOverTime, Stun, Shock, Freeze, Burn, DamageOverTime, DefenseEffect, DamageEffect, TurnDelayModification }
// Stun you miss your action on your turn
// Freeze there is a chance you miss your action on your turn, removed by a fire more
// Shock there is a chance you miss your action on your turn, removed by an earth move
// Burn causes damage over time, removed by a water move
// Poison are dots

[Serializable]
public abstract class StatusEffect : ScriptableObject {
    public string Name = "";
    public Sprite StatusIcon = null;
    public StatusType Type = StatusType.Buff;
    public StatusEffectType EffectType = StatusEffectType.DamageEffect;
    public StatusTriggerTime TriggerTime = StatusTriggerTime.StartOfTurn;
    public int Duration = 1;
    public bool Stackable = false;

    public abstract void Apply(BattleActor actor);
    public abstract void Remove(BattleActor actor);
    public abstract bool Trigger(BattleActor actor, StatusTriggerTime trigger);

    public void ReduceDuration() {
        Duration--;
        Debug.Log("Reducing " + Name + " duration to " + Duration);
    }
    public bool IsEffectDone() {
        if (Duration <= 0) return true;
        return false;
    }
}
