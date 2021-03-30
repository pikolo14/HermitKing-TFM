                        using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    private Collider coll;
    private CrabController controller;
    private bool enemy = false;

    public int attackDamage = 1;
    public float attackDuration = 0.5f;


    void Start()
    {
        coll = GetComponent<Collider>();
        coll.enabled = false;

        //Asignamos el componente controlador del cangrejo en funcion de si está asignado al cangrejo principal o a enemigos
        controller = GetComponentInParent<PlayerCrabController>();
        if(controller != null)
        {
            enemy = false;
        }
        else
        {
            controller = GetComponentInParent<CrabController>();
            enemy = true;
        }

        //if (transform.parent.gameObject == PlayerCrabController.player.gameObject)
        //    enemy = false;
        //else
        //    enemy = true;
    }

    //Atacamos si no se está atacando
    public void Attack()
    {
        if(!IsAttacking())
            StartCoroutine(AttackDuration(attackDuration));
    }

    //Animación de ataque y activacion de collider de ataque
    public IEnumerator AttackDuration(float duration)
    {
        //TODO: Animacion de ataque

        //Activamos el collider de ataque
        coll.enabled = true;
        Material mat = transform.parent.GetComponentInChildren<Renderer>().material;
        Color prev = mat.color;
        mat.color = Color.red;

        yield return new WaitForSeconds(duration);
        mat.color = prev;
        coll.enabled = false;
    }

    //Devuelve si se está atacando
    public bool IsAttacking()
    {
        return coll.enabled;
    }

    //Detectamos si golpeamos a alguien
    private void OnTriggerEnter(Collider other)
    {
        //Si estamos en un enemigo y entramos en el cuerpo del jugador, dañamos al jugador
        if (enemy && other.CompareTag(Globals.tagPlayer))
        {
            PlayerCrabController player = other.GetComponentInParent<PlayerCrabController>();
            player.GetHit(attackDamage);
        }
        //Si estamos en el cangrejo del jugador y entramos en el cuerpo de un enemigo, lo dañamos
        else if (!enemy && other.CompareTag(Globals.tagEnemy))
        {
            CrabController enemy = other.GetComponentInParent<CrabController>();
            enemy.GetHit(attackDamage);
        }
    }
}
