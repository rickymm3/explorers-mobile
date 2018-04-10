using UnityEngine;
using DG.Tweening;

[System.Serializable]
[CreateAssetMenu(menuName = "Skill/New Move Behaviour")]
public class SkillBehaviourMove : SkillBehaviour {

    BattleActor actor;
    BattleActor target;

    public float duration = 0f;
    public float distanceBuffer = 0f;

    public override float Execute(Skill skill) {
        actor = skill.actor;
        Vector3 targetPosition = skill.GetTargets()[0].GetGameObject().transform.position;
        Vector3 actorPosition = actor.GetGameObject().transform.position;

        Vector3 dir = (actorPosition - targetPosition).normalized;
        Vector3 moveTarget = targetPosition + (dir * distanceBuffer);

        actor.GetGameObject().transform.DOMove(moveTarget, duration);

        return duration;
    }
}
