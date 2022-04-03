using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    // Start is called before the first frame update
    void Start()
    {
        flashAnimator = flashPointLight.GetComponent<Animator>();
    }

    public void OnFlash()
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

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, flashRange);
    }
}
