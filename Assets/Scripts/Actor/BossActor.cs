using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossActor : BattleActor {
    public BossData boss; // The hero data
    public MonsterHandler handler; // for animations and a reference to which hero need to play which animation
    BossBattleController controller;

    // These are for the status effects
    public float DamageReduction = 1f;
    public float DefenseEffectModifier = 1f;
    public float DamageEffectModifier = 1f;

    public System.Action onHealthChanged;
    public System.Action onStatusChanged;

    public List<StatusEffect> effects = new List<StatusEffect>();

    int TurnCount = 0;

    public BossActor(BossData boss, MonsterHandler handler, FloatingCombatTextInterface FCTInterface, BossBattleController controller) {
        this.boss = boss;
        this.handler = handler;
        this.Health = boss.MaxHealth;
        this.FCTInterface = FCTInterface;
        this.controller = controller;

        this.speed = boss.Speed;
    }

    public override Vector3 GetScreenPosition() {
        return Camera.main.WorldToScreenPoint(this.handler.transform.position - FCTOffset);
    }

    public override Vector3 GetWorldPosition() {
        return this.handler.transform.position;
    }

    public void UpdateTurnCount() {
        TurnCount++;

        BossPatternTrigger Trigger = boss.GetCurrentPhaseTrigger();

        if (Trigger != null) Debug.Log("[TurnTrigger] " + boss.Name + " has trigger: [" + Trigger.ToString() + "]\n");

        if (Trigger != null && Trigger.Trigger == BossPhaseTriggerType.TurnsPassed) {
            if (TurnCount >= Trigger.Condition) {
                boss.BossPhase = Trigger.PhaseChange;
                FCTInterface.SpawnText(Trigger.MessageOnChange, Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f, textColor: Color.blue);
                TurnCount = 0;
                Debug.Log("[TurnTrigger] " + boss.Name + " has triggered and changed from phase {" + Trigger.TargetPhase + "} to phase {" + boss.BossPhase + "}\n");
            }
        }
    }

    public override void Heal(float amount, bool resurrect = false) {
        Debug.Log("Healing " + boss.Name);
        if (Health <= 0 && !resurrect) return;

        float healingScale = 1f;

        /*if (effects.Count > 0) {
            foreach (StatusEffect effect in effects) {
                if (effect is StatusEffectIncreaseHealing)
                    healingScale = ((StatusEffectIncreaseHealing) effect).healingScale;
            }
        }*/

        if (HasPassive()) {
            Debug.Log("[Passive] > This boss has a passive, if it increases healing we should be doing something here");
        }

        Debug.Log("Healing Target " + boss.Name + " for " + amount * healingScale + " [CurrHP: " + Health + "]");

        if (Health < 0) Health = 0;
        Health += amount * healingScale;
        FCTInterface.SpawnText((amount * healingScale).ToString("0"), Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f, textColor: Color.green);

        Debug.Log("Healed [CurrHP: " + Health + "]");

        Health = UnityEngine.Mathf.Min(boss.MaxHealth, Health);
        healingScale = 1f;

        if (onHealthChanged != null) onHealthChanged();
    }

    public override void TakeDamage(float damage) {
        float resolvedDamage = damage * DefenseDamageReduction * DamageReduction;
        Health -= resolvedDamage;
        
        FCTInterface.SpawnText(resolvedDamage.ToString("0"), Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 92f);

        if (Health <= 0f) {
            //dead
            //handler.Die(); // doing this in the battle controller now
            handler.GetHit();

            // TRIGGER OnDeath here
            if (boss.HasPassiveSkill(SkillTriggers.OnDeath)) {
                List<Skill> passives = boss.GetPassiveSkills(SkillTriggers.OnDeath);
                if (passives.Count > 0) {
                    // Add it to the battlecontroller incase multiple actors have died and multiple skills need to be activated
                    foreach (Skill passive in passives) {
                        controller.AddOnDeathSKill(this, passive);
                    }
                }
            }
        } else {
            handler.GetHit();
        }
        Health = UnityEngine.Mathf.Max(0f, Health);

        // Check for HP phase change
        BossPatternTrigger Trigger = boss.GetCurrentPhaseTrigger();

        if (Trigger != null) Debug.Log("[HealthTrigger] " + boss.Name + " has trigger: [" + Trigger.ToString() + "]\n");

        if (Trigger != null && Trigger.Trigger == BossPhaseTriggerType.HealthPercent) {
            float percent = Health / (float)boss.MaxHealth;

            // if the condition is met, change the phase
            if (percent < Trigger.Condition) {
                boss.BossPhase = Trigger.PhaseChange;
                FCTInterface.SpawnText(Trigger.MessageOnChange, Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f, textColor: Color.blue);
                Debug.Log("[HealthTrigger] " + boss.Name + " has triggered and changed from phase {" + Trigger.TargetPhase + "} to phase {" + boss.BossPhase + "}\n");
            }
        }

        if (onHealthChanged != null) onHealthChanged();
    }

    public override bool IsStunned() {
        Debug.Log("Implement the stunned on boss actor");
        return false;
    }

    public float DefenseDamageReduction {
        get { return 1000f / (1000f + (3f * (float) boss.Defense)); }
    }

    public float PlayAnimation(string animation) {
        return handler.PlayAnimation(animation);
    }

    public override string Name {
        get {
            return boss.Name;
        }
    }

    public override GameObject GetGameObject() {
        return handler.gameObject;
    }

    public override Sprite LoadSprite() {
        return Resources.Load<Sprite>("Monster/Portrait/" + boss.Name.Replace(" ", ""));
    }

    public override void TriggerCast() {
        handler.SkillChargingEffect.SetActive(true);
    }
    public override void StopCast() {
        handler.SkillChargingEffect.SetActive(false);
    }

    public override void UpdateStatusEffects(StatusTriggerTime TriggerTime) {
        Debug.Log("Updating Status effects on an effects list " + effects.Count + " long.");
        StatusEffect removeEffect = null;
        foreach (StatusEffect effect in effects) {
            if (effect.Trigger(this, TriggerTime)) {
                effect.ReduceDuration();
                if (effect.IsEffectDone()) {
                    // Remove the effect
                    removeEffect = effect;
                    effect.Remove(this);
                }
            }
        }
        if (removeEffect != null) effects.Remove(removeEffect);
        if (onStatusChanged != null) onStatusChanged();
    }

    public override void ApplyStatusEffect(StatusEffect Effect) {
        if (effects.Find(e => e.Name == Effect.Name) && !Effect.Stackable) {
            effects.Find(e => e.Name == Effect.Name).Duration = Effect.Duration;
            return;
        }
        
        StatusEffect effectInstance = (StatusEffect) UnityEngine.Object.Instantiate(Effect);
        effects.Add(effectInstance);
        effectInstance.Apply(this);

        onStatusChanged();
    }

    public override void OnAllyDeath() {
        BossPatternTrigger Trigger = boss.GetCurrentPhaseTrigger();

        if (Trigger != null) Debug.Log("[OnAllyDeathTrigger] " + boss.Name + " has trigger: [" + Trigger.ToString() + "]\n");

        if (Trigger != null && Trigger.Trigger == BossPhaseTriggerType.OnAllyDeath) {
            boss.BossPhase = Trigger.PhaseChange;
            FCTInterface.SpawnText(Trigger.MessageOnChange, Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f, textColor: Color.blue);
            Debug.Log("[OnAllyDeathTrigger] " + boss.Name + " has triggered and changed from phase {" + Trigger.TargetPhase + "} to phase {" + boss.BossPhase + "}\n");
            
        }
    }

    public bool HasPassive() {
        foreach (Skill skill in boss.Skills) {
            if (skill.skillType == SkillTypes.Passive)
                return true;
        }

        return false;
    }
}
