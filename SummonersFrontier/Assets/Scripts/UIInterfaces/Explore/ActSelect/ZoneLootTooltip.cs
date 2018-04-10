using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ZoneLootTooltip : MonoBehaviour {

    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemDescription;

    public void Initialize(ItemData item) {
        ItemName.text = item.Name;
        ItemDescription.text = item.Description;
    }
    public void Initialize(UniqueReference unique) {
        ItemName.text = unique.Name;
        ItemDescription.text = unique.GetItem().Description;
    }
}
