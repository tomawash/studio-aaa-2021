using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostGlimpseManager : MonoBehaviour
{
    [SerializeField]
    private float glimpseFrequencyBase;
    [SerializeField]
    private float glimpseFrequency;

    private float closestTriggerRefreshBase = 0.5f;
    [SerializeField]
    private float closestTriggerRefresh;

    [SerializeField]
    private float glimpseFrequencyVariation;

    [SerializeField]
    private Transform playerTransform;

    [SerializeField]
    private GhostGlimpseTrigger[] glimpseTriggers;

    private GhostGlimpseTrigger currentlyActiveTrigger = null;
    // Start is called before the first frame update
    void Start()
    {
        glimpseFrequency = glimpseFrequencyBase;
        closestTriggerRefresh = 0f;

        //Initialize triggers
        glimpseTriggers = GetComponentsInChildren<GhostGlimpseTrigger>();
    }

    // Update is called once per frame
    void Update()
    {
        //Activating a glimpse trigger
        glimpseFrequency -= Time.deltaTime;
        if(glimpseFrequency <= 0)
        {
            //updates active trigger after a certain amount of time
            closestTriggerRefresh -= Time.deltaTime;
            if(closestTriggerRefresh <= 0)
            {
                ActivateClosestTrigger();
                closestTriggerRefresh = closestTriggerRefreshBase;
            }

            //If trigger was hit reset the glimpse timer
            if(currentlyActiveTrigger.used == true)
            {
                //Resetting glimpse timer
                currentlyActiveTrigger = null;
                glimpseFrequency = glimpseFrequencyBase + (glimpseFrequencyVariation * Random.value);
                closestTriggerRefresh = 0;
            }

        }
    }

    private GhostGlimpseTrigger ActivateClosestTrigger()
    {
        //Finds the closest trigger
        float smallestDist = float.MaxValue;
        float currentdist;
        GhostGlimpseTrigger currentClosestTrigger = null;

        for (int i = 0; i < glimpseTriggers.Length; i++)
        {
            GhostGlimpseTrigger currentTrigger = glimpseTriggers[i];
            currentdist = Vector3.Distance(playerTransform.position, currentTrigger.transform.position);
            if (currentdist < smallestDist && currentTrigger.canTrigger)
            {
                //Saving the possibly closest trigger
                smallestDist = currentdist;
                currentClosestTrigger = currentTrigger;
            }
        }

        //Setting active trigger if null
        if (currentlyActiveTrigger == null)
        {
            currentlyActiveTrigger = currentClosestTrigger;

            //Activating clostest trigger
            currentlyActiveTrigger.used = false;
        }

        if (currentClosestTrigger != currentlyActiveTrigger)
        {
            //Disabiling previous trigger
            currentlyActiveTrigger.used = true;
            //Setting new closest trigger
            currentlyActiveTrigger = currentClosestTrigger;
            //Activating clostest trigger
            currentlyActiveTrigger.used = false;
        }

        return currentClosestTrigger;
    }

    private void OnDrawGizmos()
    {
        if (currentlyActiveTrigger != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(currentlyActiveTrigger.transform.position, 1);
        }
    }
}
