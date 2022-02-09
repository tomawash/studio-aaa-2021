using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class GhostGlimpseTriggerSight : GhostGlimpseTrigger
{
    //Trigger Timing
    [SerializeField]
    private float triggerDelayBase;
    private float triggerDelay;

    //Variables for seeing player
    [SerializeField]
    private Camera playerView;
    [SerializeField]
    private Transform playerTransform;
    [SerializeField]
    private LayerMask levelLayer;
    private Vector3 triggerToPlayer;
    private bool obstructedTrigger;
    private Vector3 spawnToPlayer;
    private bool obstructedSpawn;
    private Vector3 endToPlayer;
    private bool obstructedEnd;

    // Start is called before the first frame update
    void Start()
    {
        triggerDelay = triggerDelayBase;
    }

    private void FixedUpdate()
    {
        //Checking obstructions to line of sight to player
        triggerToPlayer = playerTransform.position - triggerPoint.position;
        obstructedTrigger = Physics.Raycast(transform.position, triggerToPlayer, triggerToPlayer.magnitude, levelLayer);

        spawnToPlayer = playerTransform.position - spawnPoint.position;
        obstructedSpawn = Physics.Raycast(transform.position, spawnToPlayer, spawnToPlayer.magnitude, levelLayer);

        endToPlayer = playerTransform.position - endPoint.position;
        obstructedEnd = Physics.Raycast(transform.position, endToPlayer, endToPlayer.magnitude, levelLayer);
    }

    // Update is called once per frame
    void Update()
    {
        //Checking for point in camera of player
        Vector3 triggerToCam= playerView.WorldToViewportPoint(triggerPoint.position);
        Vector3 spawnToCam = playerView.WorldToViewportPoint(spawnPoint.position);
        Vector3 endToCam = playerView.WorldToViewportPoint(endPoint.position);
        if (InSight(triggerToCam, obstructedTrigger) && !InSight(spawnToCam, obstructedSpawn) && !InSight(endToCam, obstructedEnd))
        {
            triggerDelay -= Time.deltaTime;
        }
        else
        {
            triggerDelay = triggerDelayBase;
        }
        if(triggerDelay < 0)
        {
            SpawnGlimpse();
            triggerDelay = triggerDelayBase;
        }
    }
    private bool InSight(Vector3 viewPortPoint, bool obstructed)
    {
        return viewPortPoint.x > 0 &&
            viewPortPoint.x < 1 &&
            viewPortPoint.y > 0 &&
            viewPortPoint.y < 1 &&
            viewPortPoint.z > 0 && 
            !obstructed;
    }
}
