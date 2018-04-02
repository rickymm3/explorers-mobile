using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;
using System;
using DG.Tweening;
using TMPro;
using SimpleJSON;

public class ResearchInterface : MonoBehaviour {
    public static int NUM_SLOTS_PER_TRAYS = 4;
    //public static ResearchInterface Instance;
    
    [Header("Colors")]
    public Color clrShadowGreen = "#1e6165".ToHexColor();
    public Color clrShadowRed = "#713641".ToHexColor();
    public Color clrDarkened = "454545FF".ToHexColor(true);

    [Header("Research Trays / Slots")]
    public RectTransform[] traysHorizontalLayouts;
    public ResearchSlotInterface slotTemplate;

    TrayInfo[] _trayCosts;
    public TrayInfo[] trayCosts { get { return _trayCosts; } }

    [Header("Sprite References")]
    public Sprite sprQuestionMark;
    public Sprite sprPlus;
    public Sprite sprLockGold;
    public Sprite sprLockSilver;
    public Sprite sprButtonRed;
    public Sprite sprButtonGreen;
    public Sprite sprDefaultItem; //Demo purposes.

    [Header("Misc.")]
    public AnimationCurve easeCircularWipe;

    List<ResearchSlotInterface> _slots;
    int _numSlotsTotal;

    // Use this for initialization
    void Start () {
        //MenuManager.Instance.UIOnCampScreenPoped = true;
        //Instance = this;
        //panel.localScale = Vector2.one * 0.1f;
        //panel.DOScale(1, 0.6f).SetEase(Ease.OutBack);
        //GameManager.Instance.InResearch = true;

        _numSlotsTotal = NUM_SLOTS_PER_TRAYS * traysHorizontalLayouts.Length;
        _slots = new List<ResearchSlotInterface>(_numSlotsTotal);

        //btnClose.onClick.AddListener(Btn_Close);

        //Read global research tray costs / booster multipliers / base costs:
        int trayLimit = GlobalProps.RESEARCH_TRAY_LIMIT.GetInt();
        int slotLimit = GlobalProps.RESEARCH_SLOT_LIMIT.GetInt();
        _trayCosts = new TrayInfo[trayLimit];
        string[] costUnlock = GlobalProps.RESEARCH_UNLOCK_COST.GetString().Split("\n");
        string[] costBoosts = GlobalProps.RESEARCH_BOOSTER_BASE.GetString().Split("\n");
        string[] costMultis = GlobalProps.RESEARCH_BOOSTER_MULTIPLIER.GetString().Split("\n");
        
        
        for (int t=0; t<_trayCosts.Length; t++) {
            var trayInfo = _trayCosts[t] = new TrayInfo();
            trayInfo.costToBoostBase = CurrencyManager.ParseToCost(costBoosts[t]);
            trayInfo.costToBoostMultiplier = float.Parse(costMultis[t]);
            trayInfo.costToUnlock = new CurrencyManager.Cost[slotLimit];
            string[] costUnlockSplit = costUnlock[t].Split("=");

            if (costUnlockSplit.Length!=2) {
                Debug.LogError("Error parsing the GlobalProps.RESEARCH_UNLOCK_COST from JSON data.");
                continue;
            }

            CurrencyTypes type = costUnlockSplit[0].AsEnum<CurrencyTypes>();
            string[] costUnlockPerSlot = costUnlockSplit[1].Split(",");

            if(costUnlockPerSlot.Length != slotLimit) {
                Debug.LogError("Incorrect # of values for {0} required to unlock slots, found {1}, should be {2}".Format2(type, costUnlockPerSlot.Length, slotLimit));
                continue;
            }

            for (int s = 0; s < slotLimit; s++) {
                int amount = int.Parse(costUnlockPerSlot[s]);
                trayInfo.costToUnlock[s] = new CurrencyManager.Cost(type, amount);
                //trace(trayInfo.costToUnlock[s].ToJSONString());
            }

            //CurrencyManager.ParseToCost(costUnlock[t]);

        }
        //DataManager.globals.GetGlobalAsString(GlobalProps.);

        //Create the actual Slot UI game-objects:
        InitSlots();

        GameAPIManager.API.Research.GetAllSlots()
            .Then(jsonSlots => {
                //Now assign their states from the Server's data:
                foreach (JSONNode jsonSlot in jsonSlots) {
                    var jsonSlotGame = jsonSlot["game"];
                    int trayID = jsonSlotGame["trayID"].AsInt;
                    int slotID = jsonSlotGame["slotID"].AsInt;
                    var slot = GetSlotAt(trayID, slotID);
                    
                    if(slot==null) {
                        Debug.LogError("Cannot get <ResearchSlotInterface> at trayID {0}, slotID {1}".Format2(trayID, slotID));
                        continue;
                    }

                    GameAPIManager.API.Research.ProcessSlotData(slot, jsonSlotGame);
                }
            });
    }

    ResearchSlotInterface GetSlotAt(int trayID, int slotID) {
        return _slots.Find(slot => slot.trayID==trayID && slot.slotID==slotID);
    }

    /*private void Btn_Close() {
        if(!Close()) return;
        DoClosingTransition(panel, modalShadow);

        GameManager.Instance.InResearch = false;
    }*/

    void InitSlots() {
        int trayID = 0;

        foreach(RectTransform tray in traysHorizontalLayouts) {
            int childCount = tray.childCount;
            while(childCount>0) {
                var child = tray.GetChild(--childCount);
                if(child.gameObject==slotTemplate.gameObject) continue;

                Destroy(child.gameObject);
            }
            
            for(int i=0; i<NUM_SLOTS_PER_TRAYS; i++) {
                var slot = tray.AddClone(slotTemplate);
                slot.transform.localScale = Vector2.one;
                slot.trayID = trayID;
                slot.slotID = i;
                slot.status = ResearchSlotStatus.LOCKED;

                _slots.Add(slot);
            }

            trayID++;
        }

        slotTemplate.gameObject.SetActive(false);
    }

    public void UpdateAllSlots() {
        foreach(ResearchSlotInterface slot in _slots) {
            slot.UpdateStatus();
        }
    }

    public class TrayInfo {
        public CurrencyManager.Cost[] costToUnlock;
        public CurrencyManager.Cost costToBoostBase;
        public float costToBoostMultiplier;
    }
}
