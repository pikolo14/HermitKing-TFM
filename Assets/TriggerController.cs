using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerController : MonoBehaviour
{
    private CrabController enemy;
    public Collider coll;
    [SerializeField]
    public EnemyState.TriggerType type;

    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponentInParent<CrabController>();
        coll.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        enemy.currState.OnTriggerEnter(other, type);
    }

    private void OnTriggerStay(Collider other)
    {
        enemy.currState.OnTriggerStay(other, type);
    }

    private void OnTriggerExit(Collider other)
    {
        if (type == EnemyState.TriggerType.FLEE && other.CompareTag(Globals.tagPlayer))
            enemy.currState.OnTriggerExit(other, type);
    }
}
