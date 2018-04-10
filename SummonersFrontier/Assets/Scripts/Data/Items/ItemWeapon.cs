using System.Collections.Generic;

[System.Serializable]
public class ItemWeapon : ItemData {
    int _damage = 0;
    public int Damage {
        get { return _damage; }
    }
	public WeaponType WeaponType = WeaponType.Sword;
    
	public ItemWeapon(int ID, string Identity, string Name, int Value, string Description, WeaponType WeaponType, int Damage, string Sprite, int Tier, float Multiplier, List<UniqueReference> UniqueItemReference) : base (ID, Identity, Name, ItemType.Weapon, EquipmentType.Weapon, Value, Description, Sprite, Tier, Multiplier, UniqueItemReference) {
		this.WeaponType = WeaponType;
        _damage = Damage;
	}

    public ItemWeapon(ItemWeapon data) : base(data.ID, data.Name, data.Identity, ItemType.Weapon, EquipmentType.Weapon, data.Value, data.Description, data.Sprite, data.Tier, data.Multiplier, data.UniqueItemReference) {
        this.WeaponType = data.WeaponType;
        this._damage = data.Damage;
    }

    public override int GetStatValue(CoreStats stat, float ilvl) {
        if (stat == CoreStats.Damage)
            return Damage + UnityEngine.Mathf.FloorToInt(Multiplier * ilvl);

        return base.GetStatValue(stat, ilvl);
    }
}
