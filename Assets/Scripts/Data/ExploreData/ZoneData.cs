using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZoneData : BaseData {
    public string Name = "";
    public string Description = "";
    public int Act = 1;
    public int Zone = 1;
    public int ActZoneID = 0;
    public float MonsterTimer = 15f;
    public float BossPower = 1f;
    public int ZoneHP = 100;
    public float MinItemLevel = 20;
    public float Variance = 0;
    public string StoryEvent = "";
    public ZoneDifficulty Difficulty = ZoneDifficulty.Normal;

    public BossFightData BossFight;

    public LootTableData LootTable;

    public List<MonsterData> Monsters = new List<MonsterData>();

    public ZoneData(string Identity, string Name, string Description, int Act, int Zone, int ActZoneID, float BossPower, int ZoneHP, LootTableData LootTable, float MinItemLevel, float Variance, float MonsterTimer, BossFightData BossFight, List<MonsterData> Monsters, ZoneDifficulty Difficulty, string StoryEvent = "") {
        this.Identity = Identity;
        this.Name = Name;
        this.Description = Description;
        this.LootTable = LootTable;
        this.Act = Act;
        this.Zone = Zone;
        this.ActZoneID = ActZoneID;
        this.MonsterTimer = MonsterTimer;
        this.BossPower = BossPower;
        this.ZoneHP = ZoneHP;
        this.BossFight = BossFight;
        this.Monsters = Monsters;
        this.MinItemLevel = MinItemLevel;
        this.Variance = Variance;
        this.Difficulty = Difficulty;
        this.StoryEvent = StoryEvent;
    }

    public Sprite LoadSprite() {
        return Resources.Load<Sprite>("ActZoneArt/Act" + Act + "/Zone" + Zone + "/display");
    }

    public Sprite LoadSpriteBackground() {
        return Resources.Load<Sprite>("ActZoneArt/Act" + Act + "/Zone" + Zone + "/background");
    }

    public Sprite LoadSpriteBackgroundBW() {
        return Resources.Load<Sprite>("ActZoneArt/Act" + Act + "/Zone" + Zone + "/colorless");
    }
}
