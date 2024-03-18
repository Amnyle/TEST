using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using Unity.Netcode;
using UnityEngine;
using System;

public class CombatSystem : NetworkBehaviour
{
    

    [Header("Combat")]
    [SerializeField] private new Camera camera;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float range = 6f;
    private Enemy aimedEnemy;
    private bool canAttack = true;


    [Header("Input")]
    public KeyCode attackKey = KeyCode.R;

    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(attackKey))
            Attack();
        HandleInteraction();
    }
    private void HandleInteraction()

    {
        Ray ray = new Ray(camera.transform.position, camera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * range);
        if (Physics.Raycast(ray, out RaycastHit raycastHitInfo, range))
        {
            if (raycastHitInfo.transform.TryGetComponent(out Enemy enemy))
            {
                //is enemy
                if (aimedEnemy != enemy)
                {
                    aimedEnemy = enemy;
                }
            }
            else
            {
                aimedEnemy = null;
            }
        }
        else
        {
            aimedEnemy = null;
        }
    }
    private void Attack()
    {
        if (!canAttack) return; //attack cooldown not finish yet

        canAttack = false;
        StartCoroutine(ResetAttackCooldown());
  
        if (aimedEnemy != null)
        {
            Debug.Log("Ennemy took damage");
        }
        else  Debug.Log("caca");

    }
    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }





}
