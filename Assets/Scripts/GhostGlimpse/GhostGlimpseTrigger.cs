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
    protected bool oneUse = true;
    public bool used = true;
    public bool canTrigger = false;

    [SerializeField]
    protected float glimpseSpeed = 100f;

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
