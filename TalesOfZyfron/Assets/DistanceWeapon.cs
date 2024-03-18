using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceWeapon : MonoBehaviour
{
    //Damage que fait l'arme
    public int damage = 1;
 
    //Détermine quel Layer on peut toucher
    public LayerMask layerMask;
 
    //Est-ce que l'arme est en train d'être utilisée ?
    public bool isAttacking = false;

    public void StartAttack()
    {
        isAttacking = true;
    }
 
    public void StopAttack()
    {
        isAttacking = false;
    }
    
}
