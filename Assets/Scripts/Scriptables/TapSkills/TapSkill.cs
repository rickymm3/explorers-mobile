using System;
using System.Collections.Generic;
using UnityEngine;

public enum TapSkillType { DPSBoost, TapDamageBoost, DamageSkill, AutoTap }

[CreateAssetMenu(menuName = "Skill/New Tap Skill")]
public class TapSkill : ScriptableObject {
    [Header("Skill Details")]
    public string Name;
    public string TooltipText;
    public TapSkillType type;
    public ElementalTypes element;
    public string animation;
    public Sprite sprite;
    [Tooltip("In Minutes")] public float cooldown;
    [Tooltip("In Seconds")] public float duration;

    public DateTime lastUsedTapAbility;

    [Header("Damage Details")]
    public float DamageMultiplier = 1f;
    public float TapsPerSecond = 5f;

    public bool isActive() {
        TimeSpan span = DateTime.Now - lastUsedTapAbility;
        if (span.TotalSeconds >= duration)
            return false;

        return true;
    }

    public Sprite LoadSprite() {
        return sprite;
    }
}
