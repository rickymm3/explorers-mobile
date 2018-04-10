using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using ExtensionMethods;

public class RetireRewardsData : BaseData {
    static string[] _VALID_ITEMS = "low_essence,medium_essence,high_essence,hero_scroll,rare_scroll,legendary_scroll,experience_shard".Split(",");
    
    public RetireRewardDictionary ItemsGuaranteed;
    public RetireRewardDictionary ItemsRandom;
    
    public int MinCount = 0;
    public int MaxCount = 0;
    public bool RequireAwoken = false;
    public HeroQuality Quality = 0;

    public List<HeroType> Types = new List<HeroType>();

    public RetireRewardsData() {}

    public void ParseJSON(JSONNode json) {
        Identity = json["identity"].AsDecodedURL();
        MinCount = json["min-count"].AsInt;
        MaxCount = json["max-count"].AsInt;
        RequireAwoken = json["awoken"].AsBool;
        Quality = json["quality"].AsEnum<HeroQuality>();

        ItemsGuaranteed = new RetireRewardDictionary();
        ItemsRandom = new RetireRewardDictionary();
        Types = new List<HeroType>();
        
        //Parse the Key-Value pairs in the Guaranteed & Random items:
        var guaranteedItems = JSONManager.SplitKVFloats(json["guaranteed-items"]);
        var randomItems = JSONManager.SplitKVFloats(json["random-items"]);
        
        foreach (var kv in guaranteedItems) ItemsGuaranteed.Add(kv.Key, kv.Value);
        foreach (var kv in randomItems) ItemsRandom.Add(kv.Key, kv.Value);
        
        //Validate this entry...
        if (MinCount>MaxCount) JSONError("MinCount > MaxCount, should probably swap these in RetireRewards table.");
        if (ItemsGuaranteed.Count==0 && ItemsRandom.Count==0) {
            JSONError("RetireReward doesn't have any Guaranteed -or- Random items!");
        }

        //Tracer.trace(Debug());
    }

    void JSONError(string err) {
        Tracer.traceError("[JSON ERROR] " + err + " " + this.Identity);
    }

    /*public RewardResult GetReward() {
        //Apply the weights of each random items:
        var randomKV = MathHelper.WeightedRandom(ItemsRandom);

        RewardResult result = new RewardResult();
        result.guaranteed = this.ItemsGuaranteed;
        result.randomAmount = UnityEngine.Random.Range(MinCount, MaxCount + 1);
        result.randomItem = randomKV.Key;

        return result;
    }*/

    public Dictionary<string, int> GetRewards() {
        KeyValuePair<string, float> randomKV;

        Dictionary<string, int> result = new Dictionary<string, int>();
        foreach(string key in this.ItemsGuaranteed.Keys) {
            if (result.ContainsKey(key))
                result[key] += Mathf.RoundToInt(this.ItemsGuaranteed[key]);
            else
                result.Add(key, Mathf.RoundToInt(this.ItemsGuaranteed[key]));
        }

        int extraLoot = MinCount + Mathf.RoundToInt((UnityEngine.Random.Range(0f, 1f) * UnityEngine.Random.Range(0f, 1f)) * ((float) MaxCount + 1f));
        for (int i = 0; i < extraLoot; i++) {
            randomKV = MathHelper.WeightedRandom(ItemsRandom);

            if (result.ContainsKey(randomKV.Key))
                result[randomKV.Key]++;
            else
                result.Add(randomKV.Key, 1);
        }

        return result;
    }

    public string Debug(bool pretty=true) {
        return this.ToJSONString(pretty);
    }

    public class RewardResult {
        public RetireRewardDictionary guaranteed;
        public string randomItem;
        public int randomAmount = 0;
    }
}
