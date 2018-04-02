using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HeroData : Entity {
    [SerializeField] HeroClass _class = HeroClass.Assassin;
    [SerializeField] HeroType _type = HeroType.Monster;
    [SerializeField] ElementalTypes _elementalType = ElementalTypes.None;
    [SerializeField] int _strength = 0;
    [SerializeField] int _vitality = 0;
    [SerializeField] int _intelligence = 0;
    [SerializeField] int _speed = 0;
    [SerializeField] int _order = 0;
    [SerializeField] int _rarity = 0;
    [SerializeField] float _strengthScale = 0;
    [SerializeField] float _vitalityScale = 0;
    [SerializeField] float _intelligenceScale = 0;
    [SerializeField] float _speedScale = 0;
    [SerializeField] float _strengthQualityScale = 0;
    [SerializeField] float _vitalityQualityScale = 0;
    [SerializeField] float _intelligenceQualityScale = 0;
    [SerializeField] float _speedQualityScale = 0;
    [SerializeField] float _strengthQualityBase = 0;
    [SerializeField] float _vitalityQualityBase = 0;
    [SerializeField] float _intelligenceQualityBase = 0;
    [SerializeField] float _speedQualityBase = 0;
    [SerializeField] string _defaultWeapon = "";
    [SerializeField] HeroData _awakenReference = null;
    [SerializeField] LeaderSkill _leaderSkill = null;

    public List<Skill> Skills = new List<Skill>();

    public HeroClass Class { get { return _class; } }
    public HeroType Type { get { return _type; } }
    public ElementalTypes ElementalType { get { return _elementalType; } }
    public int Strength { get { return _strength; } }
    public int Vitality { get { return _vitality; } }
    public int Intelligence { get { return _intelligence; } }
    public int Speed { get { return _speed; } }
    public int Order { get { return _order; } }
    public int Rarity { get { return _rarity; } }
    public float StrengthScale { get { return _strengthScale; } }
    public float VitalityScale { get { return _vitalityScale; } }
    public float IntelligenceScale { get { return _intelligenceScale; } }
    public float SpeedScale { get { return _speedScale; } }
    public float StrengthQualityScale { get { return _strengthQualityScale; } }
    public float VitalityQualityScale { get { return _vitalityQualityScale; } }
    public float IntelligenceQualityScale { get { return _intelligenceQualityScale; } }
    public float SpeedQualityScale { get { return _speedQualityScale; } }
    public float StrengthQualityBase { get { return _strengthQualityBase; } }
    public float VitalityQualityBase { get { return _vitalityQualityBase; } }
    public float IntelligenceQualityBase { get { return _intelligenceQualityBase; } }
    public float SpeedQualityBase { get { return _speedQualityBase; } }
    public string DefaultWeapon { get { return _defaultWeapon; } }
    public LeaderSkill LeadershipSkill { get { return _leaderSkill; } }
    public HeroData AwokenReference { get { return _awakenReference; } }
    public ItemData DefaultWeaponItemData {
        get {
            if (_defaultWeapon != "none")
                return dataMan.itemDataList.GetByIdentity(_defaultWeapon);
            else
                return null;
        }
    }
    
    public HeroData(int ID, string Identity, string Name, HeroClass Class, HeroType Type, ElementalTypes elementalType,
        int Strength, int Vitality, int Intelligence, int Speed,
        float StrengthScale, float VitalityScale, float IntelligenceScale, float SpeedScale,
        float StrengthQualityScale, float VitalityQualityScale, float IntelligenceQualityScale, float SpeedQualityScale,
        float StrengthQualityBase, float VitalityQualityBase, float IntelligenceQualityBase, float SpeedQualityBase,
        LeaderSkill leaderSkill, string defaultWeapon, string awakenReference, List<Skill> Skills, int order, int rarity) : base(Identity, Name) {
        this.ID = ID;
        _class = Class;
        _type = Type;
        _elementalType = elementalType;
        _strength = Strength;
        _vitality = Vitality;
        _intelligence = Intelligence;
        _speed = Speed;
        _strengthScale = StrengthScale;
        _vitalityScale = VitalityScale;
        _intelligenceScale = IntelligenceScale;
        _speedScale = SpeedScale;
        _strengthQualityScale = StrengthQualityScale;
        _vitalityQualityScale = VitalityQualityScale;
        _intelligenceQualityScale = IntelligenceQualityScale;
        _speedQualityScale = SpeedQualityScale;
        _strengthQualityBase = StrengthQualityBase;
        _vitalityQualityBase = VitalityQualityBase;
        _intelligenceQualityBase = IntelligenceQualityBase;
        _speedQualityBase = SpeedQualityBase;
        _leaderSkill = leaderSkill;
        _defaultWeapon = defaultWeapon;
        _awakenReference = DataManager.Instance.heroDataList.GetByIdentity(awakenReference);
        _order = order;
        _rarity = rarity;

        this.Skills = Skills;
    }

    string Filepath {
        get { return "Hero/" + Identity.ToLower(); }
    }

    public Sprite LoadPortraitSprite() {
        return Resources.Load<Sprite>(this.Filepath + "/portrait");
    }

    public Sprite LoadBodySprite() {
        return Resources.Load<Sprite>(this.Filepath + "/fullbody");
    }

    public GameObject LoadFightModel(bool instantiate=true) {
        string fightModelPath = this.Filepath + "/FightModel";
        GameObject prefab = Resources.Load<GameObject>(fightModelPath);
        if(prefab==null) {
            Tracer.traceError("Could not load FightModel prefab: " + fightModelPath);
            return null;
        }

        if(instantiate) {
            return GameObject.Instantiate(prefab);
        }
        return prefab;
    }
}
