using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostGlimpseTriggerCollision : GhostGlimpseTrigger
{
    [SerializeField]
    private float spawnColDwnBase;
    private float spawnColDwn;
    // Start is called before the first frame update
    void Start()
    {
        spawnColDwn = spawnColDwnBase;
    }

    // Update is called once per frame
    void Update()
    {
        spawnColDwn -= Time.deltaTime;
        canTrigger = true; // For consistency
    }
    private void OnTriggerEnter(Collider other)
    {
        if (spawnColDwn < 0 && canTrigger)
        {
            SpawnGlimpse();
            spawnColDwn = spawnColDwnBase;
        }
    }
}
