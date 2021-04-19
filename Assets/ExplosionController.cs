using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    float damage;
    
    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag(Globals.tagEnemy))
        {
            CrabController enemy = other.GetComponentInParent<CrabController>();
            enemy.GetHit((int)damage);
        }
    }

    public void Explode(float _damage, float _radius)
    {
        damage = _damage;
        SphereCollider coll = GetComponent<SphereCollider>();
        coll.enabled = true;
        coll.radius = _radius;
        //TODO: Instanciar explosion
        Debug.Log("BUUUM YA ESTA AQUI LA GUERRA");
    }
}
