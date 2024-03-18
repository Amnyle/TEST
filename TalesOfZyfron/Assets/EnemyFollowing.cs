using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollowing : MonoBehaviour
{
    public GameObject currentTarget;
    public GameObject player;
    public NavMeshAgent navMeshAgent;
    public TargetScanner targetScanner;
    [SerializeField] private float timer = 5;
    private float bulletTime;
    public GameObject enemyBullet;
    public Transform spawnPoint;
    public float enemySpeed;

    public float delayLostTarget = 10f;

    Animator animator;
    private float timeLostTarget = 0;

    //Stand ou Idle = attendre
    const string STAND_STATE = "Stand";
 
    //Reçoit des dommages
    const string TAKE_DAMAGE_STATE = "Damage";
 
    //Est vaincu
    public const string DEFEATED_STATE = "Defeated";
 
    //Est en train de marcher
    public const string WALK_STATE = "Walk";
 
    //Attaque
    public const string ATTACK_STATE = "Attack";
 
    //Mémorise l'action actuelle
    public string currentAction;
    // Start is called before the first frame update
    void Awake()
    {
         //Au départ, la créature attend en restant debout
        currentAction = STAND_STATE;
 
        //Référence vers l'Animator
        animator = GetComponent<Animator>();
 
        //Référence NavMeshAgent
        navMeshAgent = GetComponent<NavMeshAgent>();

        //Référence de Player
        player = FindObjectOfType<NewPlayerController>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        //si la créature est défaite
        //Elle ne peut rien faire d'autres
        if (currentAction == DEFEATED_STATE)
        {
            navMeshAgent.ResetPath();
            return;
        }
        
        if (currentAction == TAKE_DAMAGE_STATE)
        {
            navMeshAgent.ResetPath();
            TakingDamage();
            return;
        }
        FindingTarget();
        
        //Si pas de cible, ne fait rien
        if (currentTarget == null)
        {
            //Defaut
            Stand();
            navMeshAgent.ResetPath();
            return;
        }
        if (MovingToTarget())
        {
            //En train de marcher
            return;
        }
        
        if (currentAction != ATTACK_STATE && currentAction != TAKE_DAMAGE_STATE)
        {
            Attack();
            return;
        }
        if (currentAction == ATTACK_STATE)
        {
            Attacking();
            return;
        }
    }
    void ShootAtPlayer()
    {
        
        bulletTime -= Time.deltaTime;

        if (bulletTime>0) return;

        bulletTime = timer;
        GameObject bulletObj = Instantiate(enemyBullet, spawnPoint.transform.position, spawnPoint.transform.rotation) as GameObject;
        Rigidbody bulletRig = bulletObj.GetComponent<Rigidbody>();
        bulletRig.AddForce(bulletRig.transform.forward * enemySpeed);
        Destroy(bulletObj, 1f);
    }
    private void Stand()
    {
        //Réinitialise les paramètres de l'animator
        ResetAnimation();
        //L'action est maintenant "Stand"
        currentAction = STAND_STATE;
        //Le paramètre "Stand" de l'animator = true
        animator.SetBool("Stand", true);
    }
    private void FindingTarget()
    {
        //Si le joueur est détecté
        if (targetScanner.Detect(transform, player ))
        {
            currentTarget = player;
            timeLostTarget = 0;
            return;
        }
 
        //Si le joueur était détecté
        //Calcule le temps avant d'abandonner
        if (currentTarget != null)
        {
            timeLostTarget += Time.deltaTime;
 
            if (timeLostTarget > delayLostTarget)
            {
                timeLostTarget = 0;
                currentTarget = null;
            }
 
 
            return;
        }
 
 
        currentTarget = null;
 
    }
    public void TakeDamage()
    {
        //Réinitialise les paramètres de l'animator
        ResetAnimation();
        //L'action est maintenant "Damage"
        currentAction = TAKE_DAMAGE_STATE;
        //Le paramètre "Damage" de l'animator = true
        animator.SetBool("Damage", true);
    }
 
    public void Defeated()
    {
        //Réinitialise les paramètres de l'animator
        ResetAnimation();
        //L'action est maintenant "Defeated"  
        currentAction = DEFEATED_STATE;
        //Le paramètre "Defeated" de l'animator = true
        animator.SetBool(DEFEATED_STATE, true);
    }
    private void TakingDamage()
    {
 
        if (this.animator.GetCurrentAnimatorStateInfo(0).IsName(TAKE_DAMAGE_STATE))
        {
            //Compte le temps de l'animation
            //normalizedTime : temps écoulé nomralisé (de 0 à 1).
            //Si normalizedTime = 0 => C'est le début.
            //Si normalizedTime = 0.5 => C'est la moitié.
            //Si normalizedTime = 1 => C'est la fin.
 
 
            float normalizedTime = this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
 
 
            //Fin de l'animation
            if (normalizedTime > 1)
            {
                Stand();
            }
 
        }
 
    }
    private void Attacking()
    {
        if (this.animator.GetCurrentAnimatorStateInfo(0).IsName(ATTACK_STATE))
        {
            //Compte le temps de l'animation
            //normalizedTime : temps écoulé nomralisé (de 0 à 1).
            //Si normalizedTime = 0 => C'est le début.
            //Si normalizedTime = 0.5 => C'est la moitié.
            //Si normalizedTime = 1 => C'est la fin.
            float normalizedTime = this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
 
 
            //Fin de l'animation
            if (normalizedTime > 1)
            {
 
                Stand();
                return;
            }
 
            ShootAtPlayer();
 
        }
    }

    private bool MovingToTarget()
    {
 
        //Assigne la destination : le joueur
        navMeshAgent.SetDestination(player.transform.position);
 
        //Si navMeshAgent n'est pas prêt
        if (navMeshAgent.remainingDistance == 0)
            return true;
 
 
        // navMeshAgent.remainingDistance = distance restante pour atteindre la cible (Player)
        // navMeshAgent.stoppingDistance = à quelle distance de la cible l'IA doit s'arrêter 
        // (exemple 2 m pour le corps à sorps) 
        
        else
        {
            //Si arrivé à bonne distance, regarde vers le joueur
            RotateToTarget(currentTarget.transform);
 
            navMeshAgent.isStopped = true;
        }
 
        return false;
    }
    private void Attack()
    {
        //Réinitialise les paramètres de l'animator
        ResetAnimation();
        //L'action est maintenant "Attack"
        currentAction = ATTACK_STATE;
        //Le paramètre "Attack" de l'animator = true
        animator.SetBool(ATTACK_STATE, true);
    }
    private void RotateToTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);
    }
 
    //Réinitialise les paramètres de l'animator
    private void ResetAnimation()
    {
        animator.SetBool(STAND_STATE, false);
        animator.SetBool(TAKE_DAMAGE_STATE, false);
        animator.SetBool(DEFEATED_STATE, false);
        animator.SetBool(WALK_STATE, false);
        animator.SetBool(ATTACK_STATE, false);
    }
 
}
