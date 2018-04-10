using UnityEngine;
using DG.Tweening;
using Spine.Unity;

[System.Serializable]
[CreateAssetMenu(menuName = "Skill/New Fade Alpha Behaviour")]
public class SkillBehaviourFadeCharacter : SkillBehaviour {

    BattleActor actor;
    BattleActor target;

    public float duration = 0f;
    public float alphaTarget = 0f;

    Color color;

    public override float Execute(Skill skill) {

        Spine.Skeleton skeleton = actor.GetGameObject().GetComponent<SkeletonAnimation>().skeleton;
        
        color = skeleton.GetColor();
        color.a = alphaTarget;
        DOTween.To(() => skeleton.Color, x => skeleton.Color = x, color, duration);

        return duration;
    }
    
}
