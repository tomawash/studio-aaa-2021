using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class GhostGlimpseTrigger : MonoBehaviour
{
    [SerializeField]
    protected Transform triggerPoint;
    [SerializeField]
    protected Transform spawnPoint;
    [SerializeField]
    protected Transform endPoint;

    [SerializeField]
    protected GameObject glimpseObject;

    [SerializeField]
    protected bool oneUse;
    private bool used = false;

    [SerializeField]
    protected float glimpseSpeed = 100f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    protected GameObject SpawnGlimpse()
    {
        if (used == false || oneUse == false)
        {
            GameObject glimpseInstance = Instantiate(glimpseObject, spawnPoint.position, Quaternion.identity);
            NavMeshAgent glimpseAgent = glimpseInstance.GetComponent<NavMeshAgent>();
            glimpseAgent.SetDestination(endPoint.position);
            glimpseAgent.speed = glimpseSpeed;
            glimpseAgent.acceleration = glimpseSpeed;
            used = true;
            return glimpseInstance;
        }
        return null;
    }
}
