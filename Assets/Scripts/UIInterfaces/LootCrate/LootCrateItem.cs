using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ExtensionMethods;

public class LootCrateItem : Tracer {
    public Image filler;
    public Image imageIcon;
    public Image imageStarburst;
    public Image questionMark;
    public Image rarityGlow;

    private Item _itemData;
    private bool _isSelected = false;

    public Item itemData {
        get { return _itemData; }
        set {
            _itemData = value;

            switch (_itemData.Quality) {
                case ItemQuality.Common:
                    if (_itemData.Affixes.Count > 0)
                        rarityGlow.color = ColorConstants.ITEM_COMMON_WITH_AFFIX.ToHexColor(true);
                    else
                        rarityGlow.gameObject.SetActive(false);
                    break;
                case ItemQuality.Magic:     rarityGlow.color = ColorConstants.ITEM_MAGIC.ToHexColor(true); break;
                case ItemQuality.Rare:      rarityGlow.color = ColorConstants.ITEM_RARE.ToHexColor(true); break;
                case ItemQuality.Unique:    rarityGlow.color = ColorConstants.ITEM_UNIQUE.ToHexColor(true); break;
                default:                    rarityGlow.gameObject.SetActive(false); break;
            }

            imageIcon.sprite = _itemData.data.LoadSprite();
        }
    }

    public void UpdateIdentifiedIcon() {
        questionMark.enabled = !_itemData.isIdentified;
    }

    public bool isSelected {
        get { return _isSelected; }
        set {
            _isSelected = value;
            imageStarburst.gameObject.SetActive(value);
        }
    }

    void Start () {
        isSelected = false;
    }
	
}
