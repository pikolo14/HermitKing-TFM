using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrabController : CrabController
{
    public CharacterController charContr;
    public Camera cam;


    protected override void Update()
    {
        base.Update();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Food")
        {
            Eat(0.5f);
            Destroy(collision.collider.gameObject);
        }
    }

    //Mover el cangrejo mediante inputs
    protected override void Move()
    {
        //Rotar cangrejo para encajar en la dirección de la cámara en cierto tiempo
        float targetAngle = cam.transform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVel, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        //Desplazar cangrejo en función de su rotación actual y del joystick
        float xMove = Input.GetAxisRaw("Horizontal");
        float zMove = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(xMove, 0, zMove).normalized;
        if(dir.magnitude >= 0.1f)
        {
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * dir;
            charContr.Move(moveDir.normalized *speed *Time.deltaTime); 
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
}
