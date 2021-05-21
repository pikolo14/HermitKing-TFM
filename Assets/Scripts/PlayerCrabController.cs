using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;


public class PlayerCrabController : CrabController
{
    [SerializeField]
    public CrabActions inputActions;
    public CinemachineFreeLook cmCamera;
    public static PlayerCrabController player;
    public Camera cam;

    //Salto desde concha
    [Header("SHELL JUMP")]
    public float maxJumpForce;
    public float minJumpForce;
    public float maxJumpTime = 3;
    public float jumpAngle = 40;
    private bool jumpPressed = false;
    private float jumpPressedTime = 0;

    //Lanzamiento concha
    [Header("SHELL LAUNCH")]
    public float launchForce;
    public float launchAngle;

    //Controles de la camara
    [Header("CAMERA")]
    public float deadZoneX = 0.2f;
    private Vector2 inputMove = new Vector2(), lookCamera = new Vector2();
    private float[] orbitsRads, orbitsHeights;

    //Evento para comunicar a las conchas que se ha aumentado de tamaño
    public delegate void SizeEvent(float size);
    public static event SizeEvent SizeCallback;

    public int foodCount = 0;

    public float jumpingTime = 0;


    protected override void Awake()
    {
        base.Awake();

        currState = null;

        //Referencia estática singleton de nuestro jugador
        if (player == null)
            player = this;
        else
            Destroy(player);

        //Asignamos las funciones a los eventos de nuestro new input system
        inputActions = new CrabActions();
        inputActions.Game.Move.performed += ctx => { inputMove = ctx.ReadValue<Vector2>(); };
        inputActions.Game.Look.performed += ctx => { lookCamera = ctx.ReadValue<Vector2>().normalized; };
        inputActions.Game.Look.performed += ctx => GetInput();
        if(attackContr!=null)
            inputActions.Game.Attack.started += ctx => attackContr.Attack();
        inputActions.Game.Defence.started += ctx => Defence();
        inputActions.Game.DropShell.started += ctx => { jumpPressed = true; };
        inputActions.Game.DropShell.canceled += ctx => { jumpPressed = false; };
        inputActions.Game.Launch.performed += ctx => LaunchShell();
        inputActions.Game.Pause.started += ctx => GameManager.gameManager.SwitchPause();

        //Almacenamos los datos iniciales de las orbitas de la camara
        orbitsHeights = new float[cmCamera.m_Orbits.Length];
        orbitsRads = new float[cmCamera.m_Orbits.Length];

        for (int i = 0; i < cmCamera.m_Orbits.Length; i++)
        {
            orbitsHeights[i] = cmCamera.m_Orbits[i].m_Height;
            orbitsRads[i] = cmCamera.m_Orbits[i].m_Radius;
        }
    }

    private void OnDestroy()
    {
        //Desactivamos las vinculaciones de eventos para evitar errores de llamadas a eventos que ya no existen
        inputActions.Game.Disable();

        //Desubscribir todos los eventos del callback de cambio de tamaño
        if (SizeCallback != null)
            foreach (var d in SizeCallback.GetInvocationList())
                SizeCallback  -= (d as SizeEvent);
    }

    private void Update()
    {
        if(shell != null)
        {
            if(jumpPressed)
            {
                jumpPressedTime += Time.deltaTime;
            }
            else if(jumpPressedTime > 0)
            {
                ShellJump(jumpPressedTime);
                jumpPressedTime = 0;
            }
        }
        else
        {
            jumpPressed = false;
            jumpPressedTime = 0;
        }

        MoveLegs();
    }

    //Salto cargado del jugador al dejar la concha
    private void ShellJump(float time)
    {
        //Obtenemos la cantidad de carga del salto en función de los que se haya mantenido el botón
        time = Mathf.Min(time, maxJumpTime) / maxJumpTime;

        //Obtenemos el vector de fuerza con el ángulo y rango de fuerzas indicados
        Vector3 force = transform.forward.normalized;
        force.y = 0;
        force = Quaternion.AngleAxis(jumpAngle, -transform.right) * force;
        force.Normalize();
        force *= Mathf.Lerp(minJumpForce*size, maxJumpForce*size, time);

        //Ignoramos temporalmente la concha para que no interfiera, la soltamos y aplicamos la fuerza al jugador
        StartCoroutine(TempIgnoreColl(shell.gameObject));
        DropShell();
        jumpingTime = 0.5f;
        rb.AddForce(force, ForceMode.Impulse);
    }

    //Lanzamiento hacia adelante de la concha
    private void LaunchShell()
    {
        if(shell != null)
        {
            //De la misma manera que en la funcion de salto obtenemos el vector de fuerza que impulsara la concha
            Vector3 force = transform.forward.normalized;
            force.y = 0;
            force = Quaternion.AngleAxis(launchAngle, -transform.right) * force;
            force = force.normalized * launchForce;

            ShellController auxShell = shell;
            StartCoroutine(TempIgnoreColl(shell.gameObject));
            DropShell();
            auxShell.Launch(force);
        }
    }


    protected override void FixedUpdate()
    {
        if(jumpingTime > 0)
        {
            jumpingTime -= Time.fixedDeltaTime;
        }
        Move();
    }

    public void OnCollisionEnter(Collision collision)
    {
        //Coger concha si no tenemos una ya equipada
        if(collision.collider.CompareTag(Globals.tagShell) && shell==null)
        {
            ShellController contr = collision.collider.GetComponent<ShellController>();
            if(contr == null)
            {
                contr = collision.collider.GetComponentInParent<ShellController>();
            }
            GetShell(contr);
        }
    }

    private void Move()
    {
        //Rotar cangrejo para encajar en la dirección de la cámara en cierto tiempo
        float targetAngle = cam.transform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVel, turnSmoothTime);
        rb.MoveRotation(Quaternion.Euler(0f, angle, 0f));

        //Desplazar cangrejo en función de su rotación actual y del joystick
        //Si se está preparando para saltar de la concha no se desplaza, solo se orienta
        if(!jumpPressed)
        {
            float xMove = inputMove.x;
            float zMove = inputMove.y;
            Vector3 dir = new Vector3(xMove, 0, zMove).normalized;
            if (dir.magnitude >= 0.1f)
            {
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * dir;
                rb.MovePosition(rb.position + moveDir * baseSpeed * size * speedWeightFactor * Time.fixedDeltaTime);
            }
        }

        //Si esta grounded y no se está en mitad de un salto
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);
        int layer = 1 << 9;

        if (jumpingTime <= 0 && Physics.Raycast(ray, out hit, bodyHeight * size * 2f, layer))
        {
            //Evitar que el cangrejo se mueva solo cuando no se tocan los controles, como en una pendiente
            rb.velocity = new Vector3(0,rb.velocity.y, 0);
            if (dustParticles.isPaused)
                dustParticles.Play();
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.red);
            if(!dustParticles.isPaused)
                dustParticles.Pause();
        }
    }

    protected override void Defeat()
    {
        GameManager.gameManager.Lose();
    }

    //Aumentamos el tamaño al comer
    public void Eat()
    {
        size += GameManager.gameManager.foodSizeIncr;
        Debug.Log(foodCount++ +", "+ size);
        //Evitamos que crezca demasiado y no quepa en la concha final
        size = Mathf.Clamp(size, 0, GameManager.gameManager.maxShellSize + GameManager.gameManager.shellSizeTolerance);
        UpdateCrabSize();

        //Si lo incomoda que es la concha supera a la vida que le queda, se suelta la concha
        if(shell != null)
        {
            discomfort = shell.GetDisconfort(size);

            //Si es demasiado grande agrietamos la concha
            if (discomfort > 0)
            {
                if(discomfort >= health)
                    DropShell();
                else
                    shell.SetCrackedMaterial(1 - ((health - Mathf.Max(0, discomfort)) / (float)shell.health));
            }
        }
    }

    //Actualizar tamaño de escala del modelo del cangrejo
    public new void UpdateCrabSize()
    {
        base.UpdateCrabSize();

        //Comunicar a todas las conchas subscritas que se ha aumentado el tamaño para cambiar su visualizacion
        SizeCallback(size);

        //Alejar camara conforme crezca con la propoción del tamaño actual respecto al inicial para aplicarlo a los orbits
        float prop = body.localScale.x / initBodyScale.x;
        UpdateCameraZoom(prop);
    }

    //Alejar camara conforme crezca agrandando los orbits de la camara de cinemachine
    public void UpdateCameraZoom(float prop)
    {
        for(int i = 0; i< cmCamera.m_Orbits.Length; i++)
        {
            cmCamera.m_Orbits[i].m_Height = orbitsHeights[i]*prop;
            cmCamera.m_Orbits[i].m_Radius = orbitsRads[i]*prop;
        }
    }

    //Ignorar temporalmente a un collider como la concha al lanzarla
    IEnumerator TempIgnoreColl(GameObject obj)
    {
        Collider objColl = obj.GetComponent<Collider>();
        Physics.IgnoreCollision(bodyColl, objColl, true);
        Physics.IgnoreCollision(groundColl, objColl, true);
        yield return new WaitForSeconds(0.2f);
        if(bodyColl !=null && objColl!=null)
            Physics.IgnoreCollision(bodyColl, objColl, false);
        if (groundColl != null && objColl != null)
            Physics.IgnoreCollision(groundColl, objColl, false);
    }


    //***** FIXES CINEMACHINE + NEW INPUT SYSTEM *****
    private void OnEnable()
    {
        inputActions.Game.Enable();
    }

    private void OnDisable()
    {
        inputActions.Game.Enable();
    }

    private void GetInput()
    {
        CinemachineCore.GetInputAxis = GetAxisCustom;
    }

    public float GetAxisCustom(string axisName)
    {
        // LookCamera.Normalize();

        if (axisName == "Camera X")
        {
            if (lookCamera.x > deadZoneX || lookCamera.x < -deadZoneX) // To stabilise Cam and prevent it from rotating when LookCamera.x value is between deadZoneX and - deadZoneX
            {
                return lookCamera.x;
            }
        }

        else if (axisName == "Camera Y")
        {
            return lookCamera.y;
        }

        return 0;
    }
}
