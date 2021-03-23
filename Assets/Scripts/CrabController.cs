using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabController : MonoBehaviour
{
    protected  AttackController attackContr;

    //CONCHA
    [HideInInspector]
    public ShellController shell;
    public static float disconfortFactor = 0.5f;
    protected float discomfort = 0;

    //STATS
    [Header("Stats")]
    public float size;
    public int health;
    
    public float shellWeight;

    //MOVIMIENTO
    [Header("Movement")]
    public float speed = 5f;
    public float turnSmoothTime = 0.2f;
    protected float turnSmoothVel;
    //[HideInInspector]
    public Rigidbody rb;

    //DELAYS
    protected bool hittable = true;
    protected float hitDelay = 0.1f;
    private float attackCurrDelay = 0;
    public float attackMinDelay = 1, attackMaxDelay = 4;

    //DETECCION DEL JUGADOR
    [Header("Player detection")]
    public float attackDist = 1;
    public float detectDist = 5;
    public float detectAngle = 40;

    //DEFENSA
    public float defenceDuration = 0.5f;
    protected bool defending = false;



    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        attackContr = GetComponentInChildren<AttackController>();
        shell = GetComponentInChildren<ShellController>();
        hittable = true;
    }

    private void Update()
    {
        float dist = Vector3.Distance(PlayerCrabController.player.transform.position, transform.position);

        //El enemigo está en el rango de vision?
        //TODO

        //Ataca si no se está atacando...
        if(!attackContr.IsAttacking())
        {
            //...y si el jugador está lo suficientemente cerca y el ataque no esta en cooldown
            if (dist < attackDist && attackCurrDelay <= 0)
            {
                attackContr.Attack();
                Debug.Log("Ataca enemigo");
                //Delay random en un rango para el siguiente ataque automático
                attackCurrDelay = Random.Range(attackMinDelay, attackMaxDelay);
            }
            else
                attackCurrDelay -= Time.deltaTime;
        }
    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
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
        //Si se está defendiendo se esquiva el golpe
        if(defending)
        {
            Debug.Log("DEFENSA UH UH");
        }
        //Si se le puede golpear...
        else if(hittable)
        {
            Debug.Log("Hit: "+attack);
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

    //Activar defensa temporalmente
    public void Defence()
    {
        if(!attackContr.IsAttacking() && !defending)
        {
            StartCoroutine(DefenceDuration(defenceDuration));
        }
    }

    protected IEnumerator DefenceDuration(float time)
    {
        defending = true;
        //TODO: Aimación de defensa

        Material mat = GetComponentInChildren<Renderer>().material;
        Color prev = mat.color;
        mat.color = Color.blue;
        yield return new WaitForSeconds(time);

        mat.color = prev;
        defending = false;
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