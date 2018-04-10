using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public static class MathHelper {

	public static KeyValuePair<K, float> WeightedRandom<K> (IDictionary<K, float> dict) {
        if(dict.Count==0) throw new Exception("Requires at least 1 entry in weighted-dictionary.");

        float total = Sum(dict.Values);
        float offset = UnityEngine.Random.Range(0, total);
        List<KeyValuePair<K,float>> sorted = dict.ToSortedList();

        for (var i = 0; i < sorted.Count; i++) {
            KeyValuePair<K, float> entry = sorted[i];
            float v = entry.Value;
            if (offset < v) {
                return entry;
            }

            offset -= v;
        }

        throw new Exception("Could not pick a weighted entry in the dictionary: " + offset + " , " + total);
    }

    public static float Sum(IEnumerable<float> values) {
        float result = 0;
        foreach(float f in values) result += f;
        return result;
    }
}
