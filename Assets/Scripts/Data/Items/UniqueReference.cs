using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UniqueReference : BaseData {
    public string Reference;
    public string Name;
    public string Description;
    public string Sprite;
    public int Value;
    public List<ItemAffix> AffixReferences;

    public UniqueReference(int ID, string Identity, string Reference, string Name, string Description, int Value, List<ItemAffix> AffixReferences, string Sprite) {
        this.ID = ID;
        this.Identity = Identity;
        this.Reference = Reference;
        this.Name = Name;
        this.Value = Value;
        this.AffixReferences = AffixReferences;
        this.Description = Description;
        this.Sprite = Sprite;
    }

    public ItemData GetItem() {
        return DataManager.Instance.itemDataList.GetByIdentity(Reference);
    }

    public Sprite LoadSprite() {
        // TODO Change this to use the new fields in teh CSVs
        return Resources.Load<Sprite>("Items/Uniques/" + this.Sprite);
    }
}
