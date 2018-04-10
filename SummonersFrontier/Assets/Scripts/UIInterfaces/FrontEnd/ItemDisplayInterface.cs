using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExtensionMethods;

public class ItemDisplayInterface : MonoBehaviour {
    public Image itemBackground;
    public Image itemIcon;
    public TextMeshProUGUI qualityText;
    public Image questionMark;
    public TextMeshProUGUI DebugLabel;
    public TextMeshProUGUI txtStatsInfo;
    public GameObject StatsInfo;

    public Item item;
    Button _btn;

    HeroDetailsInterface heroInterface;

    Action<Item, ItemDisplayInterface> onItemClick;

    void Start() {
        _btn = this.GetComponent<Button>();
        _btn.onClick.AddListener(BtnOnClick);

#if !PIERRE
        DebugLabel.gameObject.SetActive(false);
#endif
    }

    private void BtnOnClick() {
        if(onItemClick!=null) onItemClick(item, this);

        UpdateStats();
    }

    private void UpdateStats() {
        if (heroInterface == null) return;

        if (item != null && item.data != null && item.isIdentified) {
            heroInterface.CalculateStatDifference(item);
        } else {
            heroInterface.ClearStatDifferences();
        }
    }

    public void LoadItem(Item item, Action<Item, ItemDisplayInterface> onItemClick, HeroDetailsInterface heroInterface) {
        this.item = item;
        this.onItemClick = onItemClick;

        itemBackground.color = item.GetBackgroundColor();
        qualityText.text = item.Quality.ToString();

        itemIcon.sprite = item.data.LoadSprite();
        this.heroInterface = heroInterface;

        DebugLabel.text = "#" + item.MongoID;

        UpdateIdentify();
    }

    public void UpdateIdentify() {
        questionMark.enabled = !item.isIdentified;
    }
}
