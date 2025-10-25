using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour {

    public RaycastWeapon weaponFab;
    public GameObject weaponCloseFab;


    private void OnTriggerEnter(Collider other) {
        ActiveWeapon activeWeapon = other.gameObject.GetComponent<ActiveWeapon>();
        if (activeWeapon) {
            if (!weaponCloseFab)
            {
                RaycastWeapon newWeapon = Instantiate(weaponFab);
                activeWeapon.Equip(newWeapon);
                Destroy(gameObject);
            }
        }

        AiWeapons aiWeapons = other.gameObject.GetComponent<AiWeapons>();
        if (aiWeapons) {
            if (!weaponCloseFab)
            {
                RaycastWeapon newWeapon = Instantiate(weaponFab);
                aiWeapons.Equip(newWeapon);
            }
            else
            {
                var newWeapon = Instantiate(weaponCloseFab);
                aiWeapons.Equip(newWeapon);
            }
                Destroy(gameObject);
        }
    }
}
