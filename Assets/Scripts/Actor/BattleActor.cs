using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BattleActor {
    public float speed = 0f;
    public float AverageSpeed = 1f;
    public float CurrentSpeedOffset = 0f;
    public float TemporaryOffset = 0f;
    public float Health = -1f;
    public Skill QueuedSkill;
    public BattleActor QueuedSkillTarget;
    public FloatingCombatTextInterface FCTInterface;
    protected Vector3 FCTOffset = new Vector3(0f, 50f, 0f);

    public virtual Vector3 GetWorldPosition() {
        UnityEngine.Debug.Log("TakeDamage Not Implemented");
        return Vector3.zero;
    }
    public virtual Vector3 GetScreenPosition() {
        UnityEngine.Debug.Log("TakeDamage Not Implemented");
        return Vector3.zero;
    }
    public virtual bool HasOnHitStatusEffectOnGear() {
        UnityEngine.Debug.Log("HasOnHitStatusEffectOnGear Not Implemented");
        return false;
    }
    public virtual List<StatusEffect> GetOnHitStatusEffects() {
        UnityEngine.Debug.Log("GetOnHitStatusEffects Not Implemented");
        return null;
    }

    public virtual void Heal(float amount, bool resurrect = false) {
        UnityEngine.Debug.Log("Heal Not Implemented");
    }

    public virtual void TakeDamage(float damage) {
        UnityEngine.Debug.Log("TakeDamage Not Implemented");
    }

    public virtual void InterruptSkill() {
        if (QueuedSkill != null) Debug.Log("Interrupting Skill: " + QueuedSkill.name);

        QueuedSkill = null;
        QueuedSkillTarget = null;
    }

    public virtual void ApplyStatusEffect(StatusEffect Effect) {
        UnityEngine.Debug.Log("ApplyStatusEffect Not Implemented");
    }

    public virtual void UpdateStatusEffects(StatusTriggerTime TriggerTime) {
        UnityEngine.Debug.Log("ApplyStatusEffect Not Implemented");
    }

    public virtual bool IsStunned() {
        Debug.LogWarning("Implement stun check");
        return false;
    }

    public virtual bool HasQueuedSkill() {
        if (QueuedSkill != null && QueuedSkillTarget != null)
            return true;
        return false;
    }

    public virtual string Name {
        get {
            return "";
        }
    }

    public virtual GameObject GetGameObject() {
        return null;
    }

    public virtual Sprite LoadSprite() {
        Debug.Log("[Load Sprite] Not implemented");
        return null;
    }

    public virtual void TriggerCast() {
        Debug.Log("[TriggerCast] Not implemented");
    }
    public virtual void StopCast() {
        Debug.Log("[StopCast] Not implemented");
    }

    public virtual void OnAllyDeath() {
        Debug.Log("[OnAllyDeath] Not implemented");
    }
}
