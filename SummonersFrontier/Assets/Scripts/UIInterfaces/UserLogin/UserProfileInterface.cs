using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;
using DG.Tweening;
using TMPro;
using RSG;


public class UserProfileInterface : PanelWithGetters {

    [Header("Containers, etc.")]
    public Image Fader;
    public RectTransform Panel;
    public CanvasGroup CanvasGroup;

    [Header("Username & Level Progress Bar")]
    public HeaderBarInterface.UserInfo userInfo;

    [Header("Buttons")]
    public Button btnLogout;
    public Button btnSoundToggle;
    public Button btnClose;

    [Header("Sound")]
    public Sprite spriteSoundOn;
    public Sprite spriteSoundOff;

    [Header("Currencies")]
    public TextMeshProUGUI txtGold;
    public TextMeshProUGUI txtGems;

    [Header("Tabs")]
    [SerializeField] public List<UserProfileTab_Base> Tabs;

    Vector2 _panelPosInit;

    // Use this for initialization
    void Start () {
        Fader.color = new Color(0,0,0,0);

        //btnSoundToggle.onClick.AddListener( Btn_OnSoundToggle );
        btnLogout.onClick.AddListener( Btn_OnLogout );
        btnClose.onClick.AddListener( Btn_OnClose );

        //UpdateSoundIcon();

        CanvasGroup.alpha = 0;

        _panelPosInit = Panel.anchoredPosition;
        Panel.anchoredPosition = _panelPosInit + new Vector2(0, 10);

        foreach (UserProfileTab_Base tab in Tabs) {
            tab.btn.onClick.AddListener(() => {
                audioMan.Play(SFX_UI.Click);
                tab.container.SetAsLastSibling();
                tab.scrollRect.DOVerticalNormalizedPos(1, 0.3f);
            });
        }

        txtGold.text = CurrencyTypes.GOLD.GetAmount().ToString();
        txtGems.text = CurrencyTypes.GEMS.GetAmount().ToString();

        HeaderBarInterface.SetUserInfo(userInfo, playerMan.Username, playerMan.Level, playerMan.ExperienceProgress());

        this.Wait(-1, () => {
            Fader.DOFade(0.8f, 0.3f);
            CanvasGroup.DOFade(1, 0.4f);
            Panel.DOAnchorPos(_panelPosInit, 0.4f);
        });
    }

    void Btn_OnClose() {
        if (!Close()) return;

        btnClose.interactable = false;
    }

    //void Btn_OnSoundToggle() {
    //    GamePrefs.IS_SOUND_ENABLED = !GamePrefs.IS_SOUND_ENABLED;

    //    UpdateSoundIcon();
    //}

    //private void UpdateSoundIcon() {
    //    Image icon = btnSoundToggle.transform.Find("Icon").GetComponent<Image>();

    //    if(GamePrefs.IS_SOUND_ENABLED) {
    //        icon.sprite = spriteSoundOn;
    //        audioMan.isMuted = false;
    //    } else {
    //        icon.sprite = spriteSoundOff;
    //        audioMan.isMuted = true;
    //    }
    //}

    public override void CloseTransition() {
        CanvasGroup.DOFade(0, 0.3f);
        Fader.DOFade(0, 0.3f);

        Panel.DOAnchorPos(_panelPosInit + new Vector2(0, 10), 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                MenuManager.Instance.Remove(this);
            });
    }

    void Btn_OnLogout() {
        if (_isClosing) return;

        ConfirmYesNoInterface.Ask("Logout", "Are you sure\nyou want to\nlogout?")
            .Then( answer => {
                if(answer!="YES") return;

                btnLogout.interactable = false;

                API.Users.Logout()
                    .Then(res => OnLogoutComplete());
            })
            .Catch( err => {
                trace("ERR: " + err.Message);
            });
    }

    private void OnLogoutComplete() {
        API.ClearUserLogin();
        UnityEngine.SceneManagement.SceneManager.LoadScene("RootReset");
    }
}

[Serializable]
public class UserProfileTab_Base : Tracer {
    public Button btn;
    public RectTransform container;
    public ScrollRect scrollRect;
    public float preloadDelay = 0.0f;

    public void WaitForData() {
        StartCoroutine(__WaitForData());
    }

    private IEnumerator __WaitForData() {
        while(!DataManager.Instance.isLoaded) yield return new WaitForEndOfFrame();

        if (preloadDelay > 0) {
            yield return new WaitForSeconds(preloadDelay);
        }
        
        OnDataLoaded();

        if(scrollRect!=null) {
            scrollRect.verticalNormalizedPosition = 1;
        }
    }

    public virtual void OnDataLoaded() { }
}
