using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class HeroSpawner : MonoBehaviour {

    public List<Transform> HeroSpawnPositions = new List<Transform>();
    
	void Start () {
        // Get the hero list
        List<Hero> heroes = PlayerManager.Instance.GetAvailableHeroes();
        int SpawnCount = heroes.Count;
        GameObject HeroHolder;

        heroes = heroes.OrderBy(h => h.Level).ToList();

        // if you don't have enough just grab what you can
        if (heroes.Count > HeroSpawnPositions.Count)
            SpawnCount = HeroSpawnPositions.Count;

        // randomly spawn in those heroes at the spawn locations
        for (int i = 0; i < SpawnCount; i++) {
            HeroHolder = Instantiate(heroes[i].LoadTapBattleModel());
            HeroHolder.transform.SetParent(HeroSpawnPositions[i]);
            HeroHolder.transform.localPosition = Vector3.zero;
            Vector3 scale = Vector3.one;
            if (heroes[i].heroData.Type == HeroType.Monster)
                scale.x = -1f;
            HeroHolder.transform.localScale = scale;
            //HeroHolder.GetComponent<TapCharacterHandler>().Initialize(CharacterBattleMode.Tap, heroes[i]);
            if (HeroHolder.transform.childCount > 0)
                HeroHolder.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
