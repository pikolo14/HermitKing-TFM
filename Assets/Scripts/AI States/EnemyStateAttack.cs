using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateAttack : EnemyState
{
    CrabController enemy;
    float nextAttackTime = 0;

    public EnemyStateAttack(CrabController contr)
    {
        enemy = contr;
    }

    //Atacamos en direccion al jugador
    public void UpdateState()
    {
        //Si la concha buscada no pertenece al enemigo (la ha soltado) se vuelve a pursue
        if(enemy.currTargetShell != null && enemy.currTargetShell != PlayerCrabController.player.shell)
        {
            enemy.currTarget = enemy.currTargetShell.transform;
            StartPursue();
            return;
        }

        if(enemy.currTarget != null)
        {
            //Se sigue dirigiendo al jugador y ataca de vez en cuando
            enemy.agent.destination = enemy.currTarget.position;
            if(nextAttackTime <= 0)
            {
                enemy.attackContr.Attack();
                nextAttackTime = Random.Range(enemy.attackMinDelay, enemy.attackMaxDelay);
            }
            else
            {
                nextAttackTime -= Time.deltaTime;
            }
        }
        else
        {
            StartWander();
        }
    }

    public void Impact() { }


    public void StartPursue()
    {
        
        enemy.currState = enemy.pursueState;
        //Debug.Log("Pursue");
    }

    public void StartWander() 
    {
        //enemy.agent.speed = enemy.wanderSpeed;
        enemy.currState = enemy.wanderState;
        //Debug.Log("Wander");
    }

    public void StartAttack() { }


    public void OnTriggerEnter(Collider coll, EnemyState.TriggerType type) { }

    public void OnTriggerStay(Collider coll, EnemyState.TriggerType type) { }

    //Cuando el jugador sale de su radio de accion vuelve a modo alerta
    public void OnTriggerExit(Collider coll, EnemyState.TriggerType type) 
    {
        if (coll.CompareTag(Globals.tagPlayer))
        {
            StartPursue();
        }
    }
}
