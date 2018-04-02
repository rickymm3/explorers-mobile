using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActData : BaseData {
    public string Name;
    public string Description;
    public int ActNumber = 0;
    public List<ZoneData> Zones = new List<ZoneData>();
    
    public ActData(string Identity, string Name, string Description, int ActNumber, List<ZoneData> Zones) {
        this.Identity = Identity;
        this.Name = Name;
        this.Description = Description;
        this.ActNumber = ActNumber;
        this.Zones = Zones;
    }

    public Sprite LoadSprite() {
        return Resources.Load<Sprite>("ActZoneArt/ActCovers/Act" + ActNumber);
    }
}
