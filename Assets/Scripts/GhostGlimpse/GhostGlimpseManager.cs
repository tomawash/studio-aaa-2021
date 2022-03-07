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

<<<<<<< HEAD
    private Transform[] glimpseTriggers;

    private Transform currentlyActiveTrigger = null;
=======
    [SerializeField]
    private GhostGlimpseTrigger[] glimpseTriggers;

    private GhostGlimpseTrigger currentlyActiveTrigger = null;
>>>>>>> main
    // Start is called before the first frame update
    void Start()
    {
        glimpseFrequency = glimpseFrequencyBase;
        closestTriggerRefresh = 0f;

<<<<<<< HEAD
        glimpseTriggers = new Transform[transform.childCount];
        //Initialzing list of children
        for (int i = 0; i < transform.childCount; i++)
        {
            glimpseTriggers[i] = transform.GetChild(i);
            glimpseTriggers[i].GetComponent<GhostGlimpseTrigger>().used = true;
        }
=======
        //Initialize triggers
        glimpseTriggers = GetComponentsInChildren<GhostGlimpseTrigger>();
>>>>>>> main
    }

    // Update is called once per frame
    void Update()
    {
        //Activating a glimpse trigger
        glimpseFrequency -= Time.deltaTime;
<<<<<<< HEAD
        if(glimpseFrequency < 0)
        {
            //updates active trigger after a certain amount of time
            closestTriggerRefresh -= Time.deltaTime;
            if(closestTriggerRefresh < 0)
=======
        if(glimpseFrequency <= 0)
        {
            //updates active trigger after a certain amount of time
            closestTriggerRefresh -= Time.deltaTime;
            if(closestTriggerRefresh <= 0)
>>>>>>> main
            {
                ActivateClosestTrigger();
                closestTriggerRefresh = closestTriggerRefreshBase;
            }

            //If trigger was hit reset the glimpse timer
<<<<<<< HEAD
            if(currentlyActiveTrigger.GetComponent<GhostGlimpseTrigger>().used == true)
            {
                //Resetting glimpse timer
=======
            if(currentlyActiveTrigger.used == true)
            {
                //Resetting glimpse timer
                currentlyActiveTrigger = null;
>>>>>>> main
                glimpseFrequency = glimpseFrequencyBase + (glimpseFrequencyVariation * Random.value);
                closestTriggerRefresh = 0;
            }

        }
    }

<<<<<<< HEAD
    private Transform ActivateClosestTrigger()
=======
    private GhostGlimpseTrigger ActivateClosestTrigger()
>>>>>>> main
    {
        //Finds the closest trigger
        float smallestDist = float.MaxValue;
        float currentdist;
<<<<<<< HEAD
        Transform currentClosestTrigger = null;

        for (int i = 0; i < glimpseTriggers.Length; i++)
        {
            Transform currentTrigger = glimpseTriggers[i];
            currentdist = (playerTransform.position - currentTrigger.position).magnitude;
            if (currentdist < smallestDist && currentTrigger.GetComponent<GhostGlimpseTrigger>().canTrigger)
=======
        GhostGlimpseTrigger currentClosestTrigger = null;

        for (int i = 0; i < glimpseTriggers.Length; i++)
        {
            GhostGlimpseTrigger currentTrigger = glimpseTriggers[i];
            currentdist = Vector3.Distance(playerTransform.position, currentTrigger.transform.position);
            if (currentdist < smallestDist && currentTrigger.canTrigger)
>>>>>>> main
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
<<<<<<< HEAD
            currentlyActiveTrigger.GetComponent<GhostGlimpseTrigger>().used = false;
=======
            currentlyActiveTrigger.used = false;
>>>>>>> main
        }

        if (currentClosestTrigger != currentlyActiveTrigger)
        {
            //Disabiling previous trigger
<<<<<<< HEAD
            currentlyActiveTrigger.GetComponent<GhostGlimpseTrigger>().used = true;

            //Setting new closest trigger
            currentlyActiveTrigger = currentClosestTrigger;

            //Activating clostest trigger
            currentlyActiveTrigger.GetComponent<GhostGlimpseTrigger>().used = false;
=======
            currentlyActiveTrigger.used = true;
            //Setting new closest trigger
            currentlyActiveTrigger = currentClosestTrigger;
            //Activating clostest trigger
            currentlyActiveTrigger.used = false;
>>>>>>> main
        }

        return currentClosestTrigger;
    }

    private void OnDrawGizmos()
    {
        if (currentlyActiveTrigger != null)
        {
            Gizmos.color = Color.magenta;
<<<<<<< HEAD
            Gizmos.DrawSphere(currentlyActiveTrigger.position, 1);
=======
            Gizmos.DrawSphere(currentlyActiveTrigger.transform.position, 1);
>>>>>>> main
        }
    }
}
