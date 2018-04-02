using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class MathTest : MonoBehaviour {
    void Start() {
        /*DirectoryInfo d = new DirectoryInfo(@"S:\Git\EggRollDigital\summonersfrontier\SummonersFrontier\Assets\Resources\Items\Artifact");
        FileInfo[] infos = d.GetFiles();
        int index = 0;
        foreach (FileInfo f in infos) {
            //File.Move(f.FullName, f.FullName.ToString().Substring(0, 43));
            if (!f.FullName.Contains(".meta")) {
                print(f.FullName);
                print(f.FullName.ToString().Substring(0, 90) + "artifact_" + index + ".png");
                //File.Move(f.FullName, f.FullName.ToString().Substring(0, 90) + "artifact_" + index + ".png");
                index++;
            }

        }*/
        
    }

    [ContextMenu("Test Math Distribution")]
    void TestDistribution() {
        float quality;
        int[] results = new int[] { 0, 0, 0 };
        for (int i = 0; i < 10000; i++) {
            quality = (Random.Range(0.01f, 0.5f) + Random.Range(0f, 0.5f)) * 3;
            results[Mathf.FloorToInt(quality)]++;
        }
        string temp = "Results:";
        for (int i = 0; i < results.Length; i++) {
            temp += "\n[" + i + "] | ";
            for (int row = 0; row < Mathf.RoundToInt(results[i] / 1000f); row++) {
                temp += "=";
            }
            temp += " [" + results[i] + "]";
        }
        Debug.Log(temp);
    }
}
