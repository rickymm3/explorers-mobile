using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PlayerLevelInterface : Panel {

    public Image fade;

    public RectTransform LevelLabel;
    public RectTransform Level;

    public TextMeshProUGUI LevelText;

    public GameObject levelRewardPrefab;
    public RectTransform levelRewardContainer;

    public void Initialize(int level) {
        // Initial State
        PlayerManager.Instance.isLevelSequenceInProgress = true;
        LevelLabel.gameObject.SetActive(false);
        Level.gameObject.SetActive(false);
        fade.color = new Color(0f, 0f, 0f, 0f);

        StartCoroutine(RevealLevel(level));
    }
    
    IEnumerator RevealLevel(int level) {
        LevelText.text = "Lvl " + level;

        fade.DOColor(new Color(0f, 0f, 0f, 0.7f), 0.5f);
        
        yield return new WaitForSeconds(0.2f);
        LevelLabel.gameObject.SetActive(true);
        Level.gameObject.SetActive(true);

        LevelLabel.DOAnchorPosX(0f, 2f);
        Level.DOAnchorPosX(200f, 2f);

        yield return new WaitForSeconds(2f);

        LevelLabel.DOAnchorPosY(500f, 0.25f).SetEase(Ease.OutBack);
        Level.DOAnchorPosY(400f, 0.25f).SetEase(Ease.OutBack);

        // Reveal items here
        List<string> rewards = PlayerManager.Instance.GetLevelRewards(level);

        foreach(string rwdStr in rewards) {
            PlayerLevelReward reward = DataManager.Instance.playerLevelRewardsDataList.GetByIdentity(rwdStr);
            PlayerManager.Instance.AddLevelRewards(reward);

            // Create the reward indicator
            GameObject temp = Instantiate(levelRewardPrefab, levelRewardContainer);
            temp.GetComponent<PlayerLevelRewardsContainer>().Initialize(reward);

            yield return new WaitForSeconds(0.3f);
        }
        PlayerManager.Instance.LastLevel = level;

        // bool trigger to signal that another can be called
    }

    public void Btn_Close() {
        LevelLabel.DOAnchorPosY(2500f, 0.5f).SetEase(Ease.OutBack);
        Level.DOAnchorPosY(2500f, 0.5f).SetEase(Ease.OutBack);
        levelRewardContainer.DOAnchorPosY(-2500f, 0.5f).SetEase(Ease.OutBack);
        fade.DOColor(new Color(0f, 0f, 0f, 0f), 1f);

        StartCoroutine(CloseDelay(1f));
    }

    IEnumerator CloseDelay(float delay) {
        yield return new WaitForSeconds(delay);
        LevelLabel.gameObject.SetActive(false);
        Level.gameObject.SetActive(false);
        MenuManager.Instance.Pop();
        PlayerManager.Instance.isLevelSequenceInProgress = false;
    }
}