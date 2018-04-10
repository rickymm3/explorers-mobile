using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ZoneInformationInterface : MonoBehaviour {

    ZoneData data;
    ZoneMovementInterface zoneMoveRef;

    public Image ZoneDisplayImage;

    public TextMeshProUGUI zoneTitle;
    public TextMeshProUGUI desciption;
    
    // TODO Boss stuff

    // TODO Monster stuff
    
    public void Initialize(ZoneData data, ZoneMovementInterface zoneMoveRef) {
        this.data = data;
        this.zoneMoveRef = zoneMoveRef;

        // change UI here
        zoneTitle.text = "Act " + data.Act + " - " + data.Name;
        desciption.text = data.Description;

        ZoneDisplayImage.sprite = data.LoadSprite();
    }

    public void Btn_NextZone() {
        //Debug.Log("Hit next");
        zoneMoveRef.NextZone();
    }

    public void Btn_PreviousZone() {
        //Debug.Log("Hit prev");
        zoneMoveRef.PreviousZone();
    }
}
