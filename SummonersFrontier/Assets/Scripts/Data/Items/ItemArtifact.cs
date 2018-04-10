using System.Collections.Generic;

[System.Serializable]
public class ItemArtifact : ItemData {
    
    int _skillLevel = 0;
    public int SkillLevel {
        get { return _skillLevel; }
    }
    public ElementalTypes ElementalType = ElementalTypes.None;

    public ItemArtifact(int ID, string Identity, string Name, int Value, string Description, string Sprite, int Tier, float Multiplier, int baseSkillLevel, ElementalTypes ElementalType, List<UniqueReference> UniqueItemReference) : base (ID, Identity, Name, ItemType.Artifact, EquipmentType.Artifact, Value, Description, Sprite, Tier, Multiplier, UniqueItemReference) {
        this._skillLevel = baseSkillLevel;
        this.ElementalType = ElementalType;
    }

    public ItemArtifact(ItemArtifact data) : base(data.ID, data.Name, data.Identity, ItemType.Artifact, EquipmentType.Artifact, data.Value, data.Description, data.Sprite, data.Tier, data.Multiplier, data.UniqueItemReference) {
        this._skillLevel = data.SkillLevel;
        this.ElementalType = data.ElementalType;
    }

    public int GetSkillLevel(float ilvl) {
        return SkillLevel + UnityEngine.Mathf.FloorToInt(Multiplier * ilvl);
    }
}
