using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using ExtensionMethods;

public class BattleResultsHeroResults : MonoBehaviour {

    public Image portrait;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI nameTextHighlight;
    public TextMeshProUGUI levelText;
    public RectTransform xpBar;
    public TextMeshProUGUI xpText;

    private Hero _hero;
    private int _exp;
    private int levelRef;
    private int experienceRef;

    float experienceGained = 0f;
    //float experienceNextLevelDistance = 0f;
    //float experienceRemainderAfterLevel = 0f;
    float experienceToNextLevel = 0f;
    float experienceProgress = 0f;

    public void Initialize(Hero hero, int exp, int index) {
        portrait.sprite = Resources.Load<Sprite>("Hero/" + hero.data.Identity.ToLower() + "/portrait");
        levelText.text = "LVL " + hero.Level;
        nameText.text = hero.data.Name;
        nameTextHighlight.text = hero.data.Name;

        _hero = hero;
        _exp = exp;
        levelRef = hero.Level;
        
        experienceToNextLevel = (float) GameManager.Instance.GetExperienceRequiredForNextLevel(_hero.Level);
        experienceRef = (_hero.Experience - GameManager.Instance.GetExperienceRequiredForLevel(_hero.Level - 1));
        
        experienceGained = experienceRef;
        //experienceNextLevelDistance = experienceToNextLevel - _hero.Experience;
        //experienceRemainderAfterLevel = experienceGained - experienceToNextLevel;

            // Starting progress of the hero xp bar
        experienceProgress = experienceRef / experienceToNextLevel;
        xpBar.localScale = new Vector3(experienceProgress, 1, 1);
        
        // Start the animations
        StartCoroutine(ExperienceBarAnimation(0.25f * index));

        Debug.Log("Hero Details before CR: [Level " + _hero.Level + "][XP " + _hero.Experience + "]");
    }
    

    IEnumerator ExperienceBarAnimation(float delay) {
        yield return new WaitForSeconds(delay);

        experienceProgress = ((float) (experienceRef + _exp)) / experienceToNextLevel;

        float duration = 4f;
        //float experienceGained = experienceToNextLevel - experienceRef;
        int levelCounter = levelRef;

        if (experienceProgress > 1f) {
            while (experienceProgress > 1f) {
                StartCoroutine(TweenIntToValue((int) experienceToNextLevel, xpText, " / " + experienceToNextLevel, 1f));
                yield return xpBar.DOScaleX(1f, 1f).WaitForCompletion();

                experienceGained = experienceToNextLevel;
                levelCounter++;
                
                yield return StartCoroutine(LevelUp(levelCounter));
                
                experienceToNextLevel = (float) GameManager.Instance.GetExperienceRequiredForNextLevel(levelCounter);
                experienceProgress = ((float) (experienceRef + _exp - experienceGained)) / experienceToNextLevel;

                Debug.Log(" >> [XP_UI] Experience progress [" + _hero.Name + "]: [xpprg: " + experienceProgress + "][_xp: " + _exp + "][xpg: " + experienceGained + "][xptnl: " + experienceToNextLevel + "][hxp: " + _hero.Experience + "][hl: " + _hero.Level + "]");
            }
            yield return StartCoroutine(LevelUp(levelCounter));

            //experienceToNextLevel = (float) GameManager.Instance.GetExperienceRequiredForLevel(_hero.Level);
            //experienceProgress = ((float) (experienceRef + _exp)) / experienceToNextLevel;
            StartCoroutine(TweenIntToValue(Mathf.RoundToInt(experienceProgress * experienceToNextLevel), xpText, " / " + experienceToNextLevel, duration * 0.5f));
            xpBar.localScale = new Vector3(0f, 1, 1);
            xpBar.DOScaleX(experienceProgress, duration * 0.5f);
        } else {
            // No level
            StartCoroutine(TweenIntToValue(Mathf.RoundToInt(experienceProgress * experienceToNextLevel), xpText, " / " + experienceToNextLevel, duration * 0.5f));
            xpBar.DOScaleX(experienceProgress, duration);
        }
    }

    IEnumerator LevelUp(int levelCounter) {
        xpBar.localScale = new Vector3(0f, 1, 1);
        levelText.text = "LVL " + (levelCounter);

        // maybe a shine on the party member bar or particle effect here

        yield return new WaitForEndOfFrame();
    }

    IEnumerator TweenIntToValue(int value, TextMeshProUGUI textArea, string affix, float duration) {
        float current = 0f;
        float timer = 0f;
        while (timer < duration) {
            timer += Time.deltaTime;
            if (timer > duration) timer = duration;
            current = timer / duration;

            textArea.text = (current * (float) value).ToString("0") + affix;

            yield return new WaitForEndOfFrame();
        }
    }
}
