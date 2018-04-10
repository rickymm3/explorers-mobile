using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRootController : MonoBehaviour {
    public Transform EnvironmentEffectsParent;
    public SpriteRenderer backgroundRenderer;
    public List<Transform> MonsterPositions = new List<Transform>();
    public List<Transform> PartyPositions = new List<Transform>();
    public List<EnemyHealthBarInterface> HealthBars = new List<EnemyHealthBarInterface>();
    public ActorSelector selector;

    void Start() {
        ZoneData zone = PlayerManager.Instance.SelectedBattle.Zone;

        // Replace the level Graphic
        backgroundRenderer.sprite = Resources.Load<Sprite>("ActZoneArt/Act" + zone.Act + "/Zone" + zone.Zone + "/battlebkg");

        // Spawn the Environment Effects
        GameObject environmentFX = (GameObject) Instantiate(Resources.Load("ActZoneArt/Act" + zone.Act + "/Zone" + zone.Zone + "/EnvFX"));
        environmentFX.transform.SetParent(EnvironmentEffectsParent);

        // Change monster/party position based on screen width (use canvas calculations maybe?)

    }

    public ActorSelector GetSelector() {
        return selector;
    }
}
