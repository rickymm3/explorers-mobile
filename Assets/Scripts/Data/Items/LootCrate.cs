using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LootCrate : MongoData<BaseData> {

	public string lootTableIdentity;

    public CrateTypeData CrateType;
    public float MagicFind = 0f;
    public float ItemLevel = 20f;
    public float Variance = 0f;
    public int ExplorationID;

    private LootTableData _lootTableData;
    private List<Item> _generatedItems;
    
    public LootTableData LootTableData {
        get {
            if (_lootTableData == null) {
                _lootTableData = DataManager.Instance.lootTableDataList.GetByIdentity(lootTableIdentity);
            }

            return _lootTableData;
        }
    }

    public List<Item> generatedItems {
        get {
            if(_generatedItems==null) {
                Tracer.traceError("You must call 'GenerateItems()' first!");
                return null;
            }
            return _generatedItems;
        }
    }

    public List<Item> GenerateItems() {
        LootCollection lootCollect = LootTableData.GetRandomItems(this);
        _generatedItems = lootCollect.randomItems;

        return _generatedItems;
    }

    public LootCrate(string lootTableIdentity, float ItemLevel, float Variance, CrateTypeData CrateType, float MagicFind = 0f) {
        this.className = "LootCrate";
        this.lootTableIdentity = lootTableIdentity;
        this.ItemLevel = ItemLevel;
        this.Variance = Variance;
        this.MagicFind = MagicFind;
        this.CrateType = CrateType;
    }
}
