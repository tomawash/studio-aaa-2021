using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhostGlimpse : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
<<<<<<< HEAD
        Vector3 toDestination = navMeshAgent.destination - transform.position;
        if(toDestination.magnitude < 1.1f)
=======
        if(navMeshAgent.remainingDistance < 1.1f)
>>>>>>> main
        {
            Destroy(gameObject);
        }
    }
}
