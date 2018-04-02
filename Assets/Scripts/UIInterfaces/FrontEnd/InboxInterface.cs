using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ExtensionMethods;
using TMPro;
using RSG;

public class InboxInterface : PanelWithGetters {
    
    public Image imgRefreshIcon;
    public ScrollRect messageScrollRect;
    public AutoRotate autoRotRefreshIcon;
    public RectTransform panel;
    public CanvasGroup canvasGroup;

    [Header("Buttons")]
    public Button btnClose;
    public Button btnTrash;
    public Button btnRefresh;
    public Button btnSelect;

    [Header("Inbox Section")]
    public RectTransform sectionInbox;
    public InboxItemInterface messageTemplate;
    public RectTransform messagesContainer;
    public TextMeshProUGUI txtNumNewMessages;

    [Header("Message Section")]
    public RectTransform sectionMessage;
    public TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtMessage;
    public TextMeshProUGUI txtDismiss;
    public Image imgBackground;
    public Image imgDismiss;
    public Button btnDismiss;
    public Button btnDelete;

    [Header("Sprites")]
    public Sprite spriteSelectModeOn;
    public Sprite spriteSelectModeOff;
    public Sprite spriteMsgTypeGeneric;
    public Sprite spriteMsgTypeLootCrate;
    public Sprite spriteMsgTypeCurrency;

    List<InboxItemInterface> _messages = new List<InboxItemInterface>();
    InboxItemInterface _currentMessage;

    Image _spriteSelectMode;

    bool _isInited = false;
    bool _isSelectMode = false;
    public bool isSelectMode {
        get { return _isSelectMode; }
        set { _isSelectMode = value; UpdateUI(); }
    }

    bool _isReadingMessage = false;
    public bool isReadingMessage {
        get { return _isReadingMessage; }
        set { _isReadingMessage = value; UpdateUI(); }
    }

    public List<InboxItemInterface> selectedMessages {
        get { return _messages.FindAll(m => m.isSelected); }
    }

    //////////////////////////////////////////////////////////////////////////

    void Start() {
        btnClose.onClick.AddListener(Btn_Close);
        btnTrash.onClick.AddListener(Btn_Trash);
        btnRefresh.onClick.AddListener(Btn_Refresh);
        btnSelect.onClick.AddListener(Btn_Select);
        btnDismiss.onClick.AddListener(Btn_Dismiss);
        btnDelete.onClick.AddListener(Btn_Delete);
        MenuManager.Instance.UIOnCampScreenPoped = true;

        _spriteSelectMode = btnSelect.GetComponent<Image>();

        autoRotRefreshIcon.enabled = false;

        messageTemplate.gameObject.SetActive(false);

        sectionInbox.gameObject.SetActive(true);
        sectionMessage.gameObject.SetActive(true);

        SetInitPositions(sectionInbox, sectionMessage);

        UpdateUI();

        StartCoroutine(__FirstInboxCheck());

        _isInited = true;
    }

    IEnumerator __FirstInboxCheck() {
        int attempts = 3;

        while (!API.isLoggedIn) {
            if ((--attempts) < 0) {
                traceError("Tried waiting a few seconds before loading Inbox. Can't login!");
                yield break;
            } else {
                yield return new WaitForSeconds(2f);
            }
        }

        RefreshInbox();
    }

    //////////////////////////////////////////////////////////////////////////

    void Btn_Close() {
        if(!Close()) return;

        MenuManager.Instance.UIOnCampScreenPoped = false;
        trace("Closing this panel.");
    }

    void Btn_Trash() {
        if (!_isSelectMode) return;

        trace("Trash the selected messages.");
    }

    void Btn_Refresh() {
        RefreshInbox();
    }

    void Btn_Select() {
        isSelectMode = !isSelectMode;
    }

    void Btn_Dismiss() {
        if(_currentMessage==null) {
            traceError("A Message should be opened before calling the DISMISS button.");
            return;
        }

        if(!_currentMessage.message.receipt.isClaimed) { 
            switch(_currentMessage.message.messageType) {
                case InboxMessageType.CURRENCY_REWARD: ClaimCurrency(_currentMessage.message); return;
                case InboxMessageType.LOOTCRATE_REWARD: ClaimLootCrate(_currentMessage.message); return;
                default: break;
            }
        }

        isReadingMessage = false;
    }

    void Btn_Delete() {
        if(!_currentMessage) return;

        SetBusy(true);

        API.Messages.DeleteMessage(_currentMessage.message)
            .Then( msg => {
                trace("Message Deleted Successfully: ");
                RefreshInbox();
            });
    }

    //////////////////////////////////////////////////////////////////////////

    public override void CloseTransition() {
        DoClosingTransition(panel);
    }

    public void ReadMessage(InboxItemInterface inboxItemInterface) {
        SetBusy(true);

        _currentMessage = inboxItemInterface;
        InboxMessageData message = _currentMessage.message;

        message.receipt.isRead = true;
        
        API.Messages.ReadMessage(message)
            .Then(msg => {
                SetBusy(false);
                inboxItemInterface.UpdateUI();
                OnReadMessageComplete(msg);
            })
            .Catch(err => {
                SetBusy(false);
                traceError("Could not read the Message #{0}:\n".Format2(message.MongoID) + err.Message + "\n" + err.StackTrace);
            });
    }

    void OnReadMessageComplete(InboxMessageData msg) {
        isReadingMessage = true;

        txtDismiss.text = "CLOSE";

        if(msg.messageType==InboxMessageType.CURRENCY_REWARD) {
            txtMessage.text = msg.message.Format2(msg.currencyAmount, msg.currencyType);
            txtTitle.text = msg.title.Format2(msg.currencyAmount, msg.currencyType);
        } else {
            txtMessage.text = msg.message;
            txtTitle.text = msg.title;
        }

        if (!msg.receipt.isClaimed) {
            txtDismiss.text = "CLAIM\nREWARD";
        } else {
            txtDismiss.text = "ALREADY\nCLAIMED";
        }
    }

    //////////////////////////////////////////////////////////////////////////

    void UpdateUI() {
        btnTrash.interactable = _isSelectMode;

        _spriteSelectMode.sprite = _isSelectMode ? spriteSelectModeOn : spriteSelectModeOff;

        if(canvasGroup.interactable) { 
            int numUnread = 0;
            foreach (InboxItemInterface msg in _messages) {
                msg.btnCheckbox.transform.DOScale(_isSelectMode ? 1 : 0, 0.3f);
                msg.isSelected = false;
                if (!msg.message.isRead) numUnread++;
            }

            txtNumNewMessages.text = (numUnread == 0 ? "NO" : numUnread.ToString()) + " NEW MESSAGE" + (numUnread != 1 ? "S" : "");
        } else {
            txtNumNewMessages.text = "...";
        }

        //Tween to the correct section:
        float width = sectionInbox.rect.width;
        float time = _isInited ? 0.5f : 0f;

        if (_isReadingMessage) {
            MoveSection(sectionInbox, -width, time);
            MoveSection(sectionMessage, 0, time);
        } else {
            MoveSection(sectionInbox, 0, time);
            MoveSection(sectionMessage, width, time);

            _currentMessage = null;
        }
    }

    void MoveSection(RectTransform section, float xPos, float time = 0.5f) {
        float initX = GetInitPosition(section).x;
        section.DOLocalMoveX(initX + xPos, time).SetEase(Ease.OutSine);
    }

    void RefreshInbox() {
        StartCoroutine(__RefreshInbox());
    }

    void SetBusy(bool isBusy) {
        canvasGroup.alpha = isBusy ? 0.5f : 1;
        canvasGroup.interactable = !isBusy;
    }

    IEnumerator __RefreshInbox() {
        if(autoRotRefreshIcon.enabled) yield break;
        SetBusy(true);
        autoRotRefreshIcon.enabled = true;
        isReadingMessage = false;

        yield return imgRefreshIcon.DOFade(0.5f, 0.2f).WaitForCompletion();

        bool isReady = false;

        CleanupPreviousMessages();
        isSelectMode = false;

        if (API.isLoggedIn) {
            API.Messages.CheckInbox()
                .Then(messages => {
                    isReady = true;
                    
                    foreach (InboxMessageData msgData in messages) {
                        var dup = messagesContainer.gameObject.AddClone( messageTemplate );
                        dup.gameObject.SetActive(true);
                        dup.transform.localScale = Vector2.one;

                        dup.Init( this, msgData );

                        _messages.Add(dup);
                    }
                })
                .Catch(err => {
                    isReady = true;
                });

            while(!isReady) yield return new WaitForSeconds(0.5f);
        }

        SetBusy(false);
        autoRotRefreshIcon.enabled = false;
        imgRefreshIcon.DOFade(1.0f, 0.2f);
        imgRefreshIcon.transform.localRotation = Quaternion.Euler(0,0,0);
        UpdateUI();
    }

    void CleanupPreviousMessages() {
        foreach(InboxItemInterface msg in _messages) {
            Destroy(msg.gameObject);
        }

        _messages.Clear();

        //Scroll back to top on list-clear:
        messageScrollRect.verticalNormalizedPosition = 1;
    }

    void ClaimCurrency(InboxMessageData msg) {
        if (!msg.rewardsData.Exists()) {
            traceError("Currency Data does not exists.");
            isReadingMessage = false;
            return;
        }
        
        SetBusy(true);

        API.Messages.ClaimReward(msg)
            .Then(o => {
                if(msg.rewardBoost!= null) {
                    return API.Users.BoostAddCurrency(msg.rewardBoost);
                }

                return API.Currency.AddCurrency(msg.rewardCurrency);
            })
            .Then(res => {
                trace("Claimed? " + msg.receipt.isClaimed);
                SetBusy(false);
                isReadingMessage = false;

                trace(res.pretty);
            })
            .Catch(err => {
                traceError("Error claiming/adding currency!");
                traceError(err);
                traceError(err.StackTrace);
                SetBusy(false);
            });
    }

    void ClaimLootCrate(InboxMessageData msg) {
        if(!msg.rewardsData.Exists()) {
            traceError("Crate Data does not exists.");
            isReadingMessage = false;
            return;
        }

        SetBusy(true);

        API.Messages.ClaimReward(msg)
            .Then(o => {
                return LootCrateScreenInterface.GenerateLootCrate(
                    msg.lootCrateData["lootTableIdentity"],
                    float.Parse(msg.lootCrateData["itemLevel"]),
                    float.Parse(msg.lootCrateData["variance"]),
                    float.Parse(msg.lootCrateData["magicFind"]),
                    null //lootCrateData["crateTypeIdentity"]
                );
            })
            .Then(res => {
                trace("Claimed? " + msg.receipt.isClaimed);
                SetBusy(false);
                isReadingMessage = false;

                trace(res.pretty);
            })
            .Catch(err => {
                traceError("Error claiming/generating a LootCrate!");
                traceError(err);
                traceError(err.StackTrace);
                SetBusy(false);
            });
    }
}
