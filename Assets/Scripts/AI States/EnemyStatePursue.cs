using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatePursue : EnemyState
{
    CrabController enemy;

    public EnemyStatePursue(CrabController contr)
    {
        enemy = contr;
    }

    //Se dirige a su objetivo
    public void UpdateState()
    {
        //Si el jugador es el objetivo y se busca su concha y ha perdido su concha el objetivo ahora es la concha
        if(enemy.currTarget == PlayerCrabController.player.transform &&
            enemy.currTargetShell != null && PlayerCrabController.player.shell != enemy.currTargetShell)
        {
            enemy.currTarget = enemy.currTargetShell.transform;
        }

        //Si tiene objetivo y si se busca una concha que no es de un enemigo o se va a por el enemigo directamente (es mas grande y ha sido atacado) se dirige al objetivo
        if (enemy.currTarget != null && (enemy.currTargetShell != null && (enemy.currTargetShell.transform.parent == null || enemy.currTargetShell.transform.parent == PlayerCrabController.player.transform))
        ||(enemy.currTargetShell == null && enemy.currTarget == PlayerCrabController.player.transform))
            enemy.agent.destination = enemy.currTarget.position;
        //En cualquier otro caso volvemos a wander
        else
            StartWander();
    }

    public void Impact() { }

    public void StartPursue() { }

    public void StartAttack()
    {
        enemy.currState = enemy.attackState;
        Debug.Log("Attack");
    }

    public void StartWander() 
    {
        //enemy.agent.speed = enemy.wanderSpeed;
        enemy.currTarget = null;
        enemy.currTargetShell = null;
        enemy.currState = enemy.wanderState;
        //Debug.Log("Wander");
    }

    public void OnTriggerEnter(Collider coll, EnemyState.TriggerType type) 
    {
        if (type == EnemyState.TriggerType.ATTACK)
        {
            //Cuando el jugador entra en su radio de ataque se pasa a atacar
            if (coll.CompareTag(Globals.tagPlayer))
            {
                StartAttack();
            }
            //Si la concha deshabitada a por la que se dirigia entra en el radio de ataque se la pone y vuelve a wander
            else if (coll.CompareTag(Globals.tagShell) && coll.transform == enemy.currTarget)
            {
                enemy.DropShell();
                enemy.GetShell(coll.GetComponent<ShellController>());
                StartWander();
            }
        }
    }

    //Si esta cerca el jugador o una concha...
    public void OnTriggerStay(Collider coll, EnemyState.TriggerType type) { }

    //Si está persiguiendo al jugador y sale de su radio de huida se vuelve a wander
    public void OnTriggerExit(Collider coll, EnemyState.TriggerType type) 
    { 
        if(type == EnemyState.TriggerType.FLEE && enemy.currTarget == PlayerCrabController.player.transform && coll.CompareTag(Globals.tagPlayer))
        {
            StartWander();
        }
    }
}
