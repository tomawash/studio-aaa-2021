using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTrap : MonoBehaviour
{
    [SerializeField]
    private bool destroyOnRelease = true;
    [SerializeField]
    private float trapDuration;
    private EnemyAI trappedGhost;
    private IEnumerable OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<EnemyAI>(out trappedGhost))
        {
            trappedGhost.Trap(true);
            yield return new WaitForSeconds(trapDuration);
            trappedGhost.Trap(false);
            trappedGhost = null;
        }
    }
    private void OnDisable()
    {
        //Release trapped ghost if trap is disabled to prevent the enemy from being trapped in place permanently.
        if (trappedGhost)
            trappedGhost.Trap(false);
    }
}
