using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelWithGetters : Panel {

    public static GameAPIManager API { get { return GameAPIManager.Instance; } }
    public static BuildManager buildMan { get { return BuildManager.Instance; } }
    public static GameManager gameMan { get { return GameManager.Instance; } }
    public static DataManager dataMan { get { return DataManager.Instance; } }
    public static MenuManager menuMan { get { return MenuManager.Instance; } }
    public static AudioManager audioMan { get { return AudioManager.Instance; } }
    public static PlayerManager playerMan { get { return PlayerManager.Instance; } }
    public static PlayerSignals signals { get { return PlayerManager.Instance.Signals; } }
    public static GlobalsData globals { get { return dataMan.globalData; } }
    public static CurrencyManager coinMan { get { return CurrencyManager.Instance; } }
}
