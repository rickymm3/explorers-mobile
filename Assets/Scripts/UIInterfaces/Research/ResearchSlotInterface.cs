using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;
using RSG;

public enum ResearchSlotStatus { NULL, LOCKED, UNLOCKED, BUSY, COMPLETED, RESET }

public class ResearchSlotInterface : Tracer {
    public static int TIME_BASE = 30;
    public static int TIME_MULTIPLIER = 30;

    public Image imgItem;
    public Image imgStatus;

    [Header("UI Reference")]
    public ResearchInterface reInterface;

    [Header("Bottom Button")]
    public Button btnButton;
    public Image imgButton;
    public TextMeshProUGUI txtButton;
    
    [Header("Circular Background / Progress Bar")]
    public RectTransform rectCircular;
    public Image imgCircularFill;
    public RectTransform rectBottomBar;
    public RectTransform rectBottomProgress;
    public TextMeshProUGUI txtBottomProgress;

    Button _btn;
    Item _item;
    Sprite _sprite;
    double _dateDiff;
    bool hasErrors = false;

    public DateTime dateCompleted;
    public DateTime dateUnlocked;
    public DateTime dateStarted;
    public DateTime dateEnd;

    float _timeAnim = 0.8f;
    ResearchUnidentifiedItems _panelUnidentifiedItems;

    Vector2 _posStatus;

    ResearchSlotStatus _status;
    public ResearchSlotStatus status {
        get { return _status; }
        set { _status = value; UpdateStatus(); }
    }

    public int trayID = -1;
    public int slotID = -1;
    public bool isChangingStatus = false;

    public Item item {
        get { return _item; }
        set {
            _item = value;
            _sprite = _item.data.LoadSprite();
            
            UpdateStatus();
        }
    }

    public ResearchInterface.TrayInfo trayInfo { get { return INTERFACE.trayCosts[trayID]; } }
    public ResearchInterface INTERFACE { get { return reInterface; } }
    public static CurrencyManager CURRENCY { get { return CurrencyManager.Instance; } }
    public static GameAPIManager.ResearchAPI RESEARCH_API { get { return GameAPIManager.API.Research; } }

    void Start () {
        rectBottomProgress.gameObject.SetActive(false);

        _posStatus = imgStatus.GetRect().anchoredPosition;

        _btn = this.GetComponent<Button>();
        _btn.onClick.AddListener(Btn_OnClick);
    }

    IPromise<ResearchSlotInterface> ChangeStatus(ResearchSlotStatus status, CurrencyTypes currency=CurrencyTypes.NONE, int amount=0, Item item=null) {
        isChangingStatus = true;
        return RESEARCH_API.ChangeStatus(this, status, currency, amount, item)
            .Then(slot => {
                isChangingStatus = false;
            })
            .Catch(err => {
                isChangingStatus = false;
                hasErrors = true;
                AudioManager.Instance.Play(SFX_UI.Invalid);
                TimelineTween.ShakeError(this.gameObject);
            });
    }

    void Update() {
        if (_status != ResearchSlotStatus.BUSY || hasErrors) return;

        this.btnButton.interactable = !isChangingStatus;
        if (isChangingStatus) return;

        DateTime now = DateTime.Now;
        TimeSpan dateDiff = dateEnd - now;
        float progressLeft = (float)(dateDiff.TotalSeconds / _dateDiff);

        if (progressLeft < 0) {
            ChangeStatus(ResearchSlotStatus.COMPLETED)
                .Then( slot => {
                    trace("Slot should now be set to COMPLETED!");
                    trace(slot.status);
                })
                .Catch( err => {
                    traceError("Could not mark the slot as COMPLETED: " + GameAPIManager.GetErrorMessage(err));
                });
        } else {
            txtBottomProgress.text = dateDiff.TotalSeconds.ToHHMMSS(isMonospaceHTML: true);
            imgCircularFill.fillAmount = progressLeft;
        }
    }

    void Btn_OnClick() {
        btnButton.transform.DOKill();
        FXPulse(btnButton.transform, 1);

        switch (_status) {
            case ResearchSlotStatus.UNLOCKED: Btn_UnlockedSelectItem(); break;
            case ResearchSlotStatus.BUSY: Btn_BusySelectBooster(); break;
            case ResearchSlotStatus.LOCKED: Btn_Unlock(); break;
            case ResearchSlotStatus.COMPLETED: Btn_CompletedIdentifying(); break;
            default:
                traceError("Unhandled button click status: " + _status);
                TimelineTween.ShakeError(btnButton.gameObject);
                return;
        }
    }

    private void Btn_UnlockedSelectItem() {
        _panelUnidentifiedItems = (ResearchUnidentifiedItems) MenuManager.Instance.Push("Interface_ResearchUnidentifiedItems");
        _panelUnidentifiedItems.OnSelectedItem += OnItemSelected;
    }

    void OnItemSelected(Item item) {
        ChangeStatus(ResearchSlotStatus.BUSY, item: item) //CurrencyTypes.NONE, 1
            .Then(slot => {
                trace("AFTER: " + CurrencyTypes.SCROLLS_IDENTIFY.GetAmount());
            })
            .Catch( err => {
                trace("Could not start researching the item: " + item.MongoID);
            });
    }

    private void Btn_BusySelectBooster() {
        var costBase = trayInfo.costToBoostBase;
        float costMultiplier = trayInfo.costToBoostMultiplier;
        int minutesLeft = (int) (dateEnd - DateTime.Now).TotalMinutes;

        var gemsType = costBase.type;
        int gemsCost = costBase.amount + Mathf.CeilToInt(minutesLeft * costMultiplier);
        int gemsLeft = gemsType.GetAmount();

        //int cost
        ConfirmYesNoInterface.Ask(
            "Instant Identify",
            "How would you".JoinNewLines(
                "like to instantly",
                "identify this item?\n",
                "<size=+10><color=#4286f4>{0} {1}</color></size>",
                "<size=-3>({2} remaining)</size>",
                "-or-",
                "<size=+10><color=#f4eb41>1 Identify Scrolls</color></size>",
                "<size=-3>({3} remaining)</size>"
                ).Format2(gemsCost, gemsType, gemsLeft, CurrencyTypes.SCROLLS_IDENTIFY.GetAmount()),
            "USE\n" + gemsType, "USE\nSCROLLS"
            ).Then(answer => {
                if(answer.Contains(gemsType.ToString())) {
                    AudioManager.Instance.Play(SFX_UI.ShardsChing);
                    return ChangeStatus(ResearchSlotStatus.COMPLETED, gemsType, gemsCost);
                }

                if(!CurrencyTypes.SCROLLS_IDENTIFY.HasEnough(1)) {
                    TimelineTween.ShakeError(this.gameObject);
                    traceError("Unsufficient Identify Scrolls.");
                    return null;
                }

                AudioManager.Instance.Play(SFX_UI.PageFlip);

                return ChangeStatus(ResearchSlotStatus.COMPLETED, CurrencyTypes.SCROLLS_IDENTIFY, 1);
            }).Then(slot => {
                if(slot==null) return;
                trace("Slot should now be set to COMPLETED!");
                trace(slot.status);

                Btn_CompletedIdentifying();
            })
                .Catch(err => {
                    traceError("Could not mark the slot as COMPLETED: " + GameAPIManager.GetErrorMessage(err));
                });
    }

    private void Btn_Unlock() {
        var cost = trayInfo.costToUnlock[this.slotID];

        if (!isUnlockable()) {
            ConfirmYesNoInterface.Ask("Research Lab", "You need\n<color={0}><size=+20>{2} {1}</size></color>\nto unlock this slot.".Format2(ColorConstants.HTML_GOLD, cost.type, cost.amount), "OK");
            return;
        }

        ConfirmYesNoInterface.Ask("Research Lab", "Would you like\nto unlock this\nResearch Slot?\n<size=+20><color={0}>{2} {1}</color></size>".Format2(ColorConstants.HTML_GOLD, cost.type, cost.amount))
            .Then( answer => {
                if (answer == "NO") {
                    TimelineTween.ShakeError(this.gameObject);
                    return;
                }

                ChangeStatus(ResearchSlotStatus.UNLOCKED, cost.type, cost.amount)
                .Then(slot => {
                    FXPulse(imgStatus.transform);
                    INTERFACE.UpdateAllSlots();
                });
            });
    }

    private void Btn_CompletedIdentifying() {
        trace("Claim the completed identified item (API CALL to mark item as identified & free up this slot...)");
        var currentItem = item;

        if (currentItem == null) {
            traceError("There MUST be an item completed before you can reset this slot!");
            return;
        }

        ChangeStatus(ResearchSlotStatus.RESET)
            .Then(slot => {
                currentItem.isIdentified = true;
                currentItem.isResearched = false;

                ItemDetailsInterface.Open(currentItem);
                //var match = DataManager.Instance.allItemsList.Find(i => i.MongoID == currentItem.MongoID);

                FXPulse(imgStatus.transform);
            });
    }

    ////////////////////////////////////////////////////////////////////

    public void CalculateTimeDiff() {
        //Add 1 second to delay and guarantee Server accepts request upon Completion:
        _dateDiff = (dateEnd - dateStarted).TotalSeconds + 1;
    }

    public void UpdateStatus() {
        //First, Apply status for majority of slot's states:
        imgItem.sprite = null;
        imgStatus.DOColor(Color.white, _timeAnim);
        imgItem.DOColor( INTERFACE.clrDarkened, _timeAnim );
        imgButton.sprite = INTERFACE.sprButtonGreen;

        bool isShowingButton = true;
        bool isCircularShown = false;

        //Now apply the state-specific changes:
        switch (_status) {
            case ResearchSlotStatus.LOCKED:
                if (isUnlockable()) {
                    txtButton.text = "UNLOCK";
                    imgStatus.sprite = INTERFACE.sprLockGold;
                } else {
                    txtButton.text = "LOCKED";
                    imgButton.sprite = INTERFACE.sprButtonRed;
                    imgStatus.sprite = INTERFACE.sprLockSilver;
                }
                
                imgStatus.transform.localScale = Vector2.one;
                break;

            case ResearchSlotStatus.UNLOCKED:
                txtButton.text = "IDENTIFY";
                imgStatus.sprite = INTERFACE.sprPlus;
                imgStatus.transform
                    .DOScale(1, _timeAnim)
                    .SetEase(Ease.OutBack);
                break;

            case ResearchSlotStatus.BUSY:
                isCircularShown = true;
                imgItem.sprite = _sprite ? _sprite : INTERFACE.sprDefaultItem;
                imgStatus.sprite = INTERFACE.sprQuestionMark;
                imgStatus.transform.localScale = Vector2.one;
                isShowingButton = false;

                rectCircular.localScale = Vector2.one * 2;
                rectCircular
                    .DOScale(1, _timeAnim)
                    .SetEase(Ease.OutBack);

                break;

            case ResearchSlotStatus.COMPLETED:
                isCircularShown = true;
                txtButton.text = "COMPLETED";
                FXPulse(txtButton.transform);

                imgItem.sprite = _sprite ? _sprite : INTERFACE.sprDefaultItem;

                imgCircularFill.fillAmount = 0;
                rectCircular
                    .DOScale(2, _timeAnim)
                    .SetEase(INTERFACE.easeCircularWipe);

                imgStatus.transform
                    .DOScale(0, _timeAnim)
                    .SetEase(Ease.InBack);

                imgItem.DOColor(Color.white, _timeAnim).OnComplete(() => {
                    FXPulse(imgItem.transform);
                });

                break;

            //DEFAULT really only exists for error-catching:
            default:
                imgStatus.transform.localScale = Vector2.one;
                imgStatus.sprite = INTERFACE.sprLockSilver;
                imgStatus.DOColor(Color.red, _timeAnim);
                break;
        }

        btnButton.gameObject.SetActive(isShowingButton);
        rectBottomBar.gameObject.SetActive(!isShowingButton);
        rectCircular.gameObject.SetActive(isCircularShown);
    }

    bool isUnlockable() {
        var trayInfo = INTERFACE.trayCosts[trayID];
        var cost = trayInfo.costToUnlock[slotID];
        int currency = cost.type.GetAmount();

        return currency >= cost.amount;
    }

    Tweener FXPulse(Transform trans, float timeMultiplier=1.8f) {
        trans.localScale = Vector2.one * 0.8f;
        return trans
            .DOScale(1, _timeAnim * timeMultiplier)
            .SetEase(Ease.OutElastic);
    }
}
