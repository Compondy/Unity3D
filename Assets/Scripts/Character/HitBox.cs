using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public Health health;

    public void OnRaycastHit(RaycastWeapon weapon, Vector3 direction) {
        health.TakeDamage(weapon.damage, direction);
    }

    public void OnCloseHit(int damage, Vector3 direction)
    {
        health.TakeDamage(damage, direction);
    }

}
