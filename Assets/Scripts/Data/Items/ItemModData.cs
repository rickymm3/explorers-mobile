using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemModData : BaseData {

    public ItemModType ModType;
    public ItemModEffects StatType;
    public float ModBase;
    public float ScalingFactor;
    public float Variance;
    public StatusTriggerTime StatusTriggerType = StatusTriggerTime.StartOfBattle;
    public StatusEffect Status;

    CoreStats? CoreStat = null;
    PrimaryStats? PrimaryStat = null;
    SecondaryStats? SecondaryStat = null;

    public ItemModData(string Identity, ItemModType ModType, ItemModEffects StatType, float ModBase, float ScalingFactor, float Variance, StatusTriggerTime StatusTriggerType, string Status, CoreStats? CoreStat = null, PrimaryStats? PrimaryStat = null, SecondaryStats? SecondaryStat = null) {
        //Debug.Log("[Mode Loaded] Mod Base: " + ModBase);
        this.Identity = Identity;
        this.ModType = ModType;
        this.StatType = StatType;
        this.ModBase = ModBase;
        this.ScalingFactor = ScalingFactor;
        this.Variance = Variance;
        this.CoreStat = CoreStat;
        this.PrimaryStat = PrimaryStat;
        this.SecondaryStat = SecondaryStat;
        this.StatusTriggerType = StatusTriggerType;
        this.Status = Resources.Load<StatusEffect>("Skills/StatusEffects/" + Status);
    }

    public float GetModEffect(float ilvl, int seed) {
        Random.InitState(seed);
        return ModBase + ((ilvl * ScalingFactor) + (Random.Range(0f, Variance) * ilvl));
    }

    public bool isStat(CoreStats stat) {
        if (CoreStat == null) return false;
        if (CoreStat == stat) return true;
        return false;
    }
    public bool isStat(PrimaryStats stat) {
        if (PrimaryStat == null) return false;
        if (PrimaryStat == stat) return true;
        return false;
    }
    public bool isStat(SecondaryStats stat) {
        if (SecondaryStat == null) return false;
        if (SecondaryStat == stat) return true;
        return false;
    }

    public override string ToString() {
        if (StatType != ItemModEffects.None)
            return Identity + ", " + ModType.ToString() + ", " + StatType.ToString();

        return "Status Effect Details here";
    }
}
