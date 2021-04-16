using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface EnemyState
{
    public enum TriggerType
    {
        ATTACK = 0,
        DETECT = 1,
        FLEE = 2
    }

    void UpdateState();

    void StartWander();
    void StartPursue();
    void StartAttack();

    void OnTriggerEnter(Collider coll, TriggerType type);
    void OnTriggerStay(Collider coll, TriggerType type);
   void OnTriggerExit(Collider coll, TriggerType type);

    void Impact();
}
