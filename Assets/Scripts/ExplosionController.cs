using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    float damage;
    public GameObject prefabExplosion;
    private bool isQuitting = false;

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag(Globals.tagEnemy))
        {
            CrabController enemy = other.GetComponentInParent<CrabController>();
            enemy.GetHit((int)damage, Vector3.zero);
        }
    }

    public void Explode(float _damage, float _radius)
    {
        damage = _damage;
        SphereCollider coll = GetComponent<SphereCollider>();
        coll.enabled = true;
        coll.radius = _radius;

        //Generamos una nube de particulas si no se está destruyendo la instancia antes de cerrar el juego para evitar errores
        if (!isQuitting)
        {
            Instantiate(prefabExplosion, transform.position, new Quaternion()).gameObject.SetActive(true);
        }
    }

    //Evitamos errores al cerrar la aplicación
    protected void OnApplicationQuit()
    {
        isQuitting = true;
    }
}
