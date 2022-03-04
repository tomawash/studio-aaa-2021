using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIChase : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private LayerMask playerLayer;

    //State Machine
    enum BasicEnemyAIStates
    {
        CHASE,
        ATTACK
    }
    enum BasicEnemyAttackStates
    {
        COOLDOWN,
        STARTUP,
        ATTACK,
        RECOVERY
    }

    private BasicEnemyAIStates currentAIState;
    private BasicEnemyAttackStates currentAttackState;

    //Attack
    [SerializeField]
    private float attackCDBase;
    private float attackCD;
    [SerializeField]
    private float attackStartupBase;
    private float attackStartup;
    [SerializeField]
    private float attackRecoverBase;
    private float attackRecover;
    [SerializeField]
    private float attackDurationBase;
    private float attackDuration;

    [SerializeField]
    private float attackRange;
    [SerializeField]
    private float attackSize;


    //Collision checks
    private bool hitPlayer;
    private bool playerInRange;
    private Vector3 toPlayer;

    private float rotationSpd;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        //Initalization
        currentAIState = BasicEnemyAIStates.CHASE;
        navMeshAgent.SetDestination(player.transform.position);
        currentAttackState = BasicEnemyAttackStates.COOLDOWN;

        rotationSpd = navMeshAgent.angularSpeed;

        //Attack timers
        attackCD = 0f;
        attackStartup = attackStartupBase;
        attackDuration = attackDurationBase;
        attackRecover = attackRecoverBase;
    }

    private void FixedUpdate()
    {
        toPlayer = player.transform.position - transform.position;
        //Checks
        playerInRange = Physics.Raycast(transform.position, toPlayer, attackRange, playerLayer);
        hitPlayer = Physics.CheckSphere(transform.position + transform.forward * attackRange, attackSize, playerLayer);
    }



    // Update is called once per frame
    void Update()
    {
        //Recuding attack cooldown
        attackCD -= Time.deltaTime;
        //Statemachine
        switch (currentAIState)
        {
            case BasicEnemyAIStates.CHASE:

                //Chasing player
                navMeshAgent.SetDestination(player.transform.position);

                //Attacking if in range
                if (playerInRange)
                {
                    currentAIState = BasicEnemyAIStates.ATTACK;
                    navMeshAgent.SetDestination(transform.position);
                }
                break;
            case BasicEnemyAIStates.ATTACK:
                //Statemachine for attack cycle
                switch (currentAttackState)
                {
                    case BasicEnemyAttackStates.COOLDOWN:

                        //Chaning state to chasing when out of range
                        if (!playerInRange)
                        {
                            currentAIState = BasicEnemyAIStates.CHASE;
                        }


                        if (!hitPlayer)
                        {
                            //rotation
                            Vector2 centerPoint = new Vector2(transform.position.x, transform.position.z);
                            Vector2 facingPoint = new Vector2(transform.forward.x, transform.forward.z);
                            Vector2 endPoint = centerPoint - new Vector2(player.transform.position.x, player.transform.position.z);
                            float angleDifference = Vector2.SignedAngle(facingPoint, endPoint);

                            Vector3 rotation = new Vector3(0f, (rotationSpd * Mathf.Sign(angleDifference)) * Time.deltaTime, 0f);
                            transform.Rotate(rotation);
                        }

                        if (attackCD < 0f)
                        {
                            currentAttackState = BasicEnemyAttackStates.STARTUP;
                            attackCD = attackCDBase;
                        }
                        break;
                    case BasicEnemyAttackStates.STARTUP:
                        attackStartup -= Time.deltaTime;
                        if (attackStartup < 0f)
                        {
                            currentAttackState = BasicEnemyAttackStates.ATTACK;
                            attackStartup = attackStartupBase;
                        }
                        break;
                    case BasicEnemyAttackStates.ATTACK:
                        attackDuration -= Time.deltaTime;
                        if (hitPlayer)
                        {
                            Debug.Log("HIT!"); //Do something when hitting player
                        }
                        if (attackDuration < 0f)
                        {
                            currentAttackState = BasicEnemyAttackStates.RECOVERY;
                            attackDuration = attackDurationBase;
                        }
                        break;
                    case BasicEnemyAttackStates.RECOVERY:
                        attackRecover -= Time.deltaTime;
                        if (attackRecover < 0f)
                        {
                            currentAttackState = BasicEnemyAttackStates.COOLDOWN;
                            attackRecover = attackRecoverBase;
                        }
                        break;
                }
                break;
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackSize);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackSize / 4);
    }
}
