using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class GetWeaponBone : MonoBehaviour {

    private Spine.Skeleton skeleton;
    Spine.ExposedList<Spine.Bone> skeletonBones = new Spine.ExposedList<Spine.Bone>();
    int index = 0;
    public List<GameObject> weapons;

    Dictionary<GameObject, Spine.Bone> weaponsDict = new Dictionary<GameObject, Spine.Bone>();

    bool initialized = false;
    
    public void Initialize(Sprite weaponSprite) {
        if (GetComponent<SkeletonAnimation>() != null)
            skeleton = GetComponent<SkeletonAnimation>().Skeleton;

        if (skeleton == null) {
            Debug.LogWarning("'skeleton' null after SkeletonAnimation, check for SkeletonGraphic.");
            skeleton = GetComponent<SkeletonGraphic>().Skeleton;
        }

        if (skeleton == null) {
            Debug.LogError("'skeleton' still null, abort and fix this");
            return;
        }

        skeletonBones = skeleton.Bones;
        int count = 0;
        string boneLog = "";
        foreach (Spine.Bone bone in skeletonBones) {
            if (bone.Data.name.ToLower() == "Sword".ToLower() && count < weapons.Count) {
                weaponsDict.Add(weapons[count], bone);
                count++;
            } else if (bone.Data.name.ToLower() == "Weapon".ToLower() && count < weapons.Count) {
                weaponsDict.Add(weapons[count], bone);
                count++;
            } else if (bone.Data.name.ToLower() == "Weapon1".ToLower() && count < weapons.Count) {
                weaponsDict.Add(weapons[count], bone);
                count++;
            } else if (bone.Data.name.ToLower() == "Weapon2".ToLower() && count < weapons.Count) {
                weaponsDict.Add(weapons[count], bone);
                count++;
            }
            /*
            if (bone.Data.name.ToLower() == "Weapon".ToLower()) {
                Debug.Log("[Weapons] We found the Weapon Bone with a counter of [" + count + "] and a weapons count of [" + weapons.Count + "][Dict: " + weaponsDict.Count + "]");
            }*/

            boneLog += bone.data.name + ", ";
        }

        if (weaponsDict.Count < 1)
            Debug.Log("[Weapons] No weapon bone found!\n" + boneLog);

        foreach(GameObject weapon in weapons)
            weapon.GetComponent<SpriteRenderer>().sprite = weaponSprite;
        
        initialized = true;
    }

    void LateUpdate() {
        if (!initialized) return;

        foreach (GameObject weapon in weaponsDict.Keys) {
            weapon.transform.localPosition = new Vector3(weaponsDict[weapon].worldX, weaponsDict[weapon].worldY, 0f);
            weapon.transform.rotation = Quaternion.AngleAxis(weaponsDict[weapon].WorldRotationX - 45f, Vector3.forward);
        }
        //weapon.transform.rotation = Quaternion.Euler(boneController.WorldRotationX, boneController.WorldRotationY, 0f);
    }
}
