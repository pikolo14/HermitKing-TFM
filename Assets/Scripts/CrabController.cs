using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabController : MonoBehaviour
{

    //CONCHA
    [HideInInspector]
    public ShellController shell;
    public static float disconfortFactor = 0.5f;
    protected float discomfort = 0;

    //STATS
    [Header("Stats")]
    public float size;
    public int health;
    public int attack;
    public float shellWeight;

    //MOVIMIENTO
    [Header("Movement")]
    public float speed = 5f;
    public float turnSmoothTime = 0.2f;
    protected float turnSmoothVel;

    //DELAYS
    private bool hittable = false;
    private float hitDelay = 0.1f;


    // Update is called once per frame
    protected virtual void Update()
    {
        Move();
    }

    //Mover el cangrejo mediante IA
    protected virtual void Move()
    {
        //TODO: Comportamiento movimiento cangrejos IA
    }

    //Recibir un golpe y perder vida
    public void GetHit(int attack)
    {
        //Si se le puede golpear...
        if(hittable)
        {
            //Si tiene concha pierde los puntos de vida del ataque del enemigo
            if(shell != null)
            {
                health -= attack;
                //Si la salud llega a 0, se suelta la concha
                if(health - discomfort <= 0)
                {
                    DropShell();
                }
                //TODO: Efecto visual en la concha

                StartCoroutine(HitDelay());
            }
            //Si no tiene concha, muere
            else 
            {
                Defeat();
            }
        }
    }

    //Muerte del cangrejo
    protected virtual void Defeat()
    {
        Debug.Log("Un ermitaño ha muerto");
        Destroy(gameObject);
        //TODO
    }

    //Mantener un tiempo como intocable despues de ser golpeado para evitar golpes seguidos injustos
    IEnumerator HitDelay()
    {
        hittable = false;
        yield return new WaitForSeconds(hitDelay);
        hittable = true;
    }

    //Coger una nueva concha
    public void GetShell(ShellController _shell)
    {
        if(_shell.GetDisconfort(size) == 0)
        {
            shell = _shell;
            health = shell.health;
            shellWeight = shell.weight;

            //TODO: Poner concha en la espalda
        }
    }

    public void UpdateSpeed()
    {
        //TODO: Cambiar velocidad de agente en función de weight y speed
    }

    //Dejar la concha equipada en el sitio
    public void DropShell()
    {
        shellWeight = 0;
        //TODO: Dejar caer objeto concha
        shell = null;
    }
}