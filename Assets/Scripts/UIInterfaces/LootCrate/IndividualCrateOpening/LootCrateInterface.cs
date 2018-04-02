using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class LootCrateInterface : PanelWithGetters {

    LootCrate crate;
    private LootCrateScreenStatus _status = LootCrateScreenStatus.OPEN_CRATE;
    private List<LootCrateItem> _crateItems;
    private ItemDetailsInterface _itemDetails;
    private LootCrateItem _currentLootCrateItem;
    private Button _identifyButton;
    private HeaderBarInterface _headerBar;
    private LayoutAnimator layoutAnim;

    [Header("Component References")]
    public Button btnGoBack;
    public Image crateBtn;
    public Image backgroundShadow;

    //[Header("Sequence Controls")]
    float timeScrollerSlides = 1.0f;
    float timeCrateShake = 2.0f;
    float timeCrateExplode = 0.8f;
    float timeItemsExplode = 0.6f;
    float timeItemsFormCircle = 1.0f;
    float timeItemsFlyUp = 0.5f;
    float timeLayoutUpdate = 0.5f;
    float timeBeforeHideSingleItem = 0.6f;
    float speedScale = 0.5f;

    System.Action callbackOnComplete;

    bool noMenuManagerRef = false;

    public Rect parentRect {
        get {
            var rectTrans = (RectTransform) transform.parent;
            return rectTrans.rect;
        }
    }

    void Start () {
        // Get the attached components we need
        layoutAnim = GetComponent<LayoutAnimator>();

        // Init lists
        _crateItems = new List<LootCrateItem>();
        
        _headerBar = (HeaderBarInterface) MenuManager.Instance.Load("Interface_HeaderBar");
        _headerBar.ShowHeader();

        signals.OnChangedCurrency += OnChangedCurrency;
        signals.OnItemRemoved += OnItemRemoved;
    }

    private void OnDestroy() {
        signals.OnChangedCurrency -= OnChangedCurrency;
        signals.OnItemRemoved -= OnItemRemoved;

        PlayerManager.Instance.isReturningFromBattleResults = false;
    }

    public void Initialize(LootCrate crate, System.Action callback = null, System.Action callbackOnComplete = null, bool fromStory = false) {
        this.crate = crate;
        List<Item> items = crate.GenerateItems();
        this.callbackOnComplete = callbackOnComplete;
        noMenuManagerRef = fromStory;

        Debug.Log("Adding " + items.Count + " items.");
        /*
        foreach(Item i in items) {
            Debug.Log(" --- --- [Item] " + i.Name);
        }*/

        // NOTE-TO-SELF: DO call this API method! (Instead of one-by-one, batch-calls adds all generated items)
        API.Items.AddMany(items)
            .Catch(err => traceError("Something went wrong while adding newly generated items to server-side: " + err.Message));
        //.Then(res => trace("Added " + items.Count + " items server-side..."))

        DataManager.Instance.allItemsList.AddRange(items);

        if (!fromStory)
            RemoveCrateFromPlayerData(callback);
        
        // Crate Sequence
        TimelineTween.Create(
            (next) => {
                AudioManager.Instance.Play(SFX_UI.Earthquake);

                //btnGoBack.transform.DOScale(Vector2.zero, 0.5f * speedScale);
                btnGoBack.GetComponent<RectTransform>().DOAnchorPosY(0f, 0.5f * speedScale);

                crateBtn.transform.SetParent(this.transform);

                // Background Shadow here
                backgroundShadow.DOColor(new Color(0f, 0f, 0f, 0.7f), timeScrollerSlides * speedScale);

                crateBtn.transform.TweenMoveByY(50, timeScrollerSlides * speedScale)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() => next());
            },

            (next) => {
                // If on crate screen reset the crate list positions

                crateBtn.transform
                    .DOShakePosition(timeCrateShake * speedScale, 10, 30, 90, false, false)
                    .OnComplete(() => next());
            },

            (next) => {

                AudioManager.Instance.Stop(SFX_UI.Earthquake);
                AudioManager.Instance.Play(SFX_UI.Explosion);

                GameObject itemPrefab = GetPrefab("SubItems/LootCrateItem");

                var canvasSize = GetScreenSize();
                float itemSize = canvasSize.x / 3f;

                for (int i = 0; i < items.Count; i++) {
                    Item itemData = items[i];
                    GameObject itemGO = GameObject.Instantiate(itemPrefab);
                    LootCrateItem item = itemGO.GetComponent<LootCrateItem>();
                    Button itemBtn = itemGO.GetComponent<Button>();
                    TextMeshProUGUI itemLabel = itemGO.GetComponentInChildren<TextMeshProUGUI>();
                    RectTransform itemTrans = (RectTransform) itemGO.transform;

                    itemTrans.SetParent(this.transform);
                    itemTrans.localPosition = Vector2.zero;
                    itemTrans.SetWidthAndHeight(itemSize);

                    item.itemData = itemData;
                    item.UpdateIdentifiedIcon();

                    _crateItems.Add(item);

                    itemBtn.onClick.AddListener(() => BtnOnItemClick(item, fromStory));
                }

                LayoutUpdate();

                TimelineTween.Scatter(0.08f, _crateItems, (item, id) => {
                    return TimelineTween.Pulsate(item.transform, 1.3f, timeItemsExplode * speedScale);
                });

                Color c = crateBtn.GetComponent<Image>().color;
                c.a = 0;
                crateBtn.GetComponent<Image>().DOColor(c, 0.3f).OnComplete(() => {
                        Destroy(crateBtn.gameObject);
                    });
                /*CanvasGroup cg = crateBtn.GetComponent<CanvasGroup>();
                cg.DOFade(0, 0.3f)
                    .OnComplete(() => {
                        Destroy(crateBtn.gameObject);
                    });*/

                crateBtn.transform
                    .DOScale(2.0f, timeCrateExplode * speedScale)
                    .SetEase(Ease.OutQuint);

                this.Wait(timeItemsExplode * 2.0f * speedScale, next);
            },

            (next) => {
                TimelineTween.Scatter(0.1f, _crateItems, (item, id) => {
                    return item.filler.DOFade(0, 1 * speedScale);
                });

                this.Wait(timeItemsFormCircle * 1.0f * speedScale, next);
            }
        );
    }

    private void RemoveCrateFromPlayerData(System.Action callback = null) {
        if (!DataManager.Instance.allLootCratesList.Contains(crate) && DataManager.Instance.allLootCratesList.Count > 0) {
            //traceError("DataManager doesn't have this LootCrate! [DebugID: " + crate.DebugID + "]");
            if (dataMan.allLootCratesList.Count > 0) trace("DebugID at index[0]: " + dataMan.allLootCratesList[0].DebugID);
            return;
        }
        
        API.LootCrates.Remove(crate)
            .Then(res => {
                trace("Succesfully removed LootCrate: " + crate.MongoID);
                dataMan.allLootCratesList.Remove(crate);
                if (callback != null) callback();
            });
    }

    public void BtnOnItemClick(LootCrateItem crateItem, bool fromStory) {
        if (fromStory) return;
        if (_itemDetails != null) {
            traceError("Can only popup one ItemDetailsInterface at a time.");
            return;
        }

        DeselectItems();

        _currentLootCrateItem = crateItem;

        Item item = crateItem.itemData;

        _itemDetails = ItemDetailsInterface.Open(item);
        _itemDetails.onRemove += (panel) => {
            //When the Item Details panel is removed, null this reference and the Identify button as well:
            _itemDetails = null;
            _identifyButton = null;
        };

        _itemDetails.AddButton("Sell\n<color={1}>({0} gold)</color>".Format2(item.SellValue, ColorConstants.HTML_GOLD), _itemDetails.Btn_Sell);
        _itemDetails.AddButton("Shard", _itemDetails.Btn_Shard);

        var rectTrans = (RectTransform) _itemDetails.transform;
        rectTrans.pivot = new Vector2(0.5f, 0.5f);

        crateItem.isSelected = true;
    }

    private void DeselectItems() {
        foreach (LootCrateItem it in _crateItems) {
            it.isSelected = false;
        }
    }

    private void LayoutUpdate() {
        WavyPosition itemWavy;

        //If there's only one item left, just move it towards the middle:
        if (_crateItems.Count == 1) {
            itemWavy = _crateItems[0].GetComponent<WavyPosition>();
            itemWavy.targetPosition = Vector2.zero;
            return;
        }

        //Otherwise... move everything in a circular layout
        var circPositions = layoutAnim.AnimateToCircle(_crateItems.Count, Vector2.zero, 90, 300);

        for (int i = 0; i < _crateItems.Count; i++) {
            itemWavy = _crateItems[i].GetComponent<WavyPosition>();
            itemWavy.targetPosition = circPositions[i];
        }
    }

    private void OnChangedCurrency(int newValue, int oldValue, CurrencyTypes currencyType) {
        if (_identifyButton == null || currencyType != CurrencyTypes.SCROLLS_IDENTIFY) return;

        _currentLootCrateItem.UpdateIdentifiedIcon();
        _identifyButton.SetEnabledState(false);
    }

    private void OnItemRemoved(Item item) {
        if (_currentLootCrateItem == null) {
            traceError("How can an item be removed... yet there's none selected on the LootCrate screen?");
            return;
        }

        HideSingleItem(_currentLootCrateItem, true, timeBeforeHideSingleItem * speedScale);

        _currentLootCrateItem = null;
    }

    private void HideSingleItem(LootCrateItem crateItem, bool isUpdateLayout, float preDelay = 0) {
        if (!_crateItems.Contains(crateItem)) {
            traceError("LootCrateItem already removed, cannot hide it again!");
            return;
        }

        _crateItems.Remove(crateItem);

        FlyOutItem(crateItem, 0)
            .SetDelay(preDelay);

        if (!isUpdateLayout) return;

        this.Wait(preDelay + timeLayoutUpdate * speedScale, () => {
            Destroy(crateItem);

            if (_crateItems.Count == 0) {
                BtnClose();
                return;
            }

            LayoutUpdate();
        });
    }

    private Tweener FlyOutItem(LootCrateItem item, int id) {
        //Disable the "wavy" motion & target position chasing:
        var wavy = item.GetComponent<WavyPosition>();
        wavy.enabled = false;

        //Disable any interactivity on this item now:
        item.SetEnabledInChildren<Button>(false);

        //Fly everything up up up!
        return item.transform.TweenMoveByY(GetScreenSize().y * 2.5f, timeItemsFlyUp * speedScale)
            .SetEase(Ease.InSine);
    }

    public void BtnClose() {
        // Gracefully close here
        if (callbackOnComplete != null) callbackOnComplete();

        //MenuManager.Instance.Pop();
        _headerBar.RemoveSelf();
        if (!noMenuManagerRef) {
            RemoveSelf();
        }
    }
}
