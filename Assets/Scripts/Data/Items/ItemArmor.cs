using System.Collections.Generic;

[System.Serializable]
public class ItemArmor : ItemData {
    
    int _defense = 0;
    public int Defense {
        get { return _defense; }
    }

    public ItemArmor(int ID, string Identity, string Name, int Value, string Description, EquipmentType EquipType, int Defense, string Sprite, int Tier, float Multiplier, List<UniqueReference> UniqueItemReference) : base(ID, Identity, Name, ItemType.Armor, EquipType, Value, Description, Sprite, Tier, Multiplier, UniqueItemReference) {
        _defense = Defense;
    }

    public ItemArmor(ItemArmor data) : base(data.ID, data.Identity, data.Name, ItemType.Armor, data.EquipType, data.Value, data.Description, data.Sprite, data.Tier, data.Multiplier, data.UniqueItemReference) {
        _defense = data.Defense;
    }

    public override int GetStatValue(CoreStats stat, float ilvl) {
        if (stat == CoreStats.Defense)
            return Defense + UnityEngine.Mathf.FloorToInt(Multiplier * ilvl);

        return base.GetStatValue(stat, ilvl);
    }
}
