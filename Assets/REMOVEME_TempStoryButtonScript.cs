using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class REMOVEME_TempStoryButtonScript : MonoBehaviour {

	public void BtnShowStoryExample() {
        StoryManager.Instance.DisplayStory("story_test");
    }

    public void BtnShowBranchingExample() {
        StoryManager.Instance.DisplayStory("story_choice_test");
    }

    public void BtnShowVideoAdExample() {
        Debug.Log("Showing Video Ad");
        AdvertisingManager.Instance.ShowVideoAd(HandleResults);
    }

    void HandleResults(ShowResult results) {
        Debug.Log("Video Ad Results: " + results);
        switch (results) {
            case ShowResult.Failed:
                Debug.Log("Ad failed to show.\n");
                break;
            case ShowResult.Skipped:
                Debug.Log("Ad was skipped before reaching the end.\n");
                break;
            case ShowResult.Finished:
                Debug.Log("Ad was completed - Reward the player.\n");

                GameAPIManager.Instance.Currency.AddCurrency(CurrencyTypes.GEMS, 10);

                break;
        }
    }
}
