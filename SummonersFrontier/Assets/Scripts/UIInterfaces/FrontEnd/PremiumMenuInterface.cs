using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class PremiumMenuInterface : MonoBehaviour {

    public CampInterface camp;
    public Button VideoAdButton;

    int videoPlays = 0;
    DateTime date;

    void Start() {
        int year = PlayerPrefs.GetInt("videoAdDate_year");
        int month = PlayerPrefs.GetInt("videoAdDate_month");
        int day = PlayerPrefs.GetInt("videoAdDate_day");
        date = new DateTime(year, month, day);
        TimeSpan span = DateTime.Now - date;

        //Debug.Log(" -- Hours Since Last Ad: " + (span.Days * 24 + span.Hours) + " [" + date + "][" + DateTime.Now + "][" + span + "]");

        if (span.Days * 24 + span.Hours > 20) {
            videoPlays = 0;
            PlayerPrefs.SetInt("videoAd_count", 0);
        } else {
            videoPlays = PlayerPrefs.GetInt("videoAd_count");

            if (videoPlays == 5) {
                VideoAdButton.interactable = false;
            }
        }
    }

    public void BtnVideoAd() {
        Debug.Log("Showing Video Ad.");
        AdvertisingManager.Instance.ShowVideoAd(HandleResults);
    }

    public void BtnPremiumShop() {
        Debug.Log("Premium Shop Trigger Here.");
        camp.BtnPremiumShop();
    }

    void HandleResults(ShowResult results) {
        Debug.Log("Video Ad Results: " + results);

        if (videoPlays == 5) {
            Debug.Log("Ad Limit Reached");
            return;
        }

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
                videoPlays++;
                PlayerPrefs.SetInt("videoAd_count", videoPlays);
                if (videoPlays > 4) {
                    PlayerPrefs.SetInt("videoAdDate_year", DateTime.Now.Year);
                    PlayerPrefs.SetInt("videoAdDate_month", DateTime.Now.Month);
                    PlayerPrefs.SetInt("videoAdDate_day", DateTime.Now.Day);
                    VideoAdButton.interactable = false;
                }
                break;
        }
    }
}
