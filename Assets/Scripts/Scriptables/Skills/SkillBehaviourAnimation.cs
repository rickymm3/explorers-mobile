using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Skill/New Animation Behaviour")]
public class SkillBehaviourAnimation : SkillBehaviour {
    
    public string animationName = "";
    
    BattleActor actor;
    float duration = 0f;
    public bool delayForAnimation = true;

    public override float Execute(Skill skill) {
        // get animation duration and return it
        actor = skill.GetActor();

        if (actor is HeroActor)
            duration = ((HeroActor) actor).PlayAnimation(animationName);
        if (actor is BossActor)
            duration = ((BossActor) actor).PlayAnimation(animationName);
        if (delayForAnimation)
            return duration;
        else
            return 0f;
    }
}
