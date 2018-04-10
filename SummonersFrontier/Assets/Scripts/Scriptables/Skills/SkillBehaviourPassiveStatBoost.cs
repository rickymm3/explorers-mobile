using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class SkillBehaviourPassiveStatBoost : SkillBehaviour {

    public string Stat;
    public int Value;

    public override float Execute(Skill skill) {
        return 0f;
    }

    //public float GetStat(CoreStats stat) {
    //    if (stat == Stat.AsEnum())
    //}
}
