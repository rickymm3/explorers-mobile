using UnityEngine;

public class BuildManager : Singleton<BuildManager> {
    public BuildType buildType = BuildType.Development;

    public string urlBase = "http://ec2-52-9-42-190.us-west-1.compute.amazonaws.com:9999/g2j/json/";
    
    public string aliasDev = "sf-dev";
    public string aliasTest = "sf-test";
    public string aliasProd = "sf-prod";
    public bool useLocalJSON = false;
    public bool useStable = true;
    public int versionNumber = 1;

    void Start() {
        if (useStable)
            Debug.Log("[DATA] Using the Stable JSON");
        else
            Debug.Log("[DATA] Using the live and Unstable JSON");
    }

    public string CURRENT_ALIAS {
        get {
            switch (buildType) {
                case BuildType.Development: return aliasDev;
                case BuildType.Test: return aliasTest;
                case BuildType.Production: return aliasProd;
                default:
                    UnityEngine.Debug.Log("Error: incorrect build type in BuildOptions");
                    return "";
            }
        }
    }

    public string URL_JSON {
        get {
            if (useStable)
                return urlBase + CURRENT_ALIAS + "/stable/" + versionNumber;
            else
                return urlBase + CURRENT_ALIAS;
        }
    }

    public string LOCAL_JSON {
        get {
            return Application.persistentDataPath + "/" + CURRENT_ALIAS + ".json";
        }
    }
}
