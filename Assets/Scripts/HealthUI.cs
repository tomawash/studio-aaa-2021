using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField]
    private Image healthBar;
    private Health health;
    // Start is called before the first frame update
    void Start()
    {
        health = GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.transform.localScale = new Vector3(Mathf.Min(1f, health.currentHealth / health.maxHealth), 1f, 1f);
    }
}
