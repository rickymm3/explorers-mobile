using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroActor : BattleActor {
    public Hero hero; // The hero data
    public CharacterHandler handler; // for animations and a reference to which hero need to play which animation
    BossBattleController controller;

    public List<StatusEffect> effects = new List<StatusEffect>();

    public float ShieldAmount = 0f;
    public float ShieldAmountMax = 0f;

    public StatusEffectReduceTurnDelay TurnDelay = null;

    const float ATTACKER_ENDURE_CHANCE = 0.05f;
    const float ASSASSIN_DODGE_CHANCE = 0.05f;
    const float MAGE_SHIELD_CHANCE = 0.25f;

    public System.Action onStatusChanged = null;

    int _blockBuffCount = 0;

    public HeroActor(Hero hero, CharacterHandler handler, FloatingCombatTextInterface FCTInterface, BossBattleController controller) {
        this.hero = hero;
        this.controller = controller;
        this.hero.EndureTriggered = false;
        this.handler = handler;
        this.Health = Mathf.Round(hero.GetCoreStat(CoreStats.Health) * PlayerManager.Instance.GetBoost(BoostType.Health));
        this.FCTInterface = FCTInterface;

        this.speed = hero.GetPrimaryStat(PrimaryStats.Speed);

        if(this.hero.data.Class == HeroClass.Mage && (Random.Range(0f, 1f) < MAGE_SHIELD_CHANCE)) {
            // They get a shield
            ShieldAmount = ((float) this.hero.GetPrimaryStat(PrimaryStats.Intelligence) * 0.25f);
            ShieldAmountMax = ShieldAmount;
            handler.Shield.SetActive(true);
        }
    }

    public void ApplyStartOfBattleBuffs() {
        foreach(EquipmentType type in hero.EquipedItems.Keys) {
            if (hero.EquipedItems[type] != null) {
                StatusEffect effect = null;
                foreach (ItemAffix affix in hero.EquipedItems[type].Affixes) {
                    effect = null;

                    if (affix.ContainsStatus() && affix.data.data.StatusTriggerType == StatusTriggerTime.StartOfBattle) {
                        effect = affix.data.data.Status;
                        ApplyStatusEffect(effect);
                    }
                }
            }
        }
        
        if (effects.Count > 0) {
            foreach (StatusEffect effect in effects) {
                if (effect is StatusEffectBlockAttacks)
                    _blockBuffCount = ((StatusEffectBlockAttacks) effect).HitsToBlock;
            }
        }
        if (onStatusChanged != null) onStatusChanged();
    }

    public override Vector3 GetScreenPosition() {
        return Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset;
    }

    public override Vector3 GetWorldPosition() {
        return this.handler.transform.position;
    }

    public bool HasPassive() {
        foreach (Skill skill in hero.Skills) {
            if (skill.skillType == SkillTypes.Passive)
                return true;
        }

        return false;
    }

    public Skill GetSkill() {
        foreach (Skill skill in hero.Skills) {
            if (skill.skillType == SkillTypes.Passive)
                return skill;
        }
        return null;
    }

    public override void Heal(float amount, bool resurrect = false) {
        if (Health <= 0 && !resurrect) return;

        float healingScale = 1f;
        
        /*if (effects.Count > 0) {
            foreach (StatusEffect effect in effects) {
                if (effect is StatusEffectIncreaseHealing)
                    healingScale = ((StatusEffectIncreaseHealing) effect).healingScale;
            }
        }*/

        if (HasPassive()) {
            Debug.Log("[Passive] > This hero has a passive, if it increases healing we should be doing something here");
        }

        Health += amount * healingScale;
        FCTInterface.SpawnText((amount * healingScale).ToString("0"), Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f, textColor: Color.green);

        Health = UnityEngine.Mathf.Min(Mathf.RoundToInt(hero.GetCoreStat(CoreStats.Health) * PlayerManager.Instance.GetBoost(BoostType.Health)), Health);
        healingScale = 1f;
    }

    public override void TakeDamage(float damage) {
        float resolvedDamage = damage * hero.GetDamageMitigation * hero.DamageReduction;
        float damageScale = 1f;

        /*foreach (EquipmentType type in hero.EquipedItems.Keys) {
            if (hero.EquipedItems[type] != null) {
                StatusEffect effect = null;
                foreach (ItemAffix affix in hero.EquipedItems[type].Affixes) {
                    effect = null;

                    if (affix.ContainsStatus() && affix.data.data.StatusTriggerType == StatusTriggerTime.OnHit) {
                        effect = affix.data.data.Status;
                        ApplyStatusEffect(effect);
                    }
                }
            }
        }*/

        if (effects.Count > 0) {
            foreach(StatusEffect effect in effects) {
                if (effect is StatusEffectReducedDamage)
                    damageScale = ((StatusEffectReducedDamage) effect).damageScale;

                if (_blockBuffCount > 0) {
                    damageScale = 0f;
                    _blockBuffCount--;
                }
            }
        }

        if (this.hero.data.Class == HeroClass.Assassin && (Random.Range(0f, 1f) < ASSASSIN_DODGE_CHANCE + Mathf.Min(((float) this.hero.GetSecondaryStat(SecondaryStats.Dodge) / 100f), 0.75f))) {
            FCTInterface.SpawnText("dodge", Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f, textColor: Color.green);
            return;
        } else if(Random.Range(0f, 1f) < Mathf.Min(((float)this.hero.GetSecondaryStat(SecondaryStats.Dodge) / 100f), .75f)) {
            FCTInterface.SpawnText("dodge", Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f, textColor: Color.yellow);
            return;
        }

        if (HasPassive()) {
            Debug.Log("[Passive] > This hero has a passive, if it's an active defense or damage reduction passive we should be doing something here");
        }

        if (ShieldAmount > 0f && damage > 0f) {
            if ((ShieldAmount - resolvedDamage) < 0f) {
                resolvedDamage -= ShieldAmount;
                FCTInterface.SpawnText(ShieldAmount.ToString("0"), Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f, textColor: Color.blue);
                ShieldAmount = 0f;

                Health -= resolvedDamage * damageScale;
                handler.Shield.SetActive(false);
                // TODO print FTC text from here
                FCTInterface.SpawnText(resolvedDamage.ToString("0"), Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f);
            } else {
                ShieldAmount -= resolvedDamage;
                FCTInterface.SpawnText((resolvedDamage * damageScale).ToString("0"), Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f, textColor: Color.blue);
                resolvedDamage = 0f;
            }
        } else {
            Health -= resolvedDamage * damageScale;
            
            FCTInterface.SpawnText((resolvedDamage * damageScale).ToString("0"), Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f);
        }

        //Debug.Log(Name + " Took " + resolvedDamage + " damage and has [" + Health + " / " + hero.GetCoreStat(CoreStats.Health) + "] remaining");

        if (Health <= 1f) {
            //dead
            //handler.Die();
            handler.GetHit();

            if (this.hero.data.Class == HeroClass.Bruiser && (Random.Range(0f, 1f) < ATTACKER_ENDURE_CHANCE) && !this.hero.EndureTriggered) {
                this.hero.EndureTriggered = true;
                FCTInterface.SpawnText("Endure", Camera.main.WorldToScreenPoint(this.handler.transform.position) + FCTOffset, 72f, textColor: Color.green);
                Health = 1f;
                handler.GetHit();
            } else {
                Debug.Log(Name + " died!");
                //handler.Die();

                // TRIGGER OnDeath here
                if (hero.HasPassiveSkill(SkillTriggers.OnDeath)) {
                    List<Skill> passives = hero.GetPassiveSkills(SkillTriggers.OnDeath);
                    if (passives.Count > 0) {
                        // Add it to the battlecontroller incase multiple actors have died and multiple skills need to be activated
                        foreach (Skill passive in passives) {
                            controller.AddOnDeathSKill(this, passive);
                        }
                    }
                }
            }
        } else {
            if (Health > Mathf.RoundToInt(hero.GetCoreStat(CoreStats.Health) * PlayerManager.Instance.GetBoost(BoostType.Health)))
                Health = Mathf.RoundToInt(hero.GetCoreStat(CoreStats.Health) * PlayerManager.Instance.GetBoost(BoostType.Health));
            handler.GetHit();
        }
        Health = UnityEngine.Mathf.Max(0f, Health);
        //Debug.Log(Name + " has [" + Health + " / " + hero.GetCoreStat(CoreStats.Health) + "] remaining at the end of TakeDamage");
        damageScale = 1f;
    }

    public override void UpdateStatusEffects(StatusTriggerTime TriggerTime) {
        //Debug.Log("Updating Status effects on an effects list " + effects.Count + " long.");
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
    
    public override bool IsStunned() {
        foreach (StatusEffect effect in effects) {
            if (effect is StatusEffectStun) {
                if (Random.Range(0f, 1f) <= ((StatusEffectStun) effect).chance)
                    return true;
            }
        }
        return false;
    }


    public override bool HasOnHitStatusEffectOnGear() {
        if (effects.Count > 0)
            foreach (StatusEffect effect in effects) {
                if (effect is StatusEffectOnHit)
                    return true;
            }

        return false;
    }

    public override List<StatusEffect> GetOnHitStatusEffects() {
        List<StatusEffect> listOfEffects = new List<StatusEffect>();

        foreach (StatusEffect effect in effects) {
            listOfEffects.Add(effect);
        }
        
        return listOfEffects;
    }

    public override void ApplyStatusEffect(StatusEffect Effect) {
        if (effects.Find(e => e.Name == Effect.Name) && !Effect.Stackable) {
            effects.Find(e => e.Name == Effect.Name).Duration = Effect.Duration;
            return;
        }

        StatusEffect effectInstance = (StatusEffect) UnityEngine.Object.Instantiate(Effect);
        effects.Add(effectInstance);
        effectInstance.Apply(this);

        if (onStatusChanged != null) onStatusChanged();
    }

    public float PlayAnimation(string animation) {
        return handler.PlayAnimation(animation);
    }

    public override string Name {
        get {
            return hero.data.Name;
        }
    }

    public override GameObject GetGameObject() {
        return handler.gameObject;
    }

    public override Sprite LoadSprite() {
        return Resources.Load<Sprite>("Hero/" + hero.data.Identity.ToLower() + "/portrait");
    }

    public override void TriggerCast() {
        handler.SkillChargingEffect.SetActive(true);
    }
    public override void StopCast() {
        handler.SkillChargingEffect.SetActive(false);
    }
}
