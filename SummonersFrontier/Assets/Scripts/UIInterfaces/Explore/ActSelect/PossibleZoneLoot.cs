using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class PossibleZoneLoot : MonoBehaviour {

    public GameObject ItemReference;
    public RectTransform ItemContainer;
    public TextMeshProUGUI UniqueNoneText;
    public bool OnlyUniques = false;

    List<GameObject> prefabItemList = new List<GameObject>();

    public void Initialize(ZoneData data) {
        List<ItemData> itemOptions = DataManager.Instance.GetItemsByLootTableFilters(data.LootTable.tierChance.Keys.ToList(), data.LootTable.itemTypes.Keys.ToList(), data.LootTable.AvailableCurrencyTypes);
        ItemReference.SetActive(false);

        foreach (GameObject go in prefabItemList) {
            go.SetActive(false);
        }

        GameObject TempItem = null;
        if (OnlyUniques) {
            List<UniqueReference> Uniques = new List<UniqueReference>();
            foreach (ItemData item in itemOptions) {
                Uniques.Add(DataManager.Instance.uniqueReferenceDataList.Find(u => u.Reference == item.Identity));
            }

            //Debug.Log(" - Uniques Count: " + Uniques.Count + "[" + DataManager.Instance.uniqueReferenceDataList.Count + "][io: " + itemOptions.Count + "]");
            
            foreach (UniqueReference item in Uniques) {
                TempItem = GetItemPrefab();
                TempItem.SetActive(true);
                TempItem.GetComponent<ZoneLootOverview>().Initialize(item);
            }

            if (Uniques.Count > 0) {
                ItemContainer.gameObject.SetActive(true);
                UniqueNoneText.gameObject.SetActive(false);
            } else {
                UniqueNoneText.gameObject.SetActive(true);
                ItemContainer.gameObject.SetActive(false);
            }
        } else {
            foreach (ItemData item in itemOptions) {
                TempItem = GetItemPrefab();
                TempItem.SetActive(true);
                TempItem.GetComponent<ZoneLootOverview>().Initialize(item);
            }
        }
        TempItem = null;
    }

    GameObject GetItemPrefab() {
        for (int i = 0; i < prefabItemList.Count; i++) {
            if (!prefabItemList[i].activeSelf)
                return prefabItemList[i];
        }

        GameObject TempItem = Instantiate(ItemReference, ItemContainer);

        prefabItemList.Add(TempItem);

        return TempItem;
    }
}
