public class CrateTypeData : BaseData {
    public string Name = "";
    public CrateChanceDictionary QualityChance = new CrateChanceDictionary();

    public CrateTypeData(int ID, string Identity, string Name, CrateChanceDictionary QualityChance) {
        this.ID = ID;
        this.Identity = Identity;
        this.Name = Name;
        this.QualityChance = QualityChance;
    }
}
