using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;
using TMPro;

public class ResearchUnidentifiedItems : PanelWithGetters {
    public static string BUTTON_LABEL = "START RESEARCH"; //FOR 1 SCROLL\n<size=-10>({0} scrolls left)</size>

    public RectTransform panel;
    public GridLayoutGroup grid;
    public ScrollRect scrollRect;
    public Image modalShadow;
    public Button btnClose;
    public Button btnIdentify;
    public TextMeshProUGUI txtIdentify;
    public ItemDisplayInterface itemTemplate;
    public RectTransform selector;
    public GameObject noItemsToSelect;
    
    [HideInInspector] public Action<Item> OnSelectedItem;

    List<ItemDisplayInterface> _items;

    Item _item;
    public Item item {
        get { return _item; }
        set { _item = value; }
    }

	// Use this for initialization
	void Start () {
        btnClose.onClick.AddListener(Btn_OnClose);
        btnIdentify.onClick.AddListener(Btn_OnIdentify);

        selector.gameObject.SetActive(false);

        _items = new List<ItemDisplayInterface>();

        if(!dataMan || dataMan.allItemsList==null) {
            traceError("Only running demo, dataMan | dataMan.allItemsList is not initialized.");
            return;
        }

        int scrolls = CurrencyTypes.SCROLLS_IDENTIFY.GetAmount();
        txtIdentify.text = BUTTON_LABEL.Format2(scrolls);
        
        var itemDatas = dataMan.allItemsList.FindAll( item => !item.isIdentified && !item.isResearched );

        foreach (Item itemData in itemDatas) {
            ItemDisplayInterface itemContainer = grid.AddClone(itemTemplate);
            itemContainer.transform.localScale = Vector2.one;
            itemContainer.LoadItem(itemData, Btn_ItemClicked, null);

            _items.Add(itemContainer);
        }

        noItemsToSelect.SetActive(false);
        itemTemplate.gameObject.SetActive(false);

        bool isNoItems = itemDatas.Count == 0;
        bool isNoScrolls = scrolls == 0;

        if (isNoItems) {
            noItemsToSelect.SetActive(true);
        }
        if(isNoItems || isNoScrolls) {
            btnIdentify.interactable = false;
        }
    }

    private void Btn_ItemClicked(Item item, ItemDisplayInterface display) {
        this.item = item;

        selector.gameObject.SetActive(true);
        selector.SetParent(display.transform);
        selector.SetAsFirstSibling();
        selector.localPosition = Vector2.zero;
        selector.localScale = Vector2.one;
    }

    void OnDestroy() {
        OnSelectedItem = null;
    }

    void Btn_OnClose() {
        if (!Close()) return;

        DoClosingTransition(panel, modalShadow);
    }
        
    void Btn_OnIdentify() {
        if(_item==null) {
            traceError("Must have one (1) item selected to begin identifying it!");
            TimelineTween.ShakeError(btnIdentify.gameObject);
            return;
        }

        if (!Close()) return;

        if(OnSelectedItem!=null) OnSelectedItem(_item);

        DoClosingTransition(panel, modalShadow);
    }
}
