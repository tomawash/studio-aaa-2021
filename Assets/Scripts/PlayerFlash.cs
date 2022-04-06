using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerFlash : MonoBehaviour
{
    //Layers
    [SerializeField]
    private LayerMask groundLayer, enemyLayer;

    //Light
    [SerializeField]
    private Light flashPointLight;
    private Animator flashAnimator;

    //Flash Variables
    [SerializeField]
    private float flashRange;
    [SerializeField]
    private float stunDuration;
    [SerializeField]
    private float flashCooldownBase;
    private float flashCooldown;

    //UI
    [SerializeField]
    private Image flashCooldownBar;

    // Start is called before the first frame update
    void Start()
    {
        //Getting Components
        flashAnimator = flashPointLight.GetComponent<Animator>();

        //Setting timers
        flashCooldown = flashCooldownBase;
    }

    private void Update()
    {
        //Reducing CD
        flashCooldown -= Time.deltaTime;

        //UI
        flashCooldownBar.transform.localScale = new Vector3(1f - Mathf.Max(0f, flashCooldown / flashCooldownBase), 1f, 1f);
    }

    public void OnFlash()
    {
        if (flashCooldown <= 0)
        {
            //Flash Animation
            flashAnimator.SetTrigger("ToFlash");

            //Checking for enemy
            RaycastHit[] enemyHit = Physics.SphereCastAll(transform.position, flashRange, Vector3.forward, flashRange);
            EnemyAI enemyAI = null;
            for (int i = 0; i < enemyHit.Length; i++)
            {
                enemyHit[i].transform.TryGetComponent<EnemyAI>(out enemyAI);

                //Checking for hitting enemy
                if (enemyAI != null)
                {
                    Vector3 toEnemy = enemyAI.transform.position - transform.position;

                    //If its not obstructed
                    if (!Physics.Raycast(transform.position, toEnemy, toEnemy.magnitude, groundLayer))
                    {
                        enemyAI.Stun(stunDuration);
                    }

                    break;
                }
            }

            //Resetting CD
            flashCooldown = flashCooldownBase;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, flashRange);
    }
}
