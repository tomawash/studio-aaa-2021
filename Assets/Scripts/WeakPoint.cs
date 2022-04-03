using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    private SphereCollider sphereCollider;
    private float focusDownDuration;
    // Start is called before the first frame update
    void Start()
    {
        EnemyAI enemyAI = GetComponentInParent<EnemyAI>();
        sphereCollider = GetComponent<SphereCollider>();

        sphereCollider.radius = enemyAI.weakPointSize;
        focusDownDuration = enemyAI.focusDownDurationBase;
    }

    public void Hit()
    {
        focusDownDuration -= Time.fixedDeltaTime;
        if(focusDownDuration <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
