using System.Collections.Generic;

public class RewardsByPlayerLevel : BaseData {
    public int Level;
    public List<string> Rewards = new List<string>();

    public RewardsByPlayerLevel(int level, List<string> rewards) {
        Level = level;
        Rewards = rewards;
    }
}
