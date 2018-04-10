using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossPhaseTriggerType { HealthPercent, TurnsPassed, OnAllyDeath }
public class BossPatternTrigger {
    public BossPhases TargetPhase = BossPhases.Phase1;
    public BossPhases PhaseChange = BossPhases.Phase2;
    public BossPhaseTriggerType Trigger = BossPhaseTriggerType.HealthPercent;
    public string message = "";
    public float Condition = 0f;

    public string MessageOnChange {
        get { return message; }
    }

    public BossPatternTrigger(BossPhases TargetPhase, BossPhases PhaseChange, BossPhaseTriggerType Trigger, float Condition, string message = "") {
        this.TargetPhase = TargetPhase;
        this.PhaseChange = PhaseChange;
        this.Trigger = Trigger;
        this.Condition = Condition;
        this.message = message;
    }

    public override string ToString() {
        return TargetPhase + " changes to " + PhaseChange + " when " + Trigger + " hits " + Condition;
    }
}
