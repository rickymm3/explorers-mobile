using System.Collections;
using System;
using UnityEngine;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class SplashManagerInterface : PanelWithGetters {

    public string scene = "";
    public RectTransform progressBar;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI versionText;
    public GameObject LoadingContainer;
    public GameObject ClickToContinueContainer;
    public bool isAllowEmergencyWipe = true;

    float progress = 0f;
    string status = "";
    bool loadingComplete = false;
    bool userClicked = false;
    
    float _emergencyWipeSeconds = 0;
    ConfirmYesNoInterface _emergencyConfirm;

    void Start () {
        DataManager.Instance.OnJSONLoaded += OnJSONLoaded;
        DataManager.Instance.LoadJSON();

        //signals.OnUserLoginOK += OnUserLoginOK;
        signals.OnUserLoginOK += onUserLogin;
        signals.OnGetEverything += onAPILoaded;
        UpdateProgress(0.1f, "Loading Game Data");

        if(versionText!=null) {
            versionText.text = "v" + Application.version;
        }

        StartCoroutine(LoadingHandler());
    }

    void OnJSONLoaded() {
        UpdateProgress(0.21f, "Authenticating User");
        
        // Login if we are not
        if (!GameAPIManager.Instance.isLoggedIn) {
            GameAPIManager.Instance.AutoLogin()
                .Then(() => trace("NODE", "Auto Login worked!"))
                .Catch(err => MenuManager.Instance.Load("Interface_UserLoginSignup"));
        }
    }

    void onUserLogin(NodeJS.NodeResponse res) {
        var game = res["game"];

        PlayerManager.Instance.Username = res["username"];
        PlayerManager.Instance.Experience = game["xp"].AsInt;

        UpdateProgress(0.34f, "Loading User Data");

        DataManager.Instance.LoadPlayerData();
    }

    void onAPILoaded() {
        UpdateProgress(0.57f, "Initializing Game");

        SetupFeaturedItem();
    }

    void LoadAds() {
        UpdateProgress(0.83f, "Initializing Systems");

        AdvertisingManager.Instance.Initialize(() => { UpdateProgress(1.0f, "Loading Game"); });
    }

    void SetupFeaturedItem() {
        GameAPIManager.Instance.Shop.GetFeaturedItemSeed()
        .Then(fiResponse => {
            ShopFeaturedItemInterface.SetCurrentResponseAndUpdate(fiResponse);

            loadingComplete = true;
            LoadAds();
        });
    }

    void UpdateProgress(float target, string notificationString) {
        progress = target;
        status = notificationString;
    }

    void Update() {
        CheckEmergencyWipeInput();

        if (!loadingComplete) return;

#if PIERRE && UNITY_EDITOR
        userClicked = true;
#endif
        if(_emergencyConfirm==null && Input.GetMouseButtonUp(0)) userClicked = true;
    }

    private void CheckEmergencyWipeInput() {
        if (!isAllowEmergencyWipe || _emergencyConfirm != null || !Input.GetMouseButton(0)) {
            _emergencyWipeSeconds = 0;
            return;
        }

        _emergencyWipeSeconds += Time.deltaTime;

        if (_emergencyWipeSeconds > 2.0) {
            _emergencyConfirm = ConfirmYesNoInterface.Ask("Wipe Data?", "Would you like to\nwipe all PlayerPrefs?");
            _emergencyConfirm.Then(answer => {
                    if(answer=="YES") {
                        PlayerPrefs.DeleteAll();
                        audioMan.Play(SFX_UI.Explosion);
                    }

                    this.Wait(1.0f, () => _emergencyConfirm = null);
                });
        }
    }

    IEnumerator LoadingHandler() {
        while (!loadingComplete && progress < 1f) {
            statusText.text = status + "... [" + (progress * 100f).ToString("0") + "%]";
            progressBar.DOScaleX(progress, 0.5f);
            yield return new WaitForEndOfFrame();
        }
        progressBar.DOKill();
        progressBar.DOScaleX(1f, 0.5f);
        statusText.text = "Loading Complete... [100%]";

        yield return new WaitForSeconds(1f);
        LoadingContainer.SetActive(false);
        ClickToContinueContainer.SetActive(true);

        while (!userClicked) {
            yield return new WaitForEndOfFrame();
        }
        

        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
}
