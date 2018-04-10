using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HeaderBoostDisplay : Tracer {
    static Color COLOR_DISABLED = new Color(.5f, .5f, .5f, .8f);
    static Color COLOR_ENABLED = Color.white;

    [HideInInspector] public BoostData boostData;
    public Image icon;
    public TextMeshProUGUI txtCount;
    public TextMeshProUGUI txtNumLeft;
    public RectTransform badge;
    public Button btnBoost;

    public void Init(BoostSlot boostSlot) {
        Image bg = this.GetComponent<Image>();

        if (boostSlot == null || boostSlot.data==null || boostSlot.count==0) {
            badge.gameObject.SetActive(false);
            icon.gameObject.SetActive(false);
            bg.color = (boostSlot == null) ? COLOR_DISABLED : COLOR_ENABLED;
            return;
        }

        badge.gameObject.SetActive(true);
        icon.gameObject.SetActive(true);
        bg.color = COLOR_ENABLED;

        txtCount.text = boostSlot.count.ToString();

        InitDataOnly(boostSlot.data);
    }

    public void InitDataOnly(BoostData boostData) {
        this.boostData = boostData;

        var sprite = boostData.LoadSprite();

        if (sprite == null) {
            traceError("Cannot find boost sprite for: " + boostData.sprite);
        }

        this.icon.sprite = sprite;

        if (txtNumLeft!=null) {
            UpdateNumLeft();
        }
    }

    public void UpdateNumLeft() {
        txtNumLeft.text = boostData.boostType.GetAmount() + "\n<size=-15>LEFT</size>";
    }
}
