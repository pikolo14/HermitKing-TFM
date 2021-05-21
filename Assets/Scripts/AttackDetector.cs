using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDetector : MonoBehaviour
{
    //Evento para comunicar al attackController del atacante que se le ha dado a alguien
    public delegate void HitEvent(GameObject crab, Vector3 pos);
    public event HitEvent HitCallback;
    public SphereCollider coll;

    public void Awake()
    {
        coll = GetComponent<SphereCollider>();
    }

    //Detectamos si golpeamos a alguien
    private void OnTriggerEnter(Collider other)
    {
        //Si estamos en un enemigo y entramos en el cuerpo del jugador, dañamos al jugador
        if (gameObject.CompareTag(Globals.tagEnemy) && other.CompareTag(Globals.tagPlayer))
        {
            try
            {
                //Obtenemos el punto cercano a la superficie del collider de la pinza en la dirección en la que está el collider enemigo
                Vector3 dir = other.transform.position - transform.position;
                dir = dir.normalized * coll.radius * 0.9f;

                HitCallback(other.gameObject, dir+transform.position);
            }
            catch { }
            //PlayerCrabController player = other.GetComponentInParent<PlayerCrabController>();
            //player.GetHit(attackDamage);
        }
        //Si estamos en el cangrejo del jugador y entramos en el cuerpo de un enemigo, lo dañamos
        else if (gameObject.CompareTag(Globals.tagPlayer) && other.CompareTag(Globals.tagEnemy))
        {        
            try
            {
                //Obtenemos el punto cercano a la superficie del collider de la pinza en la dirección en la que está el collider enemigo
                Vector3 dir = other.transform.position - transform.position;
                dir = dir.normalized * coll.radius * 0.9f;

                HitCallback(other.gameObject, dir + transform.position);
            }
            catch { }
            //CrabController enemy = other.GetComponentInParent<CrabController>();
            //enemy.GetHit(attackDamage);
        }
    }

    private void OnDestroy()
    {
        //Desubscribir todos los eventos del callback para evitar errores
        if (HitCallback != null)
            foreach (var d in HitCallback.GetInvocationList())
                HitCallback -= (d as HitEvent);
    }
}
