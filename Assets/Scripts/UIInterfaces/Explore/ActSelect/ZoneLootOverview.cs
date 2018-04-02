using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ZoneLootOverview : MonoBehaviour {
    public Image itemIcon;
    public Image hiddenIcon;
    public RectTransform tooltip;
    bool showTooltip = true;

    public void Initialize(ItemData item) {
        itemIcon.sprite = item.LoadSprite();
        
        hiddenIcon.gameObject.SetActive(false);
        showTooltip = true;

        // Setup tooltip here
        tooltip.GetComponent<ZoneLootTooltip>().Initialize(item);

        PointerUp();
    }
    public void Initialize(UniqueReference item) {
        if (item == null) {
            gameObject.SetActive(false);
            return;
        }
        itemIcon.sprite = item.LoadSprite();

        if (PlayerManager.Instance.HasItemReference(item)) {
            hiddenIcon.gameObject.SetActive(false);
            itemIcon.gameObject.SetActive(true);
            showTooltip = true;
        } else {
            hiddenIcon.gameObject.SetActive(true);
            itemIcon.gameObject.SetActive(false);
            showTooltip = false;
        }

        // Setup tooltip here
        tooltip.GetComponent<ZoneLootTooltip>().Initialize(item);

        PointerUp();
    }

    public void PointerUp() {
        if (!showTooltip) return;
        tooltip.DOKill();
        tooltip.DOScale(0f, 0.3f);
    }

    public void PointerDown() {
        if (!showTooltip) return;
        tooltip.DOKill();
        tooltip.DOScale(1f, 0.3f);
    }
}
