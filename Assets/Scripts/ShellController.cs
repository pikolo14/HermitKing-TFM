using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellController : MonoBehaviour
{
    [HideInInspector]
    public Collider coll;
    [HideInInspector]
    public Rigidbody rb;
    private Renderer rend;

    //Stats
    public int health;
    public int weight;
    public float size;

    //Rango de tamaños que admite la concha
    public float minSize;
    public float maxSize;

    public Vector3 initScale;

    //Tiempo que se ignora al cangrejo despues de soltar la concha
    public float ignoreCollTime = 4;
    public float reoccupyDelay = 1;

    public GameObject ignoredCrab;

    //Punto del cangrejo en el que se debe colocar continuamente (evitamos escalar la concha con el cangrejo)
    private Transform anchorPoint;

    //Lanzamiento de la concha y daño en area
    public SphereCollider explosionColl;
    public float explosionDamage;
    public float explosionRadius;
    public bool habitable = true;
    bool explosionReady = false;


    private void Awake()
    {
        //Suscribirse al evento de cambio de tamaño del jugador para cambiar la apariencia de la concha
        PlayerCrabController.SizeCallback += ctx => CheckAvailability(ctx);
        initScale = transform.localScale;
    }

    // Start is called before the first frame update
    public void Start()
    {
        coll = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        explosionColl.radius = explosionRadius;

        //Si no esta sobre un cangrejo se inicializa su tamaño aleatoriamente (si no se asigna manualmente)
        if(anchorPoint == null && size == 0)
        {
            //Inicializar con un tamaño aleatorio
            float ranSize = Random.Range(GameManager.gameManager.minShellSize, GameManager.gameManager.maxShellSize);
            SetSize(ranSize);
        }

        //Preparamos material de la concha
        //CheckAvailability(PlayerCrabController.player.size);
        rend = GetComponentInChildren<Renderer>();
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
        rb.drag = 0;
        rb.AddForce(force, ForceMode.Impulse);
        explosionReady = true;
    }

    //Ignorar colisiones con su cangrejo temporalmente
    public IEnumerator IgnoreCrabTemporarily(GameObject ignored, float time)
    {
        ignoredCrab = ignored;
        habitable = false;
        yield return new WaitForSeconds(reoccupyDelay);
        habitable = true;
        yield return new WaitForSeconds(time);
        ignoredCrab = null;
    }

    public IEnumerator IgnoreColliderTemporarily(GameObject ignored, float time)
    {
        //Collider ignoredColl = ignored.GetComponent<Collider>();
        //Physics.IgnoreCollision(coll, ignoredColl, true);
        coll.enabled = false;
        yield return new WaitForSeconds(time);
        coll.enabled = true;
        //Physics.IgnoreCollision(coll, ignoredColl, false);
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
        Debug.Log(cracked);
        Material material = new Material(rend.material);
        material.SetFloat("CrackedQuantity", cracked);
        rend.material = material;
    }

    public void CheckAvailability(float size)
    {
        float disconfort = GetDisconfort(size);
        //Si el jugador todavia no tiene el tamaño para entrar
        if (disconfort < 0)
        {
            SetAvailability(0);
        }
        //Si el jugador puede ocupar ahora la concha la encendemos para que se vea
        else if(disconfort == 0)
        {
            SetAvailability(1);
        }
        //Si el jugador ya no podrá ocupar esta concha la desactivamos
        else
        {
            SetAvailability(2);
        }
    }

    public void SetAvailability (int availability)
    {
        if(rend!= null)
        {
            Material material = new Material(rend.material);
        
            switch (availability)
            {
                //Normal: Demasiado pequeño para ocupar la concha
                case 0:
                    material.SetInt("Available", 0);
                    material.SetInt("Disabled", 0);
                    break;
                //Disponible: Tamaño perfecto para ocuparla
                case 1:
                    material.SetInt("Available", 1);
                    material.SetInt("Disabled", 0);
                    break;
                //Bloqueada: Demasiado grande para ocuparla
                case 2:
                    material.SetInt("Available", 0);
                    material.SetInt("Disabled", 1);
                    //Ya no es necesario estar subscrito al evento, no va a volver a ser habitable nunca
                    PlayerCrabController.SizeCallback -= ctx => CheckAvailability(ctx);
                    break;
            }

            rend.material = material;
        }
    }

    public void OnDestroy()
    {
        PlayerCrabController.SizeCallback -= ctx => CheckAvailability(ctx);
    }
}
