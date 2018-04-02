using System.Collections.Generic;

[System.Serializable]
public class ItemCurrency : ItemData {
    public CurrencyTypes CurrencyType = CurrencyTypes.GOLD;
    public int Reward = 0;

    public ItemCurrency(int ID, string Identity, string Name, string Description, string Sprite, int Value, CurrencyTypes CurrencyType, int Reward, List<UniqueReference> UniqueItemReference) : base(ID, Identity, Name, ItemType.Currency, EquipmentType.None, Value, Description, Sprite, 0, 0, null) {
        this.CurrencyType = CurrencyType;
        this.Reward = Reward;
    }

    public ItemCurrency(ItemCurrency data) : base(data.ID, data.Identity, data.Name, ItemType.Currency, data.EquipType, data.Value, data.Description, data.Sprite, data.Tier, data.Multiplier, data.UniqueItemReference) {
        this.CurrencyType = data.CurrencyType;
        this.Reward = data.Reward;
    }
}
