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

    //Controles de la camara
    private Vector2 inputMove = new Vector2(), lookCamera = new Vector2();
    public float deadZoneX = 0.2f;
    private float[] orbitsRads, orbitsHeights;


    protected override void Awake()
    {
        base.Awake();

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
        inputActions.Game.DropShell.started += ctx => DropShell();

        //Almacenamos los datos iniciales de las orbitas de la camara
        orbitsHeights = new float[cmCamera.m_Orbits.Length];
        orbitsRads = new float[cmCamera.m_Orbits.Length];

        for (int i = 0; i < cmCamera.m_Orbits.Length; i++)
        {
            orbitsHeights[i] = cmCamera.m_Orbits[i].m_Height;
            orbitsRads[i] = cmCamera.m_Orbits[i].m_Radius;
        }
    }

    private void Update()
    {
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public void OnCollisionEnter(Collision collision)
    {
        //Comer
        if(collision.collider.CompareTag(Globals.tagFood))
        {
            Eat(GameManager.gameManager.foodSizeIncr);
            Destroy(collision.collider.gameObject);
        }
        //Coger concha si no tenemos una ya equipada
        else if(collision.collider.CompareTag(Globals.tagShell) && shell==null)
        {
            GetShell(collision.collider.GetComponent<ShellController>());
        }
    }

    protected override void Move()
    {
        //Rotar cangrejo para encajar en la dirección de la cámara en cierto tiempo
        float targetAngle = cam.transform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVel, turnSmoothTime);
        rb.MoveRotation(Quaternion.Euler(0f, angle, 0f));

        //Desplazar cangrejo en función de su rotación actual y del joystick
        float xMove = inputMove.x;//Input.GetAxisRaw("Horizontal");
        float zMove = inputMove.y; //Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(xMove, 0, zMove).normalized;
        if (dir.magnitude >= 0.1f)
        {
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * dir;
            rb.MovePosition(rb.position + moveDir * speed * speedWeightFactor * Time.fixedDeltaTime);
        }
    }


    protected override void Defeat()
    {
        Debug.Log("Ermitaño principal muerto");
        //TODO
    }

    //Aumentamos el tamaño al comer
    public void Eat(float sizeIncrease)
    {
        size += sizeIncrease;
        UpdateCrabSize();

        //Si lo incomoda que es la concha supera a la vida que le queda, se suelta la concha
        if(shell != null)
        {
            discomfort = shell.GetDisconfort(size);
            if(discomfort >= health)
                DropShell();
        }
    }

    //Actualizar tamaño de escala del modelo del cangrejo
    public void UpdateCrabSize()
    {
        body.localScale = initBodyScale * size * GameManager.gameManager.scaleFactor;
        //TODO: ¿Actualizar posición del caparazón teniendo en cuenta tamaño de cangrejo y de caparazon?

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
