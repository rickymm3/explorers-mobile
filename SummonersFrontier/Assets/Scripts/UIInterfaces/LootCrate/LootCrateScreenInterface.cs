using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ExtensionMethods;
using TMPro;
using RSG;
using NodeJS;

using NodePromise = RSG.IPromise<NodeJS.NodeResponse>;
public enum LootCrateScreenStatus { NORMAL, OPEN_CRATE }

public class LootCrateScreenInterface : PanelWithGetters {

    [Header("Component References")]
    public Button btnGoBack;
    public ScrollSnap_v2 scroller;
    public RectTransform content;
    public RectTransform crateContainer;
    public LayoutAnimator layoutAnim;
    public CanvasGroup canvasGroup;

    [Header("Animation Durations")]
    public float timeScrollerSlides = 1.0f;
    public float timeCrateShake = 2.0f;
    public float timeCrateExplode = 1.0f;
    public float timeItemsExplode = 0.5f;
    public float timeItemsFormCircle = 1.0f;
    public float timeItemsFlyUp = 0.5f;
    public float timeLayoutUpdate = 0.5f;
    public float timeBeforeHideSingleItem = 0.6f;
    public float timeScale = 0.2f;

    private List<Button> _crateButtons;
    private List<LootCrateItem> _crateItems;
    private LootCrateScreenStatus _status;
    private Vector2 _scrollerInitPos;
    private LootCrateBox _currentLootCrate;
    private LootCrateItem _currentLootCrateItem;
    private ItemDetailsInterface _itemDetails;
    private HeaderBarInterface _headerBar;
    private Button _identifyButton;

    

    public Rect parentRect {
        get {
            var rectTrans = (RectTransform)transform.parent;
            return rectTrans.rect;
        }
    }

    void Start() {
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 1.0f);

        //Hook Footer button click-handlers:
        btnGoBack.onClick.AddListener(BtnGoBack);

        _scrollerInitPos = scroller.transform.localPosition;

        _headerBar = (HeaderBarInterface) MenuManager.Instance.Load("Interface_HeaderBar");
        _headerBar.ShowHeader();

        signals.OnChangedCurrency += OnChangedCurrency;
        signals.OnGetEverything += OnGetEverything;

        LoadCrates();
    }

    private void OnGetEverything() {
        trace("On everything loaded...");
        LoadCrates();
    }

    void LoadCrates() {
        API.LootCrates.GetList()
            .Then(list => PrepareCrateButtons())
            .Catch(err => {
                traceError("Could not load the list of LootCrates: " + err.Message);
            });
    }

    private void OnDestroy() {
        signals.OnChangedCurrency -= OnChangedCurrency;
        signals.OnGetEverything -= OnGetEverything;
        
        PlayerManager.Instance.isReturningFromBattleResults = false;
    }

#if UNITY_EDITOR
    private void Update() {
        CheckDebugInput();
    }

    void CheckDebugInput() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            if(_itemDetails!=null) {
                //if(!_itemDetails.item.isIdentified) _itemDetails.Btn_Identify();
                _itemDetails.Btn_Close();
            }
        }
    }
#endif

    private void OnChangedCurrency(int newValue, int oldValue, CurrencyTypes currencyType) {
        if(_identifyButton == null || currencyType!=CurrencyTypes.SCROLLS_IDENTIFY) return;

        _currentLootCrateItem.UpdateIdentifiedIcon();
        _identifyButton.SetEnabledState(false);
    }
    

    private void PrepareCrateButtons() {
        trace("Preparing the Crates...");

        GameObject cratePrefab = GetPrefab("SubItems/LootCrateBox");
        
        _crateButtons = new List<Button>();

        List<LootCrate> lootCrates = PlayerManager.Instance.GetLootCrates();

        foreach (LootCrate lootCrate in lootCrates) {
            GameObject crateGo = MakeFromPrefab(cratePrefab, crateContainer);
            crateGo.transform.localScale = Vector3.one;

            LootCrateBox box = crateGo.GetComponent<LootCrateBox>();
            box.lootCrate = lootCrate;

            Button crateBtn = crateGo.GetComponentInChildren<Button>();
            _crateButtons.Add(crateBtn);

            //Hook Crate buttons click-handlers:
            crateBtn.onClick.AddListener(() => BtnOnClickCrate(crateBtn));

            //Add a CG to control the overall alpha (of Image & Text simultaneously):
            crateBtn.gameObject.AddComponent<CanvasGroup>();
        }

        scroller.RecalculateLayout();
    }

    public void SetNavButtonsEnabled(bool isEnabled) {
        scroller.btnNavNext.gameObject.SetActive(isEnabled);
        scroller.btnNavPrev.gameObject.SetActive(isEnabled);
    }

    public void BtnGoBack() {
        trace("BtnGoBack...");
        btnGoBack.interactable = false;

        btnGoBack.transform.DOScale(0, 0.5f).SetEase(Ease.InBack);
        scroller.transform.DOLocalMoveY(GetScreenSize().y*2, 0.5f * timeScale).SetEase(Ease.InSine);

        _headerBar.HideHeader();

        canvasGroup.DOFade(0, 1.0f).OnComplete(() => {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Camp");
        });
    }
    
    // CLEAN ME
    private void BtnOnClickCrate(Button crateBtn) {
        if(_status!=LootCrateScreenStatus.NORMAL) return;

        SetNavButtonsEnabled(false);

        _status = LootCrateScreenStatus.OPEN_CRATE;

        _crateItems = new List<LootCrateItem>();
        _currentLootCrate = crateBtn.GetComponentInParent<LootCrateBox>();

        Rect parentRect = this.parentRect;
        Transform btnParent = crateBtn.transform.parent;
        LootCrate lootCrate = _currentLootCrate.lootCrate;
        List<Item> items = lootCrate.GenerateItems();

        //NOTE-TO-SELF: DO call this API method! (Instead of one-by-one, batch-calls adds all generated items)
        API.Items.AddMany(items)
            .Then(res => trace("Added " + items.Count + " items server-side..."))
            .Catch( err => traceError("Something went wrong while adding newly generated items to server-side: " + err.Message));
        
        dataMan.allItemsList.AddRange(items);
        
        
    }
    
    
    public static NodePromise GenerateLootCrate(ActiveExploration activeZone, Action<LootCrate> callback = null, bool bossCrate = false, float MagicFindBoostMultiplier = 1f) {
        return GenerateLootCrate(
            (bossCrate ? activeZone.Zone.BossFight.lootTable : activeZone.Zone.LootTable),
            activeZone.Zone.MinItemLevel,
            activeZone.Zone.Variance,
            activeZone.MagicFind * MagicFindBoostMultiplier,
            activeZoneMongoID: activeZone.MongoID,
            callback: callback
        );
    }

    public static NodePromise GenerateLootCrate(string lootTableIdentity, float minItemLevel, float Variance, float magicFind, CrateTypeData lootCrateType = null, Action<LootCrate> callback = null) {
        return GenerateLootCrate(
            dataMan.lootTableDataList.GetByIdentity(lootTableIdentity),
            minItemLevel,
            Variance,
            magicFind,
            lootCrateType: lootCrateType,
            callback: callback
        );
    }

    public static NodePromise GenerateLootCrate(LootTableData lootTable, float minItemLevel, float Variance, float magicFind, int activeZoneMongoID = -1, CrateTypeData lootCrateType = null, Action<LootCrate> callback=null) {
        LootCrate lootCrate;
        if (lootCrateType == null) {
            CrateTypeData ctd = MathHelper.WeightedRandom(lootTable.crateType).Key;
            lootCrate = new LootCrate(lootTable.Identity, minItemLevel, Variance, ctd, magicFind);
        } else
            lootCrate = new LootCrate(lootTable.Identity, minItemLevel, Variance, lootCrateType, magicFind);

        lootCrate.ExplorationID = activeZoneMongoID;

        return API.LootCrates.Add(lootCrate)
            .Then( res => {
                //dataMan.allLootCratesList.Add(lootCrate);
                if (callback!=null) callback(lootCrate);
            })
            .Catch( err => {
                traceError("Could not generate a new LootCrate: " + GameAPIManager.GetErrorMessage(err));
            });
    }
    
}
