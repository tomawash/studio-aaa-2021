using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    //Components
    private Health health;
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
    private float idleWaitBase = 5f;
    private float idleWait;

    //State Machines
    enum BasicEnemyAIStates
    {
        IDLE,
        CHASE,
        SEARCH,
        ATTACK,
        STUN,
        DEAD
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
    [SerializeField]
    private BasicEnemyAttackStates currentAttackState;
    private BasicEnemyIdleStates currentIdleState;
    private BasicEnemySearchStates currentSearchState;

    //Attack
    [System.Serializable]
    private struct AttackCycle
    {
        public float attackStartupBase;
        public float attackDurationBase;
        public float attackRecoverBase;
    }
    [SerializeField]
    private List<AttackCycle> attackCycles = new List<AttackCycle>();
    private int currentAttackCycle = 0;

    [SerializeField]
    private float attackCDBase = 3f;
    private float attackCD;
    private float attackStartup;
    private float attackDuration;
    private float attackRecover;

    [SerializeField]
    private float attackRange = 1f;
    [SerializeField]
    private float attackSize = 1f;
    private bool hitPlayer = false;

    //Damaging variables
    private float playerDamage;

    //Searching variables
    [SerializeField]
    private float lookingDurBase = 2;
    private float lookingDur;
    private int searchPointIndex = -1;

    [SerializeField]
    private float searchDistance = 10f;
    private List<Vector3> searchPoints = new List<Vector3>();

    //Seeing Variables
    [SerializeField]
    private float sightRange = 10f;
    private Vector3 lastSeen;

    //Collision checks
    private bool playerInSightRange;
    private bool playerInRange;
    private bool playerInHitbox;
    private Vector3 toPlayer;

    //Rotation
    private float rotationSpd;

    //Stunned
    private float stunnedDur = 0f;

    //Stun Weakpoints
    [SerializeField]
    public float weakPointSize = 0.3f;
    [SerializeField]
    public float focusDownDurationBase = 0.5f;
    private WeakPoint[] weakPoints;
    private bool damaged = true;

    //Speed variables
    [SerializeField]
    private float spd = 2f;
    private float chaseMulti = 1.75f;

    ///Variables
    [SerializeField]
    private float attackDamage = 5f;

    //Animation
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        //Getting Components
        health = GetComponent<Health>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if(animator == null)
        {
            animator = new Animator();
        }
        //Weakpoints
        weakPoints = GetComponentsInChildren<WeakPoint>();
        SetWeakpointsActive(false);

        //Initalization
        currentAIState = BasicEnemyAIStates.IDLE;
        currentAttackState = BasicEnemyAttackStates.COOLDOWN;
        currentIdleState = BasicEnemyIdleStates.STILL;
        currentSearchState = BasicEnemySearchStates.LOOK;

        rotationSpd = navMeshAgent.angularSpeed;
        navMeshAgent.speed = spd;

        //Setting timers
        idleWait = idleWaitBase;
        lookingDur = lookingDurBase;

        attackCD = 0f;
        attackStartup = 0f;
        attackDuration = 0f;
        attackRecover = 0f;
    }

    private void FixedUpdate()
    {
        //Calculating sight range
        toPlayer = player.transform.position - transform.position;

        //checking if player is within sight range
        Ray lookForPlayer = new Ray(transform.position + Vector3.up*0.25f, toPlayer); //added Vector3.up the origin of ray for better collisions
        if (Physics.Raycast(lookForPlayer, out RaycastHit hit, terrainPlayerLayer))
        {
            playerInSightRange = (hit.distance < sightRange && hit.transform.gameObject.name == player.GetComponentInChildren<CapsuleCollider>().transform.name);
        }

        //if player is within reasonable attack range
        playerInRange = Physics.Raycast(transform.position, toPlayer, attackRange + attackSize/1.5f, playerLayer);

        //if player is within attack hitbox
        playerInHitbox = Physics.CheckSphere(transform.position + transform.forward * attackRange, attackSize, playerLayer);
    }

    // Update is called once per frame
    void Update()
    {

        
        //Recuding attack cooldown
        attackCD -= Time.deltaTime;

        //Debug.Log(playerInSightRange);
        //Statemachine
        switch (currentAIState)
        {
            case BasicEnemyAIStates.IDLE:

                //Patroling between points state machine
                switch (currentIdleState)
                {
                    case BasicEnemyIdleStates.STILL:
                        //Standing still
                        idleWait -= Time.deltaTime;
                        if (idleWait <= 0f)
                        {
                            if (patrolPath != null)
                            {
                                //Setting new patrol point and start moving
                                currentIdleState = BasicEnemyIdleStates.WAYPOINT;
                                currentPoint = patrolPath.GetChild(patrolIndex);
                                navMeshAgent.SetDestination(currentPoint.position);
                                animator.SetTrigger("ToRun");
                            }
                            idleWait = idleWaitBase;
                        }
                        break;
                    case BasicEnemyIdleStates.WAYPOINT:

                        //Colliding with waypoints
                        Collider[] waypointContacts = Physics.OverlapSphere(transform.position, 1f, waypointLayer);

                        for (int i = 0; i < waypointContacts.Length; i++)
                        {
                            if (waypointContacts[i].name == currentPoint.name)
                            {
                                //Itterating patrol index
                                patrolIndex++;
                                if (patrolIndex > patrolPath.childCount - 1)
                                {
                                    patrolIndex = 0;
                                }

                                //Changing states
                                currentIdleState = BasicEnemyIdleStates.STILL;
                                animator.SetTrigger("ToIdle");

                                break;
                            }
                        }
                        break;
                }

                //Player in range chase
                if (playerInSightRange)
                {
                    ExitIdle();
                    EnterChase();
                }
                break;
            case BasicEnemyAIStates.CHASE:

                if (playerInSightRange)
                {
                    //AI Setting and remembering last seen
                    lastSeen = player.transform.position;
                    navMeshAgent.SetDestination(lastSeen);
                }
                else
                {
                    //Calculating search points
                    if (Vector3.Distance(transform.position, lastSeen) < 0.5f)
                    {
                        bool positiveFound = false;
                        bool negativeFound = false;

                        float angleCheck;
                        Vector3 checkAngle;

                        //Finding searchpoints starting from tangent line of toPlayer
                        for (int i = 90; i >= 0; i--)
                        {
                            angleCheck = i * (Mathf.PI / 180f);

                            if (!positiveFound)
                            {
                                //New Angle
                                checkAngle = RotateAngle(angleCheck);
                                Debug.DrawRay(lastSeen, checkAngle, Color.green, 15f);

                                //Checking for valid path
                                if(CheckNewPath(checkAngle))
                                {
                                    searchPoints.Add(transform.position + checkAngle);
                                    positiveFound = true;
                                }
                            }

                            if (!negativeFound)
                            {
                                //New Angle
                                checkAngle = RotateAngle(-angleCheck);
                                Debug.DrawRay(lastSeen, checkAngle, Color.red, 15f);

                                //Checking for valid path
                                if (CheckNewPath(checkAngle))
                                {
                                    searchPoints.Add(transform.position + checkAngle);
                                    negativeFound = true;
                                }
                            }
                        }

                        //If there are no valid search points return to idle
                        if (!positiveFound && !negativeFound)
                        {
                            EnterIdle();
                        }
                        else
                        {
                            EnterSearch();
                        }
                    }
                }

                //Attacking if in range
                if (playerInRange)
                {
                    EnterAttack();
                }
                break;
            case BasicEnemyAIStates.SEARCH:

                //Returning to chase when in range
                if (playerInSightRange)
                {
                    ExitSearch();
                    EnterChase();
                }

                //Search points state machine
                switch (currentSearchState)
                {
                    case BasicEnemySearchStates.LOOK:
                        lookingDur -= Time.deltaTime;

                        //Rotating to simulate looking
                        if (lookingDur > lookingDurBase * (1.0f / 2.0f))
                        {
                            Vector3 rotation = new Vector3(0f, -rotationSpd / 2 * Time.deltaTime, 0f);
                            transform.Rotate(rotation);
                        }
                        else
                        {
                            Vector3 rotation = new Vector3(0f, rotationSpd / 2 * Time.deltaTime, 0f);
                            transform.Rotate(rotation);
                        }

                        //Changing based on available search points
                        if (lookingDur <= 0f)
                        {
                            searchPointIndex++;

                            //End of search points
                            if (searchPointIndex > searchPoints.Count - 1)
                            {
                                ExitSearch();
                                EnterIdle();
                            }
                            else // Next search point
                            {
                                navMeshAgent.SetDestination(searchPoints[searchPointIndex]);
                                currentSearchState = BasicEnemySearchStates.SEARCHPOINT;
                            }
                            lookingDur = lookingDurBase;
                        }

                        break;

                    case BasicEnemySearchStates.SEARCHPOINT:

                        if (Vector3.Distance(transform.position, searchPoints[searchPointIndex]) < 0.5f)
                        {
                            currentSearchState = BasicEnemySearchStates.LOOK;
                        }

                        break;
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
                            ExitAttack();
                            EnterChase();
                        }

                        //rotation if player out of range
                        if (!playerInHitbox)
                        {
                            Vector2 centerPoint = new Vector2(transform.position.x, transform.position.z);
                            Vector2 facingPoint = new Vector2(transform.forward.x, transform.forward.z);
                            Vector2 endPoint = centerPoint - new Vector2(player.transform.position.x, player.transform.position.z);
                            float angleDifference = Vector2.SignedAngle(facingPoint, endPoint);

                            Vector3 rotation = new Vector3(0f, (rotationSpd * Mathf.Sign(angleDifference)) * Time.deltaTime, 0f);
                            transform.Rotate(rotation);
                        }
                        else
                        {
                            //Starting Attack
                            if (attackCD < 0f)
                            {
                                //State Change
                                currentAttackState = BasicEnemyAttackStates.STARTUP;

                                //Setting timers
                                attackCD = attackCDBase;
                                attackStartup = attackCycles[currentAttackCycle].attackStartupBase;

                                //Starting animation
                                animator.SetTrigger("ToAttack");
                            }
                        }
                        break;
                    case BasicEnemyAttackStates.STARTUP:
                        if (!animator.IsInTransition(0))
                        {
                            attackStartup -= Time.deltaTime;
                            if (attackStartup < 0f)
                            {
                                currentAttackState = BasicEnemyAttackStates.ATTACK;
                                attackDuration = attackCycles[currentAttackCycle].attackDurationBase;
                            }
                        }
                        break;
                    case BasicEnemyAttackStates.ATTACK:
                        attackDuration -= Time.deltaTime;
                        if (attackDuration < 0f)
                        {
                            currentAttackState = BasicEnemyAttackStates.RECOVERY;   
                            attackRecover = attackCycles[currentAttackCycle].attackRecoverBase;
                        }

                        //Hitting the player
                        if (playerInHitbox && !hitPlayer)
                        {
                            hitPlayer = true;
                            player.GetComponent<Health>().LoseHealth(attackDamage);
                        }
                        break;
                    case BasicEnemyAttackStates.RECOVERY:
                        attackRecover -= Time.deltaTime;
                        if (attackRecover < 0f)
                        {
                            hitPlayer = false;
                            currentAttackState = BasicEnemyAttackStates.COOLDOWN;

                            currentAttackCycle++;

                            if(currentAttackCycle >= attackCycles.Count)
                            {
                                currentAttackState = BasicEnemyAttackStates.COOLDOWN;
                                animator.SetTrigger("ToIdle");
                                currentAttackCycle = 0;
                            }
                            else
                            {
                                attackStartup = attackCycles[currentAttackCycle].attackStartupBase;
                                currentAttackState = BasicEnemyAttackStates.STARTUP;
                            }
                        }
                        break;
                }
                break;
            case BasicEnemyAIStates.STUN:

                //Transition out of stun after animation
                if (damaged)
                {
                    //After taking damage animation
                    if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
                    {
                        ExitStun();
                    }
                }

                //Hitting weakpoints and taking damage
                if (AllWeakpointsDisabled() && damaged == false)
                {
                    health.LoseHealth(playerDamage);
                    damaged = true;
                }

                //Ending Stun Duration
                stunnedDur -= Time.deltaTime;
                if (stunnedDur <= 0 && !damaged)
                {
                    ExitStun();
                }

                break;
            case BasicEnemyAIStates.DEAD:
                break;
        }
    }

    //State machine Transitions
    private void EnterIdle()
    {
        //State and navigation
        currentAIState = BasicEnemyAIStates.IDLE;
        navMeshAgent.speed = spd;
        navMeshAgent.SetDestination(transform.position);
        
        //Animation
        animator.SetTrigger("ToIdle");
        animator.speed = 1;
    }
    private void ExitIdle()
    {
        //Resetting current state
        currentIdleState = BasicEnemyIdleStates.STILL;
        idleWait = idleWaitBase;
    }
    private void EnterChase()
    {
        //Setting up next state
        currentAIState = BasicEnemyAIStates.CHASE;

        //Navigation
        navMeshAgent.SetDestination(lastSeen);
        navMeshAgent.speed = spd * chaseMulti;

        //Animation
        animator.SetTrigger("ToRun");
        animator.speed = chaseMulti;
    }
    private void EnterSearch()
    {
        //State and navigation
        currentAIState = BasicEnemyAIStates.SEARCH;
        navMeshAgent.SetDestination(transform.position);
        navMeshAgent.speed = spd;

        //Animation
        animator.SetTrigger("ToRun");
        animator.speed = 1;
    }
    private void ExitSearch()
    {
        //Exit Variables
        currentSearchState = BasicEnemySearchStates.LOOK;
        lookingDur = lookingDurBase;
        searchPoints.Clear();
        searchPointIndex = -1;
    }
    private void EnterAttack()
    {
        //State
        currentAIState = BasicEnemyAIStates.ATTACK;
        
        //Navigation
        navMeshAgent.SetDestination(transform.position);
        navMeshAgent.updateRotation = false;
        
        //Animation
        animator.SetTrigger("ToIdle");
        animator.speed = 1;
    }
    private void ExitAttack()
    {
        //Updating rotation
        navMeshAgent.updateRotation = true;
        hitPlayer = false;

        //Resetting attack timers
        attackStartup = 0f;
        attackDuration = 0f;
        attackRecover = 0f;
        currentAttackCycle = 0;
    }
    private void EnterStun(float duration)
    {
        //States
        ExitAnyState();
        currentAIState = BasicEnemyAIStates.STUN;
        navMeshAgent.SetDestination(transform.position);
        navMeshAgent.speed = 0;

        //Weakpoints
        SetWeakpointsActive(true);

        //Adjusting Variables
        stunnedDur = duration;
        damaged = false;

        //Animation
        animator.speed = 1;
        animator.SetTrigger("ToStun");
    }
    private void ExitStun()
    {
        EnterChase();
        SetWeakpointsActive(false);
    }
    private void EnterDead()
    {
        //Chaning States
        ExitAnyState();
        currentAIState = BasicEnemyAIStates.DEAD;

        //Animation
        //animator.SetTrigger("ToDie");
    }

    //Exit States
    private void ExitAnyState()
    {
        switch (currentAIState)
        {
            case BasicEnemyAIStates.IDLE:
                ExitIdle();
                break;
            case BasicEnemyAIStates.SEARCH:
                ExitSearch();
                break;
            case BasicEnemyAIStates.ATTACK:
                ExitAttack();
                break;
            case BasicEnemyAIStates.STUN:
                ExitStun();
                break;
        }
    }
    //Rotation
    private Vector3 RotateAngle(float angleCheck)
    {
        Vector3 lastSeenToPlayer = (player.transform.position - lastSeen).normalized * searchDistance;
        float checkX;
        float checkZ;

        //Matrix rotation
        checkX = lastSeenToPlayer.x * Mathf.Cos(angleCheck) - lastSeenToPlayer.z * Mathf.Sin(angleCheck);
        checkZ = lastSeenToPlayer.x * Mathf.Sin(angleCheck) + lastSeenToPlayer.z * Mathf.Cos(angleCheck);
        return new Vector3(checkX, lastSeenToPlayer.y, checkZ);
    }
    //Checking New Path
    private bool CheckNewPath(Vector3 checkAngle)
    {
        NavMeshPath newPath = new NavMeshPath();
        //Checking for valid complete paths
        NavMesh.CalculatePath(transform.position, transform.position + checkAngle, navMeshAgent.areaMask, newPath);
        if (newPath.status == NavMeshPathStatus.PathComplete)
        {
            return true;
        }
        return false;
    }
    //Weakpoints
    private void SetWeakpointsActive(bool condition)
    {
        for (int i = 0; i < weakPoints.Length; i++)
        {
            weakPoints[i].gameObject.SetActive(condition);
        }
    }
    private bool AllWeakpointsDisabled()
    {
        for (int i = 0; i < weakPoints.Length; i++)
        {
            if(weakPoints[i].gameObject.activeSelf)
            {
                return false;
            }
        }
        return true;
    }
    //Stunned
    public void Stun(float duration)
    {
        if (currentAIState != BasicEnemyAIStates.DEAD)
        {
            EnterStun(duration);
        }
    }
    //Dying
    public void Die()
    {
        EnterDead();
    }
    //Setting damage taken
    public void SetPlayerDamageTaken(float amount)
    {
        playerDamage = amount;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lastSeen, 1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackSize);
    }
}
