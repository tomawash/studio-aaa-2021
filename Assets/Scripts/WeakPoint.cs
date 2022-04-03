using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    private SphereCollider sphereCollider;
    private float focusDownDuration;
    private EnemyAI enemyAI;
    // Start is called before the first frame update
    void Start()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
        sphereCollider = GetComponent<SphereCollider>();

        sphereCollider.radius = enemyAI.weakPointSize;
        focusDownDuration = enemyAI.focusDownDurationBase;
    }

    public void Hit(float damageAmount)
    {
        focusDownDuration -= Time.fixedDeltaTime;
        if(focusDownDuration <= 0)
        {
            enemyAI.SetPlayerDamageTaken(damageAmount);
            gameObject.SetActive(false);
        }
    }
}
