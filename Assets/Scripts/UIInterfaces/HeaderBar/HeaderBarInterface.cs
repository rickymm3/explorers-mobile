using System.Collections;
using System.Collections.Generic;
using ExtensionMethods;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using NodeJS;
using SimpleJSON;

[Serializable]
public class CurrencyIcon {
    [SerializeField] public TextMeshProUGUI label;
    [SerializeField] public RectTransform imageRect;
}

[Serializable]
public class HeaderBarInterface : PanelWithGetters {
    [Serializable]
    public class BoostTypeDictionary : SerializableDictionary<BoostType, string> { }

    public RectTransform userInformation;
    public RectTransform PlayerLevelRect;
    public Button BtnUsernameRibbon;
    public TextMeshProUGUI txtSectionTitle;

    //[Header("Animation Times")]
    float timeForShowHide = 0.3f;
    float timeForCurrencyBump = 0.6f;

    [Header("User Info")]
    public UserInfo userInfo;


    [Inspectionary("Currency", "Icon and Label")]
    [SerializeField]
    public CurrencyIconsDictionary currencyIcons;

    [Header("Boost Info")]
    public HorizontalLayoutGroup BoostHorizontalLayout;
    public RectTransform BoostTab;
    public GameObject BoostSlotTemplate;
    public GameObject BoostChoiceTemplate;
    public RectTransform BoostVerticalLayout;
    public CanvasGroup BoostCanvasGroup;
    public Button btnBoostTabToggle;
    public Button btnBoostAddSlot;
    public TextMeshProUGUI txtBoostAddSlot;
    bool _isBoostTabOpen = false;
    float _boostTabHeight;
    float _boostVertHeight;
    List<HeaderBoostDisplay> _boostSlots = new List<HeaderBoostDisplay>();
    List<HeaderBoostDisplay> _boostChoices = new List<HeaderBoostDisplay>();

    public bool isBoostTabOpen {
        get { return _isBoostTabOpen; }
        set { _isBoostTabOpen = value; UpdateBoostTab(); }
    }

    void Start() {
        if (DataManager.globals == null) {
            BoostVerticalLayout.gameObject.SetActive(false);
            return;
        }

        signals.OnChangedCurrency += OnChangedCurrency;
        signals.OnUserLoginOK += OnUserLoginOK;
        signals.OnAllCurrenciesUpdated += RefreshBoostSlots;
        signals.OnBoostsUpdated += RefreshBoostSlots;
        playerMan.OnExperienceGain += UpdateUserUI;
        playerMan.OnLevel += UpdateUserUI;
        
        BtnUsernameRibbon.onClick.AddListener(Btn_OnUsernameClick);
        btnBoostTabToggle.onClick.AddListener(Btn_BoostsTabToggle);
        btnBoostAddSlot.onClick.AddListener(Btn_BoostsAddSlot);

        RefreshCurrency();
        UpdateUserUI();
        UpdateBoostUI();
        UpdateBoostTab(true);
    }

    public void SetSectionTitle(string title) {
        txtSectionTitle.text = title;
    }

    void OnDestroy() {
        signals.OnChangedCurrency -= OnChangedCurrency;
        signals.OnUserLoginOK -= OnUserLoginOK;
        signals.OnAllCurrenciesUpdated -= RefreshBoostSlots;
        signals.OnBoostsUpdated -= RefreshBoostSlots;
        playerMan.OnExperienceGain -= UpdateUserUI;
        playerMan.OnLevel -= UpdateUserUI;
    }


    private void UpdateBoostUI() {
        _boostSlots.Clear();
        _boostChoices.Clear();

        for (int b=0; b<GlobalProps.BOOST_LIMIT.GetInt(); b++) {
            _boostSlots.Add( this.Clone<HeaderBoostDisplay>(BoostSlotTemplate) );
        }

        BoostSlotTemplate.SetActive(false);

        foreach (BoostData boostData in dataMan.boostDataList) {
            HeaderBoostDisplay boostChoice = this.Clone<HeaderBoostDisplay>(BoostChoiceTemplate);
            boostChoice.btnBoost.onClick.AddListener(() => ActivateBoost(boostChoice));
            boostChoice.boostData = boostData;
            _boostChoices.Add( boostChoice );
        }

        BoostChoiceTemplate.SetActive(false);

        _boostTabHeight = BoostTab.rect.height;
        _boostVertHeight = BoostVerticalLayout.rect.height;

        RefreshBoostSlots();
    }

    public void RefreshBoostSlots() {
        var boosts = PlayerManager.Instance.BoostsSlotsActive;

        int b = 0;
        foreach (var boostSlot in _boostSlots) {
            BoostSlot boost = b >= boosts.Count ? null : boosts[b];
            boostSlot.Init(boost);

            b++;
        }

        foreach(var boostChoice in _boostChoices) {
            boostChoice.InitDataOnly(boostChoice.boostData);
        }

        if(boosts.Count>=GlobalProps.BOOST_LIMIT.GetInt()) {
            btnBoostAddSlot.interactable = false;
            txtBoostAddSlot.text = "<color=#dddddd>NO MORE\nSLOTS\nAVAILABLE</color>"; //<size=-5></size>
        } else {
            btnBoostAddSlot.interactable = true;
            var cost = CurrencyManager.ParseToCost(GlobalProps.BOOST_SLOT_COST.GetString());
            txtBoostAddSlot.text = "ADD SLOT\n<size=-3>FOR {0} {1}</size>".Format2(cost.amount, cost.type);
        }
    }

    public void Btn_BoostsTabToggle() {
        isBoostTabOpen = !_isBoostTabOpen;
    }

    public void Btn_BoostsAddSlot() {
        if (BoostCanvasGroup.alpha != 1) return;

        var cost = CurrencyManager.ParseToCost( GlobalProps.BOOST_SLOT_COST.GetString() );
        if(cost.type.GetAmount()<cost.amount) {
            TimelineTween.ShakeError( btnBoostAddSlot.gameObject );
            return;
        }

        BoostCanvasGroup.alpha = 0.5f;

        API.Users.BoostAddSlot(cost)
            .Then(res => {
                trace("Boost slot added successfully.");
                RefreshBoostSlots();
                BoostCanvasGroup.alpha = 1;
            })
            .Catch(err => {
                BoostCanvasGroup.alpha = 1;
                TimelineTween.ShakeError(btnBoostAddSlot.gameObject);
                traceError("Could not add new boost slot: " + GameAPIManager.GetErrorMessage(err));
            });
    }

    void ActivateBoost(HeaderBoostDisplay boostChoice) {
        if(BoostCanvasGroup.alpha!=1) return;

        var slots = PlayerManager.Instance.BoostsSlotsActive;
        var slotUnused = slots.Find(slot => slot.data==null);
        
        //Do an early currency check (client-side), it also checks server-side for HACKERZ! :D
        var boostCurrency = boostChoice.boostData.ToCurrencyType();
        int boostAmount = boostCurrency.GetAmount();
        if(slotUnused == null || boostAmount <= 0) {
            TimelineTween.ShakeError(boostChoice.gameObject);
            return;
        }

        trace("Activating boost!");
        trace(boostChoice.boostData);
        
        BoostCanvasGroup.alpha = 0.5f;

        API.Users.BoostActivate(slotUnused.slotID, boostChoice.boostData)
            .Then(res => {
                boostChoice.UpdateNumLeft();
                BoostCanvasGroup.alpha = 1;
                RefreshBoostSlots();
            })
            .Catch( err => {
                traceError("Could not activate boost: " + GameAPIManager.GetErrorMessage(err));
                TimelineTween.ShakeError(boostChoice.gameObject);
                BoostCanvasGroup.alpha = 1;
            });
    }

    void UpdateBoostTab(bool isImmediate=false) {
        if (_isBoostTabOpen) {
            BoostVerticalLayout.gameObject.SetActive(true);
            BoostCanvasGroup.alpha = 0;
            ResizeBoostTab(_boostTabHeight + _boostVertHeight);
            RefreshBoostSlots();

            if (isImmediate) {
                BoostCanvasGroup.alpha = 1;
            } else {
                BoostCanvasGroup.DOFade(1, 0.5f).SetDelay(0.2f);
            }
        } else {
            if (isImmediate) {
                BoostCanvasGroup.alpha = 0;
            } else {
                BoostCanvasGroup.DOFade(0, 0.2f);
            }

            ResizeBoostTab(_boostTabHeight)
                .OnComplete(() => {
                    BoostVerticalLayout.gameObject.SetActive(false);
                });
        }
    }

    Tweener ResizeBoostTab(float height) {
        return BoostTab.DOSizeDelta(new Vector2(BoostTab.rect.width, height), 0.4f)
            .SetEase(Ease.OutSine);
    }

    void UpdateUserUI() {
        SetUserInfo(userInfo, PlayerManager.Instance.Username, PlayerManager.Instance.Level, PlayerManager.Instance.ExperienceProgress());
    }
    
    void OnChangedCurrency(int newValue, int oldValue, CurrencyTypes currencyType) {
        UpdateCurrency(newValue, currencyType, true);
    }

    void Btn_OnUsernameClick() {
        MenuManager.Instance.Load("Interface_UserProfile");
    }

    private void OnUserLoginOK(NodeResponse res) {
        trace("PIERRE: ---------------------- ARE WE HERE?");

        UpdateUserUI();
        ShowHeader();

        //var curMan = CurrencyManager.Instance;

        //curMan.ParseCurrencyData(res["game"]["currency"]);
    }
    //////////////////////////////////////////////////////////////////////////////////

    public void ShowHeader() {
        userInformation.DOAnchorPosY(0f, timeForShowHide).SetEase(Ease.OutQuint);

        RefreshCurrency();
    }

    private void RefreshCurrency() {
        EnumUtils.ForEach((CurrencyTypes currencyType) => {
            int amount = currencyType.GetAmount();
            UpdateCurrency(amount, currencyType, false);
        });
    }

    public void HideHeader() {
        userInformation.DOAnchorPosY(200f, timeForShowHide).SetEase(Ease.InQuint);
    }

    public void UpdateCurrency(int value, CurrencyTypes currencyType, bool isAnimateIcon) {
        CurrencyIcon icon = GetCurrencyLabelAndIcon(currencyType);

        if (icon==null || icon.label==null || icon.imageRect==null) {
            Tracer.traceError("MISSING-UI", "Unknown currency type, cannot update icon & label count for: " + currencyType);
            return;
        }

        if(icon.label.text==value.ToString()) return;

        icon.label.text = value.ToString();

        if (isAnimateIcon) {
            //Do a subtle "bump" scale-animation to indicate value recently changed
            float amplitude = 0.5f;
            Transform trans = icon.imageRect;
            Sequence anim = DOTween.Sequence();
            Vector2 scaleStart = new Vector2(2 - amplitude, amplitude);
            anim.Append( trans.DOScale(scaleStart, timeForCurrencyBump * 0.3f).SetEase(Ease.OutSine) );
            anim.Append( trans.DOScale(Vector2.one, timeForCurrencyBump).SetEase(Ease.OutElastic) );
        }
    }

    private CurrencyIcon GetCurrencyLabelAndIcon(CurrencyTypes currencyType) {
        if (!currencyIcons.ContainsKey(currencyType)) return null;
        return currencyIcons[currencyType];
    }

    public static void SetUserInfo(UserInfo userInfo, string name, int level, float progress) {
        trace("Setting User Info: " + name + ",  level: " + level + " (%" + progress + ")");

        userInfo.labelUsername.text = name;
        userInfo.labelLevel.text = level.ToString();

        userInfo.progressLevel.fillAmount = 0;
        userInfo.progressLevel.DOFillAmount( progress, 0.6f ).SetEase(Ease.OutSine);
    }

    [Serializable]
    public struct UserInfo {
        public TextMeshProUGUI labelUsername;
        public TextMeshProUGUI labelLevel;
        public Image progressLevel;
        public Transform progressTrans;
    }
}
