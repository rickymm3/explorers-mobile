using System.Collections.Generic;
using UnityEngine;

public enum BossPhases { Phase1, Phase2, Phase3, Phase4, Phase5 }

[System.Serializable]
public class BossData : Entity {
    [SerializeField]
    int _damage = 100;
    public int Damage {
        get { return _damage; }
    }
    [SerializeField]
    int _defense = 100;
    public int Defense {
        get { return _defense; }
    }

    /*[SerializeField]
    int _strength = 0;
    public int Strength {
        get { return _strength; }
    }
    [SerializeField]
    int _vitality = 0;
    public int Vitality {
        get { return _vitality; }
    }
    [SerializeField]
    int _intelligence = 0;
    public int Intelligence {
        get { return _intelligence; }
    }*/
    [SerializeField]
    int _speed = 0;
    public int Speed {
        get { return _speed; }
    }

    public ElementalTypes elementalType;
    public string Sprite;
    [Inspectionary] public BossPhaseSkillList Phases = new BossPhaseSkillList();
    //public List<Skill> Skills = new List<Skill>();
    public List<Skill> Skills {
        get { return Phases[BossPhase]; }
    }
    public BossPhases BossPhase = BossPhases.Phase1;

    public List<BossPatternTrigger> PhaseTriggers = new List<BossPatternTrigger>();

    public BossData(string Identity, string name, int Health, int Damage, int Defense, int Speed, ElementalTypes EType, string Sprite, /*List<Skill> Skills,*/ BossPhaseSkillList PhaseSkillList, List<BossPatternTrigger> PhaseTriggers) : base(Identity, name, Health) {
        //moved name, maxhealth and health to Entity ("health = maxHealth" in Entity too).
        _damage = Damage;
        _defense = Defense;
        _speed = Speed;
        elementalType = EType;
        this.Sprite = Sprite;

        Phases = PhaseSkillList;
        this.PhaseTriggers = PhaseTriggers;

        string triggerText = "";
        foreach(BossPatternTrigger trig in this.PhaseTriggers) {
            triggerText += trig.ToString() + "\n";
        }

        //Debug.Log("Adding Phase Triggers to " + name + "\n" + (triggerText.Length < 1 ? "None" : triggerText));
    }

    public Sprite LoadPortrait() {
        return Resources.Load<Sprite>("Monster/Portrait/" + Name.Replace(" ", ""));
    }

    public BossPatternTrigger GetCurrentPhaseTrigger() {
        if (PhaseTriggers.Count <= 0) return null;
        
        foreach(BossPatternTrigger trigger in PhaseTriggers) {
            if (trigger.TargetPhase == BossPhase)
                return trigger;
        }

        return null;
    }

    public bool HasPassiveSkill() {
        foreach (Skill skill in Skills) {
            if (skill.skillType == SkillTypes.Passive)
                return true;
        }
        return false;
    }
    public bool HasPassiveSkill(SkillTriggers trigger) {
        foreach (Skill skill in Skills) {
            if (skill.skillType == SkillTypes.Passive && skill.skillTrigger == trigger)
                return true;
        }
        return false;
    }
    public List<Skill> GetPassiveSkills() {
        List<Skill> passives = new List<Skill>();

        foreach (Skill skill in Skills) {
            if (skill.skillType == SkillTypes.Passive)
                passives.Add(skill);
        }

        return passives;
    }
    public List<Skill> GetPassiveSkills(SkillTriggers trigger) {
        List<Skill> passives = new List<Skill>();

        foreach (Skill skill in Skills) {
            if (skill.skillType == SkillTypes.Passive && skill.skillTrigger == trigger)
                passives.Add(skill);
        }

        return passives;
    }
}
