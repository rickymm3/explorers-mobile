using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LeadershipSkillType { CoreStatBoost, PrimaryStatBoost, SecondaryStatBoost}

[CreateAssetMenu(menuName = "Skill/New Leader Skill")]
public class LeaderSkill : ScriptableObject {
    public string Name = "";
    public string Tooltip = "";
    public Sprite sprite;

    public LeadershipSkillType type = LeadershipSkillType.CoreStatBoost;
    [Range(0f, 1f)] public float percent = 0.1f;

    public CoreStats coreStat = CoreStats.Damage;
    public PrimaryStats primaryStat = PrimaryStats.Strength;
    public SecondaryStats secondaryStat = SecondaryStats.CriticalChance;

    public float Multiplier {
        get { return 1f + percent; }
    }

    public Sprite LoadSprite() {
        return sprite;
    }
}
