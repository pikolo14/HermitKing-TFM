using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellController : MonoBehaviour
{
    [HideInInspector]
    public Collider coll;
    [HideInInspector]
    public Rigidbody rb;


    //Stats
    public int health;
    public int weight;
    public float size;

    //Rango de tamaños que admite la concha
    public float minSize;
    public float maxSize;

    public Vector3 initScale;

    //Tiempo que se ignora al cangrejo despues de soltar la concha
    public static float ignoreCollTime = 3;
    public GameObject ignoredCrab;

    //Punto del cangrejo en el que se debe colocar continuamente (evitamos escalar la concha con el cangrejo)
    private Transform anchorPoint;

    //Lanzamiento de la concha y daño en area
    public SphereCollider explosionColl;
    public float explosionDamage;
    public float explosionRadius;
    bool explosionReady = false;

    public Renderer renderer;


    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        initScale = transform.localScale;
        explosionColl.radius = explosionRadius;

        //Si no esta sobre un cangrejo se inicializa su tamaño aleatoriamente (si no se asigna manualmente)
        if(anchorPoint == null)
        {
            //Inicializar con un tamaño aleatorio
            float ranSize = Random.Range(GameManager.gameManager.minShellSize, GameManager.gameManager.maxShellSize);
            SetSize(ranSize);
        }

        //Preparamos material de la concha
        renderer = GetComponentInChildren<Renderer>();
        SetCrackedMaterial(0);
    }

    // Update is called once per frame
    void Update()
    {
        if(anchorPoint != null)
        {
            transform.position = anchorPoint.position;
        }
    }

    public void SetSize(float _size)
    {
        size = _size;
        minSize = Mathf.Max(size - GameManager.gameManager.shellSizeTolerance, 0);
        maxSize = size + GameManager.gameManager.shellSizeTolerance;

        //Actualizar escala con ese tamaño
        transform.localScale = initScale * size * GameManager.gameManager.scaleFactor;
    }


    public float GetDisconfort(float crabSize)
    {
        //Si el cangrejo es demasiado pequeño devolvemos un valor negativo con lo que le falta
        float underSize = (crabSize - minSize) * CrabController.disconfortFactor;
        if (underSize < 0)
            return underSize;

        //Si el cangrejo es demasiado grande devolvemos un valor positivo con lo que le sobra
        float overSize = (crabSize - maxSize) * CrabController.disconfortFactor;
        if(overSize > 0)
            return overSize;

        return 0;
    }

    //Vincular concha al cangrejo poniendola en su espalda
    public bool PickUp(Transform parent, Transform point)
    {
        if (!explosionReady && ignoredCrab != parent.gameObject)
        {
            coll.enabled = false;
            rb.isKinematic = true;
            transform.parent = parent;
            anchorPoint = point;
            //transform.position = point.transform.position;
            transform.localRotation = point.transform.localRotation;
            return true;
        }
        else
            return false;
    }

    //Desvinculamos la concha de su cangrejo e ignoramos sus colisiones temporalmente
    public void Drop()
    {
        StartCoroutine(IgnoreCrabTemporarily(transform.parent.gameObject, ignoreCollTime));

        coll.enabled = true;
        rb.isKinematic = false;
        transform.parent = null;
        anchorPoint = null;

        //Restablecemos la apariencia de la concha
        SetCrackedMaterial(0);
    }

    //Lanzamos concha hacia delante para que explote
    public void Launch(Vector3 force)
    {
        //Ignoramos durante el lanzamiento el suelo
        StartCoroutine(IgnoreColliderTemporarily(GameObject.FindGameObjectWithTag(Globals.tagGround), 0.2f));
        //Aplicamos la fuerza a la concha para lanzarla e indicamos que va a explotar al tocar algo
        rb.AddForce(force, ForceMode.Impulse);
        explosionReady = true;
    }

    //Ignorar colisiones con su cangrejo temporalmente
    public IEnumerator IgnoreCrabTemporarily(GameObject ignored, float time)
    {
        ignoredCrab = ignored;
        yield return new WaitForSeconds(time);
        ignoredCrab = null;
    }

    public IEnumerator IgnoreColliderTemporarily(GameObject ignored, float time)
    {
        Collider ignoredColl = ignored.GetComponent<Collider>();
        Physics.IgnoreCollision(coll, ignoredColl, true);
        yield return new WaitForSeconds(time);
        Physics.IgnoreCollision(coll, ignoredColl, false);
    }

    //Explotamos la concha al tocar cualquier superficie haciendo daño en area
    public void OnCollisionEnter(Collision collision)
    {
        if(explosionReady && !collision.collider.CompareTag(Globals.tagPlayer))
        {
            GetComponentInChildren<ExplosionController>().Explode(explosionDamage, explosionRadius);
            GetComponentInChildren<MeshRenderer>().enabled = false;
            Destroy(gameObject, 0.1f);
        }
    }

    //Modificar la apariencia de la concha en funcion de la vida que quede
    public void SetCrackedMaterial(float cracked)
    {
        Material material = new Material(renderer.material);
        material.SetFloat("CrackedQuantity", cracked);
        renderer.material = material;
    }
}
