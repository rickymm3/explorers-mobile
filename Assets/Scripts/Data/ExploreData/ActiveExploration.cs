using System;
using System.Collections.Generic;

[Serializable]
public class ActiveExploration : MongoData<ZoneData> {
    public float DPS = 0f;
    public float AccumulatedDamage = 0f;
    public float MagicFind = 0f;
    public float MonsterFind = 0f;
    public float TreasureFind = 0f;
    public int ChestsEarned = 0;
    public float TimeReduced = 0f;
    public bool EventTriggered = false;

    public List<Hero> Party = new List<Hero>();
    public Hero GroupLeader;
    public DateTime TimeStarted;

    public ZoneData Zone { get { return data; } }

    public ActiveExploration(ZoneData Zone, List<Hero> Party) {
        this.data = Zone;
        this.Party = Party;
        TimeStarted = new DateTime();
        TimeStarted = DateTime.Now;

        InitPartyAndLootCrates();
    }

    public ActiveExploration(ZoneData Zone, List<Hero> Party, DateTime TimeStarted, Hero GroupLeader) {
        this.data = Zone;
        this.Party = Party;
        this.TimeStarted = TimeStarted;
        this.GroupLeader = GroupLeader;

        InitPartyAndLootCrates();
    }

    private void InitPartyAndLootCrates() {
        foreach (Hero hero in Party) {
            //Power += hero.GetCoreStat(CoreStats.Damage);
            if(hero==null) {
                Tracer.traceError("Can't 'InitPartyAndLootCrates' on Hero in Party, it's null!");
                continue;
            }
            DPS += hero.GetCoreStat(CoreStats.Damage) * ((float)hero.GetPrimaryStat(PrimaryStats.Speed) / 100f);
            MagicFind += hero.GetSecondaryStat(SecondaryStats.MagicFind);
            MonsterFind += hero.GetSecondaryStat(SecondaryStats.MonsterFind);
            TreasureFind += hero.GetSecondaryStat(SecondaryStats.TreasureFind);
        }
    }

    public float Duration {
        get {
            float results = 0f;

            results = ((Zone.ZoneHP - AccumulatedDamage) / DPS) + (DataManager.Instance.globalData.GetGlobalAsInt(GlobalProps.ZONE_MONSTER_COUNT) * GameManager.DEATH_DELAY);
            //(((Zone.ZoneHP * Zone.MonsterCount) - AccumulatedDamage) / DPS) + (Zone.MonsterCount * GameManager.DEATH_DELAY); // old duration calculation
            //TimeSpan remaining = data.TimeStarted.AddSeconds(duration) - DateTime.Now;

            return results;
        }
    }

    public TimeSpan Remaining {
        get {
            return TimeStarted.AddSeconds(Duration) - DateTime.Now;
        }
    }

    public float RemainingSeconds {
        get {
            TimeSpan remaining = TimeStarted.AddSeconds(Duration) - DateTime.Now;
            return UnityEngine.Mathf.Max((float) (remaining.Seconds + (60f * (remaining.Minutes + 60f * remaining.Hours))), 0f);
        }
    }

    public void CompleteExploration(bool success) {
        UnityEngine.Debug.Log("Starting to mark active zone complete. [Act " + Zone.Act + " Zone " + Zone.Zone + "]");
        if (success) {
            if (Zone.ActZoneID > PlayerManager.Instance.ActZoneCompleted) {
                PlayerManager.Instance.ActZoneCompleted = Zone.ActZoneID;
                GameAPIManager.API.Users.ActZoneComplete(Zone.ActZoneID);
            }
        }

        API.Explorations.Remove(Zone.ActZoneID)
            .Then(res => {
                Tracer.trace("Removed Exploration successfully!");
                Tracer.trace(res.pretty);

                foreach(Hero hero in Party) {
                    hero.ExploringActZone = 0;
                }

                UnityEngine.Debug.Log("Zone is complete. [Act " + Zone.Act + " Zone " + Zone.Zone + "]");
                dataMan.allExplorationsList.Remove(this);
            })
            .Catch(err => {
                Tracer.traceError("Could not remove the exploration: " + GameAPIManager.GetErrorMessage(err));
            });
    }
}
