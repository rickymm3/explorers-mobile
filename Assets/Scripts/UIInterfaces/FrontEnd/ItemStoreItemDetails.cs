using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using ExtensionMethods;

public enum StoreTransactionType { Buy, Sell }

public class ItemStoreItemDetails : Tracer {

    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI cost;
    public TextMeshProUGUI sellValue;
    public Image itemIcon;
    public Image itemBackdrop;
    public Image itemQuestion;

    public GameObject BtnBuy;
    public GameObject BtnSell;

    public GameObject ItemDetailsArea;
    public GameObject SoldOutArea;

    Item _item;
    int _index;
    int _seed;
    StoreTransactionType _transactionType;
    Button _detailsSell;
    Button _detailsIdentify;
    ItemDetailsInterface _details;


    public void Initialize(int index, int seed, Item item, bool soldOut = false, StoreTransactionType transactionType = StoreTransactionType.Buy) {
        if (soldOut) {
            SoldOutArea.SetActive(true);
            ItemDetailsArea.SetActive(false);
            return;
        }

        SoldOutArea.SetActive(false);
        ItemDetailsArea.SetActive(true);

        this._index = index;
        this._item = item;
        this._seed = seed;
        this._transactionType = transactionType;

        UpdateItemInformation();
        UpdateActiveButtons();
    }

    private void UpdateItemInformation() {
        if (_item.isIdentified) {
            title.text = _item.Name;
            description.text = _item.data.Description;
            cost.text = _item.Value.ToString();
            sellValue.text = _item.SellValue.ToString();
            itemIcon.sprite = _item.data.LoadSprite();
            itemBackdrop.color = _item.GetBackgroundColor();
            itemQuestion.gameObject.SetActive(false);
        } else {
            title.text = "UNIDENTIFIED " + _item.data.EquipType; //_item.Name;
            description.text = "???"; //_item.data.Description;
            cost.text = "???"; //_item.Value.ToString();
            sellValue.text = _item.SellValue.ToString();
            itemIcon.sprite = _item.data.LoadSprite();
            itemBackdrop.color = _item.GetBackgroundColor();
            itemQuestion.gameObject.SetActive(true);
        }
    }

    private void UpdateActiveButtons() {
        if (_transactionType == StoreTransactionType.Sell) {
            BtnBuy.SetActive(false);
            BtnSell.SetActive(true);
        } else {
            BtnBuy.SetActive(true);
            BtnSell.SetActive(false);
        }
    }

    public void Btn_Buy() {
        if(_item==null || !CurrencyTypes.GOLD.HasEnough(_item.Value)) {
            BtnBuy.transform.DOKill();
            AudioManager.Instance.Play(SFX_UI.Invalid);
            TimelineTween.ShakeError(BtnBuy);
            return;
        }

        // Do buy logic here
        print("buying [" + _index + "] " + _item.Name);

        if (CheatManager.ALWAYS_CREATE_AS_UNIDENTIFIED) {
            trace("CHEATS: ALWAYS_CREATE_AS_UNIDENTIFIED is enabled, so all purchased items will be unidentified.");
            AudioManager.Instance.Play(SFX_UI.ShardsChing);
            _item.isIdentified = false;
        }

        GameAPIManager.API.Shop.BuyItem(_item, _index, _seed, CurrencyTypes.GOLD, _item.Value, OnItemBought)
            .Then(res => {
                if (_details != null)
                    _details.Close();
            })
            .Catch( error => traceError(error) );
    }

    void OnItemBought(int index, int seed) {
        AudioManager.Instance.Play(SFX_UI.ShardsChing);
        
        if (_item.data.Type != ItemType.Currency) {
            DataManager.Instance.allItemsList.Add(_item);
        } else {
            ItemCurrency itemCurrency = (ItemCurrency) _item.data;
            trace("You should of gained: " + itemCurrency.Reward + " " + itemCurrency.CurrencyType);
        }

        SoldOutArea.SetActive(true);
        ItemDetailsArea.SetActive(false);
    }

    public void Btn_Sell() {
        if (_item == null) {
            traceError("Cannot SELL when the item == null");
            return;
        }

        trace("Selling " + _item.Name + " for GOLD = " + _item.SellValue);
        
        GameAPIManager.API.Shop.SellItems(_item, CurrencyTypes.GOLD, _item.SellValue)
            .Then(res => {
                trace("Btn_Sell on ItemStore OK");
                
                OnSoldComplete();
            })
            .Catch(error => traceError(error));
        
    }

    void OnSoldComplete() {
        AudioManager.Instance.Play(SFX_UI.Coin);

        // remove from the list
        DataManager.Instance.allItemsList.Remove(_item);

        AnimateOut();
    }

    public void AnimateOut() {
        this.transform.DOScale(0, 0.3f)
            .SetEase(Ease.InSine)
            .OnComplete(DisableMe);
    }

    void DisableMe() {
        gameObject.SetActive(false);
    }

    public void Btn_Details() {
        if(_item.isResearched) {
            ConfirmYesNoInterface.Ask("Researched Item", "Cannot bring up details of items in research.", "OK");
            return;
        }

        _details = ItemDetailsInterface.Open(_item);
        _details.btnEnchant.gameObject.SetActive(false);

        //_item.isIdentified = false;
        trace("Value " + _item.Value +
            ", SellValue " + _item.SellValue +
            ", ItemTier " + _item.data.Tier +
            ", ItemLevel " + _item.ItemLevel +
            ", Item Quality " + _item.QualitySeed
        );

        switch(_transactionType) {
            case StoreTransactionType.Buy:
                _details.AddButton("Buy\n<color={0}>(-{1} gold)</color>".Format2(ColorConstants.HTML_GOLD, _item.Value), Btn_Buy);
                break;

            case StoreTransactionType.Sell:
                _detailsSell = _details.AddButton(GetSellValueButtonLabel(), Btn_Sell);
                break;
        }
    }

    string GetSellValueButtonLabel() {
        return "Sell\n<color={0}>(+{1} gold)</color>".Format2(ColorConstants.HTML_GOLD, _item.SellValue);
    }
}
