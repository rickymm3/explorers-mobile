using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class SkillUIHandler : MonoBehaviour {

    [Header("Skill UI")]
    public Image skillImage;
    public SkillTooltip tooltip;

    [Header("Cooldown UI")]
    public Image cooldownImage;
    public TextMeshProUGUI cooldownText;

    public GameObject passiveNotifier;

    Image highlight;
    BossBattleInterface UIinterface;
    public Skill skill;
    int cooldown = 0;
    float tapduration = 0f;
    bool trackTouch = false;

    private List<VineMovement> _vines = new List<VineMovement>();

    public void Initialize(int index, Hero hero, BossBattleInterface UIinterface) {
        this.UIinterface = UIinterface;
        skill = hero.Skills[index];
        cooldown = hero.Skills[index]._cooldown;

        SetCooldown();

        highlight = GetComponent<Image>();

        skillImage.sprite = skill.LoadSprite();
        _vines = GetComponentsInChildren<VineMovement>().ToList();

        tooltip.Initialize(skill, hero);

        passiveNotifier.SetActive(skill.skillType == SkillTypes.Passive);
    }

    public void Btn_Skill() {
        if (cooldown < 1) {
            UIinterface.SelectSkill(this);
            highlight.DOFade(1f, 0.1f);
        }
    }

    public void SkillTapDown() {
        tapduration = 0f;
        trackTouch = true;
    }

    public void SkillTapRelease() {
        if (skill.skillType != SkillTypes.Passive) {
            if (tapduration <= 0.25f) {
                Btn_Skill();
            } else {
                tooltip.HideTooltip();
            }
        } else {
            tooltip.HideTooltip();
        }
        trackTouch = false;
        tapduration = 0f;
    }

    void Update() {
        if (!trackTouch) return;

        tapduration += Time.deltaTime;

        if (tapduration > 0.25f) {
            tooltip.ShowTooltip();
        }
    }

    public void Deselect() {
        foreach (VineMovement vine in _vines)
            vine.Hide();
    }

    public void Select() {
        foreach (VineMovement vine in _vines)
            vine.Show();
    }

    void SetCooldown() {
        Color c = cooldownImage.color;

        if (cooldown > 0) {
            c.a = 0.65f;
            cooldownImage.color = c;

            cooldownText.text = cooldown.ToString();
        } else {
            c.a = 0f;
            cooldownImage.color = c;

            cooldownText.text = "";
        }
    }
}
