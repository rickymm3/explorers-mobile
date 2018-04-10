using System;
using UnityEngine;

[Serializable]
public class Entity : BaseData {

    public Entity(string Identity, string Name, int maxHealth = 0) {
        this.Identity = Identity;
        _name = Name;
        MaxHealth = maxHealth;
        _health = maxHealth;
    }

    [SerializeField]
    string _name = "";
    public string Name {
        get { return _name; }
    }

    [SerializeField]
    int _health = 1000;
    public int Health {
        get { return _health; }
        protected set { _health = value; }
    }

    [SerializeField]
    int _maxHealth = 1000;
    public int MaxHealth {
        get { return _maxHealth; }
        protected set { _maxHealth = value; }
    }
}
