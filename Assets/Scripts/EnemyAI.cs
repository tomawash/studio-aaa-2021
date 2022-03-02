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
    private LayerMask terrainPlayerLayer, playerLayer, waypointLayer;

    //Patrol pathing and idle state
    [SerializeField]
    private Transform patrolPath;
    private Transform currentPoint;
    private int patrolIndex = 0;

    [SerializeField]
    private float idleWaitBase = 10f;
    private float idleWait;

    //State Machines
    enum BasicEnemyAIStates
    {
        IDLE,
        CHASE,
        SEARCH,
        ATTACK
    }
    enum BasicEnemyAttackStates
    {
        COOLDOWN,
        STARTUP,
        ATTACK,
        RECOVERY
    }
    enum BasicEnemyIdleStates
    {
        STILL,
        WAYPOINT
    }
    enum BasicEnemySearchStates
    {
        LOOK,
        SEARCHPOINT
    }

    private BasicEnemyAIStates currentAIState;
    private BasicEnemyAttackStates currentAttackState;
    private BasicEnemyIdleStates currentIdleState;
    private BasicEnemySearchStates currentSearchState;

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
    private float sightRange;
    private Vector3 lastSeen;

    //Collision checks
    private bool playerInSightRange;
    private bool playerInRange;
    private bool hitPlayer;
    private Vector3 toPlayer;

    //Rotation
    private float rotationSpd;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        //Initalization
        currentAIState = BasicEnemyAIStates.IDLE;
        currentAttackState = BasicEnemyAttackStates.COOLDOWN;
        currentIdleState = BasicEnemyIdleStates.STILL;
        currentSearchState = BasicEnemySearchStates.LOOK;

        rotationSpd = navMeshAgent.angularSpeed;

        //Setting timers
        idleWait = idleWaitBase;

        attackCD = 0f;
        attackStartup = attackStartupBase;
        attackDuration = attackDurationBase;
        attackRecover = attackRecoverBase;
    }

    private void FixedUpdate()
    {
        //Checks
        RaycastHit seePlayer;
        Ray lookForPlayer = new Ray(transform.position, toPlayer);
        Physics.Raycast(lookForPlayer, out seePlayer, terrainPlayerLayer);
        if (seePlayer.transform != null)
        {
            playerInSightRange = (seePlayer.distance < sightRange && seePlayer.transform.gameObject.name == player.transform.GetChild(1).name);
        }
        playerInRange = Physics.Raycast(transform.position, toPlayer, attackRange, playerLayer);
        hitPlayer = Physics.CheckSphere(transform.position + transform.forward * attackRange, attackSize, playerLayer);
    }

    // Update is called once per frame
    void Update()
    {
        //Calculating sight range
        toPlayer = player.transform.position - transform.position;

        //Recuding attack cooldown
        attackCD -= Time.deltaTime;
        //Statemachine
        switch (currentAIState)
        {
            case BasicEnemyAIStates.IDLE:
                switch (currentIdleState) {
                    case BasicEnemyIdleStates.STILL:
                        //Standing still
                        idleWait -= Time.deltaTime;
                        if (idleWait <= 0f)
                        {
                            if (patrolPath != null)
                            {
                                currentIdleState = BasicEnemyIdleStates.WAYPOINT;
                                currentPoint = patrolPath.GetChild(patrolIndex);
                                navMeshAgent.SetDestination(currentPoint.position);
                            }
                            idleWait = idleWaitBase;
                        }
                        break;
                    case BasicEnemyIdleStates.WAYPOINT:
                        Collider[] waypointContacts = Physics.OverlapSphere(transform.position, 0.5f, waypointLayer);

                        for (int i = 0; i < waypointContacts.Length; i++)
                        {
                            if (waypointContacts[i].name == currentPoint.name)
                            {
                                //Setting new patrol index
                                patrolIndex++;
                                if (patrolIndex > patrolPath.childCount - 1)
                                {
                                    patrolIndex = 0;
                                }

                                //Changing states
                                currentIdleState = BasicEnemyIdleStates.STILL;
                            }
                        }
                        break;
                }

                //Player in range chase
                if (playerInSightRange)
                {
                    currentIdleState = BasicEnemyIdleStates.STILL;
                    currentAIState = BasicEnemyAIStates.CHASE;

                    //Resetting current state
                    idleWait = idleWaitBase;

                    //Setting up next state
                    lastSeen = player.transform.position;
                    navMeshAgent.SetDestination(lastSeen);
                }
                break;
            case BasicEnemyAIStates.CHASE:

                if(playerInRange)
                {
                    //AI Setting and remembering last seen
                    lastSeen = player.transform.position;
                }
                else
                {
                    //Travelling to last seen location
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
                    navMeshAgent.updateRotation = false;
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
                            navMeshAgent.updateRotation = true;
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
                        else
                        {
                            if (attackCD < 0f)
                            {
                                currentAttackState = BasicEnemyAttackStates.STARTUP;
                                attackCD = attackCDBase;
                            }
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
        Gizmos.color = Color.white;
        Gizmos.DrawRay(new Ray(transform.position, (player.transform.position - transform.position)));
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lastSeen, 1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackSize);
    }
}
