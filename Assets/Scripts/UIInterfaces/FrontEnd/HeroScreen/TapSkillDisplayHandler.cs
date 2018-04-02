using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TapSkillDisplayHandler : MonoBehaviour {
    
    public Image icon;
    public SkillTooltip tooltip;

    Hero _hero;
    TapSkill _skill;

    public void Initialize(TapSkill skill) {
        _skill = skill;

        tooltip.Initialize(skill);
        icon.sprite = skill.LoadSprite();
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
