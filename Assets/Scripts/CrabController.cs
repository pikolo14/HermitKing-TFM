using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class CrabController : MonoBehaviour
{
    [HideInInspector]
    public AttackController attackContr;
    protected Rigidbody rb;
    [HideInInspector]
    public Animator animator;

    //SALUD
    [Header("HEALTH")]
    public int health;

    //TAMAÑO
    [Header("SIZE")]
    public float size;
    protected Vector3 initBodyScale;
    protected Transform body;
    
    //CONCHA
    [Header("SHELL")]
    public Transform shellPoint;
    [HideInInspector]
    public ShellController shell;
    public static float disconfortFactor = 0.5f;
    protected float discomfort = 0;

    //MOVIMIENTO y HABILIDADES
    [Header("MOVEMENT AND SKILLS")]
    public float speed = 5f;
    public float turnSmoothTime = 0.2f;
    protected float turnSmoothVel;
    protected bool defending = false;

    //PESO
    [Header("WEIGHT")]
    public float shellWeight = 0;
    public float maxWeight = 10;
    public float minSpeedMult = 0.2f, maxSpeedMult = 1f;
    protected float speedWeightFactor = 1;

    //DELAYS
    [Header("DELAYS/DURATIONS")]
    protected bool hittable = true;
    protected float hitDelay = 0.1f;
    public float attackMinDelay = 1, attackMaxDelay = 4;
    public float defenceDuration = 0.5f;

    //DETECCION DEL JUGADOR
    [Header("IA DETECTION")]
    public float attackDist = 1;
    public float detectDist = 5;
    public float detectAngle = 40;

    [Header("IA")]
    //Triggers para detectar entorno en disitintos estados
    public NavMeshAgent agent;
    public float wanderSpeed, wanderTargetMinDist, wanderTargetMaxDist, wanderTargetAngle;
    [Range(0,1)]
    public float wanderWaitProbability; //Valor random que altera la probabilidad de hacer esperas en el wander del cangrejo
    public float wanderWaitMinTime, wanderWaitMaxTime;
    public float pursueSpeed;
    [SerializeField]
    public Transform currTarget;
    public ShellController currTargetShell;
    
    //Estados que componen la maquina de estados de la IA
    [HideInInspector]
    public EnemyState currState;
    [HideInInspector]
    public EnemyStateWander wanderState;
    [HideInInspector]
    public EnemyStatePursue pursueState;
    [HideInInspector]
    public EnemyStateAttack attackState;

    [Header("PROCEDURAL ANIMATION")]
    public Transform tipsEffectorsPrefab;
    protected Transform tipsEffectors; //Puntos reales en el mundo en los que se sitúan las puntas de las patas (si están lo suficientemente cerca como para llegar)
    public Transform[] projectPointsTips; //Puntos emparentados con el cangrejo donde idealmente deberían estar situadas las puntas de las patas
    protected Coroutine[] tipMovementCorr;
    protected TwoBoneIKConstraint[] legConstraints;
    public RigBuilder rigBuilder;
    public float legMaxDist = 0.4f;
    public float effectorMoveTime = 0.2f;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        attackContr = GetComponentInChildren<AttackController>();
        shell = GetComponentInChildren<ShellController>();
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        body = transform.GetChild(0);

        //Preparacion effectors animacion
        tipsEffectors = Instantiate(tipsEffectorsPrefab);
        tipsEffectors.position = transform.position;
        tipsEffectors.rotation = transform.rotation;

        legConstraints = GetComponentsInChildren<TwoBoneIKConstraint>();
        for (int i = 0; i < legConstraints.Length; i++)
        {
            legConstraints[i].data.target = tipsEffectors.GetChild(i);
        }
        //La asignacion de targets debe hacerse antes de activar el rigBuilder
        rigBuilder.enabled = true;


        hittable = true;
        initBodyScale = body.localScale;
        UpdateSpeed();

        //Preparamos la máquina de estados
        wanderState = new EnemyStateWander(this);
        pursueState = new EnemyStatePursue(this);
        attackState = new EnemyStateAttack(this);
        currState = wanderState;
    }
    protected void Start()
    {
        //Inicializamos el tamaño para que inicialmente  al multiplicar por el factor se obtenga la escala original
        size = 1 / GameManager.gameManager.scaleFactor;

        tipMovementCorr = new Coroutine[projectPointsTips.Length];
    }

    private void Update()
    {
        //Actualizamos el estado de la maquina en el que estemos situados actualmente
        currState.UpdateState();
        MoveLegs();
    }

    protected void MoveLegs()
    {
        for (int i = 0; i<projectPointsTips.Length; i++)
        {
            Transform tip = tipsEffectors.GetChild(i);
            //Proyectar posición ideal de la patas sobre la capa del suelo exclusivamente
            Ray ray = new Ray(new Vector3(projectPointsTips[i].position.x, 100, projectPointsTips[i].position.z), Vector3.down);
            RaycastHit hit;

            int layerMask = (1 << LayerMask.NameToLayer("Ground"));
            Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
            Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.2f, Color.magenta);

            //Comprobamos si se supera el umbral de distancia desde la posicion actual de la punta hasta el punto proyectado
            float dist = Vector3.Distance(hit.point, tip.position);
            if (dist > legMaxDist)
            {
                //Lanzamos la corrutina de desplazar la pata progresivamente, si hay una corrutina en marcha se para antes para evitar solapes
                if(tipMovementCorr[i]!=null)
                {
                    StopCoroutine(tipMovementCorr[i]);
                    tipMovementCorr[i] = null;
                }
                tipMovementCorr[i] = StartCoroutine(EffectorDisplacement(tip, hit.point, effectorMoveTime, i));
                tip.position = hit.point;
            }
        }
    }

    //Funcion para desplazar progresivamente un effector de la animacion como la punta de la pata
    protected IEnumerator EffectorDisplacement(Transform effector, Vector3 target, float time, int id)
    {
        float currTime = 0;
        float multTime = 1 / time;//Multiplicar tiempo para que esté entre 0 y 1 al introducirlo en el lerp
        Vector3 origin = effector.position;

        while(currTime < 1)
        {
            currTime += Time.deltaTime * multTime;
            effector.position = Vector3.Lerp(origin, target, currTime);
            //yield return null;
            yield return new WaitForEndOfFrame();
        }
    }


    // Update is called once per frame
    protected virtual void FixedUpdate() { }
    
    
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

            if(currState != null)
                currState.Impact();

            //Si tiene concha pierde los puntos de vida del ataque del enemigo
            if(shell != null)
            {
                health -= attack;
                //Si la salud llega a 0, se suelta la concha
                if(health - discomfort <= 0)
                {
                    DropShell();
                }
                else
                {
                    //Aumentar grietas visibles en la concha
                    shell.SetCrackedMaterial(1-(health/(float)shell.health));
                }

                StartCoroutine(HitDelay());
            }
            //Si no tiene concha, muere
            else 
            {
                Defeat();
            }
        }
    }

    //Mantener un tiempo como intocable despues de ser golpeado para evitar golpes seguidos injustos
    IEnumerator HitDelay()
    {
        hittable = false;
        yield return new WaitForSeconds(hitDelay);
        hittable = true;
    }

    //Muerte del cangrejo
    protected virtual void Defeat()
    {
        Debug.Log("Un ermitaño ha muerto");
        Destroy(gameObject);
        //TODO
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
        //Aimación de defensa
        animator.SetBool(Globals.inputDefence, true);
        yield return new WaitForSeconds(0.15f);

        //Comenzar periodo defensa
        defending = true;
        //DEBUG
        Material mat = GetComponentInChildren<Renderer>().material;
        Color prev = mat.color;
        mat.color = Color.blue;
        yield return new WaitForSeconds(0.45f);

        //Terminar periodo defensa
        mat.color = prev;
        defending = false;
    }

    //Coger una nueva concha
    public void GetShell(ShellController _shell)
    {
        if(_shell.GetDisconfort(size) == 0)
        {
            //Asociamos la concha al cangrejo y la ponemos en un punto en la espalda
            //Si hace poco que la hemos soltado, no nos permitirá cogerla
            if(_shell.PickUp(transform, shellPoint))
            {
                //Pasamos sus stats al cangrejo
                shell = _shell;
                health = shell.health;
                shellWeight = shell.weight;
                UpdateSpeed();
            }
        }
    }
    
    //Dejar la concha equipada en el sitio
    public void DropShell()
    {
        if(shell!=null)
        {
            //Quitamos las stats que ofrecia la concha al cangrejo
            shellWeight = 0;
            health = 0;
            UpdateSpeed();

            //Dejar caer objeto concha
            shell.Drop();
            shell = null;
        }
    }

    //Actualizar velocidad en función del peso de la concha
    public void UpdateSpeed()
    {
        speedWeightFactor = 1 - (shellWeight / maxWeight) * (maxSpeedMult - minSpeedMult) + minSpeedMult;
    }
}