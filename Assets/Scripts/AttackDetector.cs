using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDetector : MonoBehaviour
{
    //Evento para comunicar al attackController del atacante que se le ha dado a alguien
    public delegate void HitEvent(GameObject crab, Vector3 pos);
    public AttackController controller;
    public event HitEvent HitCallback;
    public SphereCollider coll;

    public void Awake()
    {
        coll = GetComponent<SphereCollider>();
    }

    //Detectamos si golpeamos a alguien
    private void OnTriggerStay(Collider other)
    {
        //Si daña el enemigo al jugador o viceversa
        if (controller.damage && (gameObject.CompareTag(Globals.tagEnemy) && other.CompareTag(Globals.tagPlayer) || (gameObject.CompareTag(Globals.tagPlayer) && other.CompareTag(Globals.tagEnemy))))
        {
            Debug.Log("STAY IN ATTACK");
            try
            {
                //Obtenemos el punto cercano a la superficie del collider de la pinza en la dirección en la que está el collider enemigo
                Vector3 dir = other.transform.position - transform.position;
                dir = dir.normalized * coll.radius * 0.9f;

                HitCallback(other.gameObject, dir+transform.position);
                controller.damage = false;
            }
            catch { }
        }
    }

    //Detectamos si golpeamos a alguien
    private void OnTriggerEnter(Collider other)
    {
        //Si daña el enemigo al jugador o viceversa
        if (controller.damage && (gameObject.CompareTag(Globals.tagEnemy) && other.CompareTag(Globals.tagPlayer) || (gameObject.CompareTag(Globals.tagPlayer) && other.CompareTag(Globals.tagEnemy))))
        {
            Debug.Log("ENTER IN ATTACK");
            try
            {
                //Obtenemos el punto cercano a la superficie del collider de la pinza en la dirección en la que está el collider enemigo
                Vector3 dir = other.transform.position - transform.position;
                dir = dir.normalized * coll.radius * 0.9f;

                HitCallback(other.gameObject, dir + transform.position);
                controller.damage = false;
            }
            catch { }
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
