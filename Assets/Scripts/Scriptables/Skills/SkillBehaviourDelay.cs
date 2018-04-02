using UnityEngine;
using DG.Tweening;

[System.Serializable]
[CreateAssetMenu(menuName = "Skill/New Delay Behaviour")]
public class SkillBehaviourDelay : SkillBehaviour {
    
    public float duration = 0f;

    public override float Execute(Skill skill) {
        return duration;
    }
}
