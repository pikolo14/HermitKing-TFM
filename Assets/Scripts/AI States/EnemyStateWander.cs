using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateWander : EnemyState
{
    CrabController enemy;
    private float remainingWait = 0;

    public EnemyStateWander(CrabController contr)
    {
        enemy = contr;
    }

    //El cangrejo se mueve con objetivos y pausas aleatorios
    public void UpdateState()
    {
        //Si estamos cerca del punto de destino actual calculamos un nuevo punto aleatorio
        if(enemy.agent.destination == null || enemy.agent.remainingDistance <= enemy.agent.stoppingDistance)
        {
            //Obtenemos una direccion aleatoria entre el angulo maximo escogido respecto al forward
            Vector3 direction =
            Quaternion.AngleAxis(Random.Range(-enemy.wanderTargetAngle, enemy.wanderTargetAngle), Vector3.up) * enemy.transform.forward;
            direction.Normalize();
            //Nos aseguramos de que sea paralelo al suelo
            direction.y = enemy.transform.position.y;
            //Lejania pseudoaleatoria del siguiente punto
            direction *= Random.Range(enemy.wanderTargetMinDist*enemy.size, enemy.wanderTargetMaxDist*enemy.size);
            direction.y = 0;
            Vector3 point = direction + enemy.agent.transform.position;
            point.y = enemy.transform.position.y;
            enemy.agent.destination = point;

            //Cambiamos la velocidad a más lento para caminar
            enemy.agent.speed = enemy.slowSpeedMult * enemy.baseSpeed * enemy.size;

            //¿Hacemos una parada aleatoria al llegar a este punto?
            if (Random.Range(0f, 1f) > enemy.wanderWaitProbability)
            {
                //Parada de tiempo aleatorio
                remainingWait = Random.Range(enemy.wanderWaitMinTime, enemy.wanderWaitMaxTime);
            }
        }

        //Mientras dure la pausa paramos al agente
        if(remainingWait > 0)
        {
            enemy.agent.isStopped = true;
            remainingWait -= Time.deltaTime;
        }
        else
        {
            enemy.agent.isStopped = false;
        }
    }

    //Al recibir un impacto perseguimos al jugador marcandolo como objetivo (pero sin interesarnos su concha)
    //TODO: ¿y si esta fuera de deteccion ya se volverá a wander desde el pursue?
    public void Impact()
    {
        enemy.currTarget = PlayerCrabController.player.transform;
        enemy.currTargetShell = null;
        StartPursue();
    }

    //Al pasar a pursue aumentamos la velocidad y nos cercioramos que el agente sigue un objetivo
    public void StartPursue()
    {
        enemy.agent.isStopped = false;
        //enemy.agent.speed = enemy.pursueSpeed;
        enemy.currState = enemy.pursueState;
        //Debug.Log("Pursue");
    }

    public void StartAttack() {}

    public void StartWander() {}


    public void OnTriggerEnter(Collider coll, EnemyState.TriggerType type) { }

    public void OnTriggerStay(Collider coll, EnemyState.TriggerType type)
    {
        switch (type)
        {
            //Detectamos mejores conchas 
            case EnemyState.TriggerType.DETECT:

                ShellController shell = null;

                //Obtenemos la concha en cuestion, deshabitada o la del jugador
                if (coll.gameObject.CompareTag(Globals.tagShell) && coll.gameObject.name != Globals.finalShell)
                {
                    //TODO: Optimizar para no tener que usar en cada iteración
                    shell = coll.GetComponent<ShellController>();
                }
                else if(coll.gameObject.CompareTag(Globals.tagPlayer))
                {
                    shell = PlayerCrabController.player.shell;
                }
                else
                {
                    return;
                }
                    
                //Si es una concha mayor que puede habitar, la fija como objetivo
                if (shell != null && shell.GetDisconfort(enemy.size) == 0 && (enemy.shell == null || shell.size > enemy.shell.size))
                {
                    StartPursue();
                    //Concha deshabitada
                    if(shell.transform.parent == null)
                    {
                        enemy.currTarget = coll.transform;
                    }
                    //Concha habitada por jugador
                    else if (shell == PlayerCrabController.player.shell)
                    {
                        enemy.currTarget = PlayerCrabController.player.transform;
                    }
                    //Evitamos las conchas de cangrejos enemigos
                    else
                    {
                        return;
                    }

                    enemy.currTargetShell = shell;
                    StartPursue();
                }
                break;

            //Si el jugador se acerca demasiado a un cangrejo grande este pasará a perseguirlo
            case EnemyState.TriggerType.ATTACK:
                if(coll.CompareTag(Globals.tagPlayer))
                {
                    enemy.currTarget = PlayerCrabController.player.transform;
                    StartPursue();
                }
                break;
        }
    }

    public void OnTriggerExit(Collider coll, EnemyState.TriggerType type) { }
}
