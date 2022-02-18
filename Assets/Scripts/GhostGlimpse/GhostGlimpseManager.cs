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

    private Transform[] glimpseTriggers;

    private Transform currentlyActiveTrigger = null;
    // Start is called before the first frame update
    void Start()
    {
        glimpseFrequency = glimpseFrequencyBase;
        closestTriggerRefresh = 0f;

        glimpseTriggers = new Transform[transform.childCount];
        //Initialzing list of children
        for (int i = 0; i < transform.childCount; i++)
        {
            glimpseTriggers[i] = transform.GetChild(i);
            glimpseTriggers[i].GetComponent<GhostGlimpseTrigger>().used = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Activating a glimpse trigger
        glimpseFrequency -= Time.deltaTime;
        if(glimpseFrequency < 0)
        {
            //updates active trigger after a certain amount of time
            closestTriggerRefresh -= Time.deltaTime;
            if(closestTriggerRefresh < 0)
            {
                ActivateClosestTrigger();
                closestTriggerRefresh = closestTriggerRefreshBase;
            }

            //If trigger was hit reset the glimpse timer
            if(currentlyActiveTrigger.GetComponent<GhostGlimpseTrigger>().used == true)
            {
                //Resetting glimpse timer
                glimpseFrequency = glimpseFrequencyBase + (glimpseFrequencyVariation * Random.value);
                closestTriggerRefresh = 0;
            }

        }
    }

    private Transform ActivateClosestTrigger()
    {
        //Finds the closest trigger
        float smallestDist = float.MaxValue;
        float currentdist;
        Transform currentClosestTrigger = null;

        for (int i = 0; i < glimpseTriggers.Length; i++)
        {
            Transform currentTrigger = glimpseTriggers[i];
            currentdist = (playerTransform.position - currentTrigger.position).magnitude;
            if (currentdist < smallestDist && currentTrigger.GetComponent<GhostGlimpseTrigger>().canTrigger)
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
            currentlyActiveTrigger.GetComponent<GhostGlimpseTrigger>().used = false;
        }

        if (currentClosestTrigger != currentlyActiveTrigger)
        {
            //Disabiling previous trigger
            currentlyActiveTrigger.GetComponent<GhostGlimpseTrigger>().used = true;

            //Setting new closest trigger
            currentlyActiveTrigger = currentClosestTrigger;

            //Activating clostest trigger
            currentlyActiveTrigger.GetComponent<GhostGlimpseTrigger>().used = false;
        }

        return currentClosestTrigger;
    }

    private void OnDrawGizmos()
    {
        if (currentlyActiveTrigger != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(currentlyActiveTrigger.position, 1);
        }
    }
}
