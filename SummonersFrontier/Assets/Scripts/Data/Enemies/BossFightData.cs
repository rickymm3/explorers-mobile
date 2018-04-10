using System;
using System.Collections.Generic;

[Serializable]
public class BossFightData : BaseData {
    public List<BossData> monsters = new List<BossData>();
    public LootTableData lootTable;
    public int Gold = 0;
    public int Experience = 0;

    public BossFightData(string Identity, List<BossData> monsters, LootTableData lootTable, int Gold, int Experience) {
        this.Identity = Identity;
        this.monsters = monsters;
        this.lootTable = lootTable;
        this.Gold = Gold;
        this.Experience = Experience;

        if (lootTable == null) UnityEngine.Debug.Log("ERROR ERROR NO LOOT TABLE");
    }
}
