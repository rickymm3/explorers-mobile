using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillDisplayHandler : MonoBehaviour {

    public Image icon;
    public TextMeshProUGUI SkillLevel;
    public SkillTooltip tooltip;

    Hero _hero;
    Skill _skill;

    public void Initialize(Skill skill, Hero hero = null) {
        _skill = skill;
        _hero = hero;

        if (hero != null)
            tooltip.Initialize(skill, hero);
        else
            tooltip.Initialize(skill);

        if (SkillLevel != null) SkillLevel.text = hero.GetSkillLevel(skill).ToString();
        icon.sprite = skill.LoadSprite();
    }

    public void Initialize(LeaderSkill skill) {
        tooltip.Initialize(skill);
        icon.sprite = skill.LoadSprite();
    }

    public void Initialize(TapSkill skill) {
        tooltip.Initialize(skill);
        icon.sprite = skill.LoadSprite();
    }

    void Update() {
        if (_hero != null && _skill != null)
            SkillLevel.text = _hero.GetSkillLevel(_skill).ToString();
    }

    public void PointerDown() {
        tooltip.ShowTooltip();
    }

    public void PointerUp() {
        Invoke("FadeTooltip", 0.5f);
    }

    void FadeTooltip() {
        tooltip.HideTooltip();
    }
}
