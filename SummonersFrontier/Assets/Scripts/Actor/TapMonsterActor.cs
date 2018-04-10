using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapMonsterActor : MonoBehaviour {

    public MonsterData monster;
    public bool isShielded = false;

    public float TotalHealth = 1000f;
    public float Health = 1000f;

    public float DamageReduction = 0f;

    System.Action onDeath = null;

    TapMonsterHandler handler;

    bool dead = false;

    public void BreakShieldCheck(TapSkill skill) {
        if (isShielded && monster.TapType == TapMonsterType.Unique) {
            // if the shield is of an opposite type then break it
            if (GameManager.IsElementACounter(skill.element, monster.Element)) {
                isShielded = false;
            }
        }
    }


    public void Initialize(MonsterData data, System.Action onDeath = null) {
        monster = data;
        handler = GetComponent<TapMonsterHandler>();
        Health = PlayerManager.Instance.SelectedBattle.Zone.ZoneHP / DataManager.Instance.globalData.GetGlobalAsInt(GlobalProps.ZONE_MONSTER_COUNT);

        DamageReduction = 0f;
        if (monster.TapType == TapMonsterType.Timed)
            Health = Health * 2f;
        else if (monster.TapType == TapMonsterType.Unique)
            DamageReduction = 0.8f;

        TotalHealth = Health;

        this.onDeath += onDeath;
        
        isShielded = (monster.TapType == TapMonsterType.Unique);
    }

    public float DealDPSDamage(float damage) {
        if (dead) return 0f;

        float dps = damage;
        // no Anim for this
        if (isShielded)
            dps = damage * (1f - DamageReduction);
        else if (monster.TapType == TapMonsterType.Timed)
            dps = damage * (1f - DamageReduction);
        else
            dps = damage;
        Health -= dps;

        if (Health <= 0f) Death();

        Health = (Mathf.Max(0f, Health));

        return dps;
    }
    
    public float GetHit(float damage) {
        if (dead) return 0f;

        float dmg = damage;
        if (isShielded)
            dmg = damage * (1f - DamageReduction);
        else if (monster.TapType == TapMonsterType.Timed)
            dmg = damage * (1f - DamageReduction);
        else
            dmg = damage;
        Health -= dmg;

        if (Health <= 0f) Death();

        Health = (Mathf.Max(0f, Health));

        // play sfx here
        handler.GetHit();

        return dmg;
    }
    public void Death() {
        dead = true;
        if (onDeath != null) onDeath();
        // play sfx here
        handler.Die();
    }
    public void Attack() {
        // play sfx here
        handler.Attack();
    }
}
