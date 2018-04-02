using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroUIAnimationHandler : MonoBehaviour {
    Hero hero;

	public void Initialize (Hero hero) {
        this.hero = hero;
        /*
        GetWeaponBone weaponBone = GetComponent<GetWeaponBone>();

        if (weaponBone != null) {
            Sprite weaponSprite = Resources.Load<Sprite>("Items/Weapon/" + hero.EquipedItems[EquipmentType.Weapon].data.Sprite);
            weaponBone.Initialize(weaponSprite);
        }*/
    }
}
