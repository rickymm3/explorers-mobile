using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdvertisingManager : Singleton<AdvertisingManager> {

    public string GameID = "1724650";

    public IEnumerator Initialize(Action onLoaded = null) {
        Advertisement.Initialize(GameID, true);

        Debug.Log("[GameID: " + GameID + "] Initializing Ads.\n");

        yield return StartCoroutine(LoadAdvertising(onLoaded));
    }

    IEnumerator LoadAdvertising(Action onLoaded) {
        while (!Advertisement.IsReady()) {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("[GameID: " + GameID + "] Ads Initialized.\n");

        if (onLoaded != null) onLoaded();
    }

    public void ShowVideoAd(Action<ShowResult> callback = null) {
        if (!Advertisement.IsReady()) {
            Debug.Log("[GameID: " + GameID + "] Ads not ready for placement.");
            return;
        }

        Debug.Log("Video Ad requested");

        ShowOptions options = new ShowOptions();
        if (callback != null)
            options.resultCallback = callback;
        
        Advertisement.Show(options);
    }
}
