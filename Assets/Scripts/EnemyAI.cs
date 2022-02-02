using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private LayerMask groundLayer, playerLayer, waypointLayer;

    //Patrol pathing
    [SerializeField]
    private Transform patrolPath;
    private int patrolIndex = 0;

    //State Machine
    enum BasicEnemyAIStates
    {
        IDLE,
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

    //Seeing Variables
    [SerializeField]
    private float sightRangeBase;
    private float sightRange;
    private Vector3 lastSeen;

    //Collision checks
    private bool playerInSightRange;
    private bool terrainInRange;
    private bool playerInRange;
    private bool hitPlayer;
    private Vector3 toPlayer;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        //Initalization
        currentAIState = BasicEnemyAIStates.IDLE;
        currentAttackState = BasicEnemyAttackStates.COOLDOWN;

        sightRange = sightRangeBase;
        attackCD = 0f;
        attackStartup = attackStartupBase;
        attackDuration = attackDurationBase;
        attackRecover = attackRecoverBase;
    }

    private void FixedUpdate()
    {
        //Checks
        playerInSightRange = Physics.Raycast(transform.position, toPlayer, sightRange, playerLayer);
        terrainInRange = Physics.Raycast(transform.position, toPlayer, sightRange, groundLayer);
        playerInRange = Physics.CheckSphere(transform.position + transform.forward * attackRange, attackSize / 4, playerLayer);
        hitPlayer = Physics.CheckSphere(transform.position + transform.forward * attackRange, attackSize, playerLayer);
    }

    // Update is called once per frame
    void Update()
    {
        //Calculating sight range
        toPlayer = player.transform.position - transform.position;
        if (toPlayer.magnitude < sightRange)
        {
            sightRange = toPlayer.magnitude;
        }
        else
        {
            sightRange = sightRangeBase;
        }

        //Recuding attack cooldown
        attackCD -= Time.deltaTime;
        //Statemachine
        switch (currentAIState)
        {
            case BasicEnemyAIStates.IDLE:
                //Player in ranage chase
                if(playerInSightRange && !terrainInRange)
                {
                    currentAIState = BasicEnemyAIStates.CHASE;
                    lastSeen = player.transform.position;
                    navMeshAgent.SetDestination(lastSeen);
                }
                break;
            case BasicEnemyAIStates.CHASE:
                if (playerInSightRange)
                {
                    if (terrainInRange)
                    {
                        //Travelling to last seen location
                        if (navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
                        {
                            currentAIState = BasicEnemyAIStates.IDLE;
                        }
                    }
                    else
                    {
                        //AI Setting and remembering last seen
                        lastSeen = player.transform.position;
                    }
                }
                else
                {
                    //Traveling to last seen location
                    if (navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
                    {
                        currentAIState = BasicEnemyAIStates.IDLE;
                    }
                }
                
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
                        if(!playerInRange)
                        {
                            currentAIState = BasicEnemyAIStates.CHASE;
                            lastSeen = player.transform.position;
                            navMeshAgent.SetDestination(lastSeen);
                        }

                        if(attackCD < 0f)
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
                        if (attackDuration < 0f)
                        {
                            if (hitPlayer)
                            {
                                Debug.Log("HIT!"); //Do something when hitting player
                            }
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


        //Old AI, need to use waypoint/patroling stuff

        //if (playerInRange && !terrainInRange)
        //{
        //    Debug.Log("chasing");
        //    foundPlayer = true;
        //    lastSeen = player.transform;
        //    navMeshAgent.SetDestination(player.transform.position);
        //}
        //else if(foundPlayer)
        //{
        //    navMeshAgent.SetDestination(transform.position);
        //}
        //else if (patrolPath != null ){
        //    Transform currentPoint = patrolPath.GetChild(patrolIndex);
        //    navMeshAgent.SetDestination(currentPoint.position);
        //    Collider[] waypointContacts = Physics.OverlapSphere(transform.position, 0.5f, waypointLayer);

        //    Debug.Log(patrolIndex);

        //    for (int i = 0; i < waypointContacts.Length; i++)
        //    {
        //        Debug.Log(waypointContacts[i].name + " : " + currentPoint.name);
        //        if (waypointContacts[i].name == currentPoint.name)
        //        {
        //            patrolIndex++;
        //            if(patrolIndex > patrolPath.childCount-1)
        //            {
        //                patrolIndex = 0;
        //            }
        //        }
        //    }
        //}
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.DrawRay(new Ray(transform.position, (player.transform.position - transform.position)));
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, sightRangeBase);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lastSeen, 1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackSize);
    }
}
