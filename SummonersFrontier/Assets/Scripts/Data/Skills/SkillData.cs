[System.Serializable]
public class SkillData : BaseData {
    public string Name;
    public float Weight;
    public string Icon;

    // Other stuff for skills such as damage, the type of skill or subclass for each type
    public int Cooldown = 0;

    public SkillData(int ID, string Identity, string Name, float Weight, int Cooldown, string Icon) {
        this.ID = ID;
        this.Identity = Identity;
        this.Name = Name;
        this.Weight = Weight;
        this.Cooldown = Cooldown;
        this.Icon = Icon;
    }
}
