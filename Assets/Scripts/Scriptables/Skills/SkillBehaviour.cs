using UnityEngine;

[System.Serializable]
public abstract class SkillBehaviour : ScriptableObject {
    public string Name = "";

    public abstract float Execute(Skill skill);
}
