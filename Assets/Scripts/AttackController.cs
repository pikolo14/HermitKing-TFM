                        using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    public Collider [] colls;
    private CrabController controller;
    private bool enemy = false;
    public float attackDuration = 0.5f;
    public GameObject hitParticles;
    public bool attacking = false;
    private IEnumerator attackCorr = null;
    public bool damage = false;


    void Start()
    {
        //Nos suscribimos al evento de golpear a un cangrejo contrincante en los dos detectores
        foreach(Collider c in colls)
        {
            c.gameObject.GetComponent<AttackDetector>().HitCallback += HitCrab;
        }

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
    }

    //Atacamos si no se está atacando
    public void Attack()
    {
        if(!IsAttacking() && !GameManager.gameManager.paused)
            StartCoroutine(AttackDuration(attackDuration));
    }

    //Animación de ataque y activacion de collider de ataque
    public IEnumerator AttackDuration(float duration)
    {
        attacking = true;
        controller.generalSounds.Play("Attack");

        //Animacion de ataque
        controller.animator.SetBool(Globals.inputAttack, true);
        yield return new WaitForSeconds(0.5f);

        //Activamos el collider de ataque
        damage = true;

        //Desactivamos el collider
        yield return new WaitForSeconds(attackDuration);
        damage = false;

        attacking = false;
    }

    //Devuelve si se está atacando
    public bool IsAttacking()
    {
        return attacking;
    }

    //Comunicar el golpe al cangrejo contrincante en función de la diferencia de tamaños
    public void HitCrab(GameObject crab, Vector3 hitPos)
    {
        CrabController advContr;

        if (enemy)
            advContr = crab.GetComponentInParent<PlayerCrabController>();
        else
            advContr = crab.GetComponentInParent<CrabController>();

        //El aumento de daño se incrementa en enteros en funcion del valor que hemos puesto de margen. El mínimo siempre será 1
        int damage = Mathf.Max((int)(Mathf.Floor((controller.size - advContr.size)/GameManager.gameManager.sizeDiffAttackStep) +1), 1);

        advContr.GetHit(damage, hitPos);
    }
}
