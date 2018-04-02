using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class InboxItemInterface : Tracer {

    public InboxMessageData message;
    public Button btnSelf;
    public Image imgMsgStatus;
    public Image imgMsgType;
    public TextMeshProUGUI txtTitle;
    public Image imgCheckbox;
    public Button btnCheckbox;
    
    bool _isSelected = false;
    InboxInterface _inboxInterface;

    public bool isSelected {
        get { return _isSelected; }
        set {
            _isSelected = value;
            
            UpdateSelectUI(btnCheckbox.transform.localScale.x!=1 ? 0 : 0.3f);
        }
    }

    void Start() {
        btnSelf.onClick.AddListener(Btn_OpenMessage);
        btnCheckbox.onClick.AddListener(Btn_ToggleSelect);

        btnCheckbox.transform.localScale = Vector2.zero;

        UpdateSelectUI(0);
    }

    private void Btn_ToggleSelect() {
        isSelected = !isSelected;
    }

    public void Init(InboxInterface inboxInterface, InboxMessageData message) {
        this.message = message;
        this._inboxInterface = inboxInterface;

        UpdateUI();
    }

    public void Btn_OpenMessage() {
        _inboxInterface.ReadMessage(this);
    }

    private void UpdateSelectUI(float timeAnim = 0.3f) {
        var t = imgCheckbox.transform;

        if (_isSelected) {
            t.DOScale(1, timeAnim).SetEase(Ease.OutBack);
        } else {
            t.DOScale(0, timeAnim).SetEase(Ease.InBack);
        }
    }

    public void UpdateUI() {
        txtTitle.text = message.title.Format2(message.currencyAmount, message.currencyType);

        trace(message.messageType);
        switch (message.messageType) {
            case InboxMessageType.GENERIC_MESSAGE: imgMsgType.sprite = _inboxInterface.spriteMsgTypeGeneric; break;
            case InboxMessageType.LOOTCRATE_REWARD: imgMsgType.sprite = _inboxInterface.spriteMsgTypeLootCrate; break;
            case InboxMessageType.CURRENCY_REWARD: imgMsgType.sprite = _inboxInterface.spriteMsgTypeCurrency; break;
        }

        imgMsgStatus.gameObject.SetActive(!message.isRead);
    }
}

///////////////////////////////////////////////////////////////////////

[Serializable]
public class InboxMessageData : IMongoData {
    int _MongoID;
    public string title;
    public string message;
    public bool isForEveryone = false;
    public bool hasReceipt = false;
    public int sentFromID = -1;
    public InboxMessageType messageType;
    public string dateSentStr;
    public string dateExpiresStr;
    public string rewardsData;
    public InboxReceiptData receipt;
    public CurrencyManager.Cost rewardCurrency;
    public CurrencyManager.BoostCost rewardBoost;
    public string currencyType = "";
    public int currencyAmount = 0;
    public Dictionary<string, string> lootCrateData;

    public DateTime dateSent { get { return dateSentStr.FromNodeDateTime(); } }
    public DateTime dateExpires { get { return dateExpiresStr.FromNodeDateTime(); } }

    public int MongoID {
        get { return _MongoID; }
        set { _MongoID = value; }
    }

    public string DebugID {
        get { return "[Message #{0} title=\"{1}\"]".Format2(_MongoID, title); }
    }

    public bool isRead {
        get { return hasReceipt && receipt.isRead; }
    }


}

[Serializable]
public class InboxReceiptData {
    public int MongoID;
    public bool isRead = false;
    public bool isClaimed = false;
    public bool isDeleted = false;

    public string dateReadStr;
    public string dateClaimedStr;
    public string dateDeletedStr;

    public DateTime dateRead { get { return dateReadStr.FromNodeDateTime(); } }
    public DateTime dateClaimed { get { return dateClaimedStr.FromNodeDateTime(); } }
    public DateTime dateDeleted { get { return dateDeletedStr.FromNodeDateTime(); } }
}