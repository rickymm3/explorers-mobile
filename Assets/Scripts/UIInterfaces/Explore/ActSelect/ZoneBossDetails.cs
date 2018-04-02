using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ZoneBossDetails : MonoBehaviour {

    public TextMeshProUGUI BossName;
    public Image BossImage;
    public Image WeaknessImage;

    public void Initialize(BossData boss) {
        Initialize(boss.Name, boss.LoadPortrait(), boss.elementalType);
    }

    public void Initialize(string name, Sprite sprite, ElementalTypes elementalType = ElementalTypes.None) {
        BossName.text = name;
        BossImage.sprite = sprite;

        WeaknessImage.gameObject.SetActive(true);
        Sprite elementalSprite = null;
        switch(elementalType) {
            case ElementalTypes.Fire:
                elementalSprite = Resources.Load<Sprite>("Items/None/element_water");
                break;
            case ElementalTypes.Water:
                elementalSprite = Resources.Load<Sprite>("Items/None/element_nature");
                break;
            case ElementalTypes.Nature:
                elementalSprite = Resources.Load<Sprite>("Items/None/element_fire");
                break;
            case ElementalTypes.Dark:
                elementalSprite = Resources.Load<Sprite>("Items/None/element_light");
                break;
            case ElementalTypes.Light:
                elementalSprite = Resources.Load<Sprite>("Items/None/element_dark");
                break;
            default:
                WeaknessImage.gameObject.SetActive(false);
                break;
        }

        WeaknessImage.sprite = elementalSprite;
    }
}
