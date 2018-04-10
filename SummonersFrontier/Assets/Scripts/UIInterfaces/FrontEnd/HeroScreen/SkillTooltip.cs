using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class SkillTooltip : MonoBehaviour {

    public TextMeshProUGUI SkillName;
    public TextMeshProUGUI Cooldown;
    public TextMeshProUGUI Description;

    Transform overlayContainer;
    Transform restContainer;

    public RectTransform scaler;

    public void Initialize(Skill skill) {
        overlayContainer = GameObject.FindGameObjectWithTag("OverlayContainer").transform;
        restContainer = gameObject.transform.parent.transform;
        SkillName.text = skill.Name;
        
        Cooldown.text = "Cooldown: " + skill.Cooldown + " turns\nCast: " + (skill.CastDelay <= 0f ? "Instant" : skill.CastDelay.ToString("0.0"));
        if (skill.skillType == SkillTypes.Passive)
            Cooldown.text = "Cast: " + skill.skillTrigger + "\nPassive Skill";

        Description.text = skill.Tooltip();
        scaler.DOScale(0f, 0f);
    }

    public void Initialize(Skill skill, Hero hero) {
        overlayContainer = GameObject.FindGameObjectWithTag("OverlayContainer").transform;
        restContainer = gameObject.transform.parent.transform;
        SkillName.text = skill.Name;
        
        Cooldown.text = "Cooldown: " + skill.Cooldown + " turns\nCast: " + (skill.CastDelay <= 0f ? "Instant" : skill.CastDelay.ToString("0.0"));
        if (skill.skillType == SkillTypes.Passive)
            Cooldown.text = "Cast: " + skill.skillTrigger + "\nPassive Skill";

        Description.text = skill.Tooltip(hero);
        scaler.DOScale(0f, 0f);
    }

    public void Initialize(TapSkill skill) {
        overlayContainer = GameObject.FindGameObjectWithTag("OverlayContainer").GetComponent<RectTransform>();
        restContainer = gameObject.transform.parent.transform;
        SkillName.text = skill.Name;
        
        Cooldown.text = "Cooldown: " + skill.cooldown + " Minutes\n";
        if (skill.type == TapSkillType.DamageSkill)
            Cooldown.text += "Duration: Instant";
        else
            Cooldown.text += "Duration: " + skill.duration + " Seconds";

        Description.text = skill.TooltipText;
        scaler.DOScale(0f, 0f);
    }

    public void Initialize(LeaderSkill skill) {
        overlayContainer = GameObject.FindGameObjectWithTag("OverlayContainer").GetComponent<RectTransform>();
        restContainer = gameObject.transform.parent.transform;
        SkillName.text = skill.Name;

        switch(skill.type) {
            case LeadershipSkillType.CoreStatBoost:
                Cooldown.text = "Core Stat Boost\n+" + Mathf.RoundToInt(skill.percent * 100f) + "% " + skill.coreStat;
                break;
            case LeadershipSkillType.PrimaryStatBoost:
                Cooldown.text = "Primary Stat Boost\n+" + Mathf.RoundToInt(skill.percent * 100f) + "% " + skill.primaryStat;
                break;
            case LeadershipSkillType.SecondaryStatBoost:
                Cooldown.text = "Secondary Stat Boost\n+" + Mathf.RoundToInt(skill.percent * 100f) + "% " + skill.secondaryStat;
                break;
        }

        Description.text = skill.Tooltip;
        scaler.DOScale(0f, 0f);
    }

    public void ShowTooltip() {
        if (overlayContainer != null)
            gameObject.transform.SetParent(overlayContainer);
        
        scaler.DOKill();
        scaler.DOScale(1f, 0.25f).SetEase(Ease.OutCirc);
    }
    public void HideTooltip() {
        gameObject.transform.SetParent(restContainer);
        scaler.DOKill();
        scaler.DOScale(0f, 0.25f).SetEase(Ease.OutCirc);
    }
}
