using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;


public class PlayerCrabController : CrabController
{
    [SerializeField]
    public CrabActions inputActions;

    public static PlayerCrabController player;
    public Camera cam;

    private Vector2 inputMove = new Vector2(), lookCamera = new Vector2();
    public float deadZoneX = 0.2f;


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
        inputActions.Game.Attack.performed += ctx => attackContr.Attack();
        inputActions.Game.Defence.performed += ctx => Defence();
        inputActions.Game.DropShell.performed += ctx => DropShell();
    }

    private void Update()
    {
        //if(!attackContr.IsAttacking() && !defending)
        //{
        //    if(Input.GetButtonDown(Globals.inputAttack))
        //        attackContr.Attack();
        //    else if (Input.GetButtonDown(Globals.inputDefence))
        //        Defence();
        //}
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Food")
        {
            Eat(0.5f);
            Destroy(collision.collider.gameObject);
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
            rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);
        }
    }


    protected override void Defeat()
    {
        Debug.Log("Ermitaño principal muerto");
        //TODO
    }

    //Aumentamos el tamaño al comer X cantidad
    public void Eat(float sizeIncrease)
    {
        size += sizeIncrease;
        Debug.Log("Size player: " + size);

        //Si lo incomoda que es la concha supera a la vida que le queda, se suelta la concha
        discomfort = shell.GetDisconfort(size);
        if(discomfort >= health)
        {
            DropShell();
        }

        //TODO: Aumentar escala del cuerpo del cangrejo
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
