using UnityEngine;
using DG.Tweening;

[System.Serializable]
[CreateAssetMenu(menuName = "Skill/New Return Behaviour")]
public class SkillBehaviourReturn : SkillBehaviour {
    
    BattleActor actor;
    public float duration = 0f;

    public override float Execute(Skill skill) {
        actor = skill.actor;
        
        actor.GetGameObject().transform.DOLocalMove(new Vector3(0f, 0f, 0f), duration); // may need to do a local position target

        return duration;
    }
}
