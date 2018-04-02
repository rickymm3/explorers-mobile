using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillTargetType { SingleTarget, ScatteredAOE, RandomAOE, AllAOE }
public enum SkillTypes { Offensive, Support, Passive }
public enum SkillTargeting { Enemy, Self, Party, AllParty }
public enum SkillTriggers { Cast, OnDeath, OnBattleStart, OnTakeDamage, OnHit, Active }
// Fire, Water, Nature, Dark, Light
public enum SkillType { None, Melee, Ranged, Ice, Lightning, Earth, Poison, Wind, Healing, Void }

[CreateAssetMenu(menuName = "Skill/New Skill")]
public class Skill : ScriptableObject {
    [Header("Skill")]
    public string Identity = "";

    [Header("Skill Data")]
    public string Name = "";
    public float Weight = 1f;
    public Sprite Icon;
    public int Cooldown = 0;
    public float CastDelay = 0f;
    public string _tooltip = "";
    public string Tooltip(Hero hero) {
        return _tooltip.Replace("{damage}", GetDisplayDamage(hero).ToString() + " damage ");
    }
    public string Tooltip() {
        return _tooltip.Replace("{damage}", "x damage ");
    }

    [HideInInspector] public int _cooldown = 0;

    [Header("Skill Details")]
    public SkillTypes skillType = SkillTypes.Offensive;
    public SkillTargeting skillTargeting = SkillTargeting.Enemy;
    public SkillTriggers skillTrigger = SkillTriggers.Cast;
    public ElementalTypes elementalType = ElementalTypes.None;
    public SkillType elementalSubType = SkillType.Melee;
    public float EndDelay = 0.5f; // Used if we need to delay the skill incase skillEvents don't line up correctly to auto delay
    public float LevelDamageMultiplier = 0f;
    public float DamageMultiplier = 1f;
    public bool canCritical = true;
    public float CriticalMultiplier = 1.5f;
    [Tooltip("This needs to be checked in order to allow the skill to interrupt")] public bool CanInterrupt = false;
    [Tooltip("Set the scale of a turn to delay the target. ex. 0.5 is half a turn, 2 is 2 turns")] public float TurnDelayScale = 0f;
    public bool OneTimeTrigger = false;
    [HideInInspector] public bool triggeredOnce = false;
    
    [Header("Targeting Details")]
    public SkillTargetType TargetType = SkillTargetType.SingleTarget;
    public int TargetCount = 1;
    public HeroClass PreferredTarget = HeroClass.Tank;

    [Header("Skill Timeline")]
    [Inspectionary("Time [Seconds]", "Behaviour")] public SkillTimeLine Timeline = new SkillTimeLine();

    //[HideInInspector] public FloatingCombatTextInterface FCTInterface;
    List<BattleActor> targets = new List<BattleActor>();
    [HideInInspector] public BattleActor actor;
    List<BattleActor> lastTargets = new List<BattleActor>();

    public void Init(BattleActor actor/*, FloatingCombatTextInterface FCTInterface*/) {
        this.actor = actor;
        triggeredOnce = false;
    }
    
    // Manage the timeline, calling this from the heroActor
    public IEnumerator ExecuteSkill(List<BattleActor> targets) {
        Debug.Log("Using Skill: " + Identity + " [" + targets.Count + "]");
        this.targets = targets;

        _cooldown = Cooldown;
        
        float duration = 0f;
        float timer = 0f;
        SkillTimeLine timeline = new SkillTimeLine(); // sort based on time? lowest to highest

        // clone the list
        foreach (float key in Timeline.Keys) {
            timeline.Add(key, Timeline[key]);
        }

        SkillTimeLine eventsToExecute = new SkillTimeLine();

        _cooldown = Cooldown;

        while (timeline.Count > 0 && timer < 120f) {
            foreach (float key in timeline.Keys) {
                if (timer > key) {
                    eventsToExecute.Add(key, timeline[key]);
                } else continue;
            }
            /*foreach (SkillBehaviour e in timeline.Keys) {
                if (timer > timeline[e]) {
                    eventsToExecute.Add(e, timeline[e]);
                } else continue;
            }*/

            if (eventsToExecute.Count > 0)
                foreach (float key in eventsToExecute.Keys) {
                    //Debug.Log("Executing Behaviour: " + eventsToExecute[key].Name);
                    timeline.Remove(key);
                    float d = eventsToExecute[key].Execute(this); // for animations/particle durations so we can delay the last step long enough to execute the animations
                    if (d > duration) duration = d;
                }
            
            eventsToExecute.Clear();
            if (timeline.Count < 0)
                yield return new WaitForSeconds(Mathf.Max(EndDelay, duration));
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
    }
    
    public List<BattleActor> GetTargets() {
        lastTargets.Clear();
        // Change this to smart target select
        switch(TargetType) {
            case SkillTargetType.SingleTarget:
                lastTargets.Add(targets[0]);
                return lastTargets;
            case SkillTargetType.AllAOE:
                lastTargets = targets;
                return lastTargets;
            case SkillTargetType.RandomAOE:
                for (int i = 0; i < TargetCount; i++) {
                    lastTargets.Add(targets[Random.Range(0, targets.Count)]);
                }
                return lastTargets;
            case SkillTargetType.ScatteredAOE:
                BattleActor temp;
                for (int i = 0; i < TargetCount;) {
                    temp = targets[Random.Range(0, targets.Count)];
                    if (!lastTargets.Contains(temp)) {
                        lastTargets.Add(temp);
                        i++;
                    } else if (targets.Count == lastTargets.Count) {
                        lastTargets.Add(temp);
                        i++;
                    }
                }
                return lastTargets;
        }
        return lastTargets;
    }

    public List<BattleActor> GetLastTarget() {
        return lastTargets;
    }

    public BattleActor GetActor() {
        // Return the controlling Actor
        return actor;
    }

    public float GetDamage() {
        float critMultiplier = 1f;
        float damage = 0f;
        if (actor is HeroActor) {
            if (canCritical) {
                float chance = ((HeroActor) actor).hero.GetSecondaryStat(SecondaryStats.CriticalChance) * 0.01f;
                if (Random.Range(0f, 1f) < chance)
                    critMultiplier = CriticalMultiplier + (((HeroActor) actor).hero.GetSecondaryStat(SecondaryStats.CriticalDamage) * 0.01f);
            }
            damage = ((HeroActor) actor).hero.GetCoreStat(CoreStats.Damage) * Mathf.Max((DamageMultiplier + ((HeroActor) actor).hero.GetSkillDamageMultiplier(this) + (LevelDamageMultiplier * ((HeroActor) actor).hero.GetSkillLevel(this))), 0f) * critMultiplier;
            //Debug.Log("[Skill Behaviour] Core[Damage] " + ((HeroActor) actor).hero.GetCoreStat(CoreStats.Damage) + " | DamageMulti: " + Mathf.Min((DamageMultiplier + ((HeroActor) actor).hero.GetSkillDamageMultiplier(this) + LevelDamageMultiplier), 1f) + " | critMulti: " + critMultiplier + " | skill level: " + ((HeroActor) actor).hero.GetSkillLevel(this));
            if (((HeroActor) actor).HasPassive()) {
                Debug.Log("[Passive] > This hero has a passive, if it's a damage passive we should be doing something here");
            }
            return damage;
        } else if (actor is BossActor) {
            damage = ((BossActor) actor).boss.Damage * DamageMultiplier * critMultiplier * ((1f + LevelDamageMultiplier));
            return damage;
        }

        //Debug.LogError("We only should hit this out of combat.");
        return 0f;
    }

    public int GetDisplayDamage(Hero hero) {
        return Mathf.RoundToInt(hero.GetCoreStat(CoreStats.Damage) * Mathf.Max((DamageMultiplier + hero.GetSkillDamageMultiplier(this) + (LevelDamageMultiplier * hero.GetSkillLevel(this))), 0f));
    }

    public Sprite LoadSprite() {
        return Icon;// Resources.Load<Sprite>("Skills/Icons/" + skill.Icon);
    }
    
}
