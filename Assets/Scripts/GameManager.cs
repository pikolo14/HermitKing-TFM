using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    //Menu pausa
    public GameObject pauseMenu;
    public bool paused = false;


    //Spawn de comida, conchas y enemigos
    [Header("SPAWNS")]
    public Collider spawnZoneColl;
    public GameObject foodPrefab, enemyPrefab;
    public GameObject[] shellPrefabs;
    public float distBetweenSpawns = 2f;
    public float enemyProportion = 0.06f;
    public float shellProportion = 0.04f;
    public float foodSizeIncr = 0.1f;

    public float scaleFactor = 0.5f; //Escala incrementada por unidad de size (cangrejos y conchas)

    [Header("SHELLS")]
    public float shellSizeTolerance = 0.5f;
    public float minShellSize = 0, maxShellSize = 10;


    private void Awake()
    {
        if(gameManager == null)
        {
            gameManager = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        PrepareLevel();
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //Des/Pausar juego con escape
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchPause();
        }
    }

    //Pausar/despausar
    public void SwitchPause()
    {
        paused = !paused;

        //Quitar pausa
        if (!paused)
        {
            Time.timeScale = 1;
            pauseMenu.SetActive(false);
            Debug.Log("pause");
        }
        //Poner pausa
        else
        {
            Time.timeScale = 0;
            pauseMenu.SetActive(true);
            Debug.Log("unpause");
        }

    }

    //Hacer spawn de comida y enemigos dentro de la zona indicada y con cierto espacio entre elementos
    void PrepareLevel()
    {
        Bounds bounds = spawnZoneColl.bounds;
        Vector2 zoneSize = new Vector2(bounds.size.x, bounds.size.z)*2;

        List<Vector2> points2D = Globals.GeneratePoissonDiscPoints(distBetweenSpawns, zoneSize);
        List<int> enemiesIndexes = new List<int>(); 

        //Generar enemigos
        int nEnemies = Mathf.FloorToInt(points2D.Count * enemyProportion);
        SpecialSpawn(nEnemies, enemyPrefab, ref points2D, bounds);

        //Generar conchas de distintos tipos a partes iguales
        int nShells = Mathf.FloorToInt(points2D.Count * shellProportion /shellPrefabs.Length);
        foreach(GameObject shell in shellPrefabs)
        {
            SpecialSpawn(nShells, shell, ref points2D, bounds);
        }

        //Generar comida con el resto de puntos
        foreach (Vector2 point in points2D)
        {
            Vector3 pos;
            if(Globals.GetGroundPoint(point, bounds, out pos))
            {
                Quaternion rot = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
                Instantiate(foodPrefab, pos, rot);
            }
        }
    }

    public void SpecialSpawn(int nObjects, GameObject prefab, ref List<Vector2> points2D, Bounds bounds)
    {
        List<int> objIndexes = new List<int>();

        for (int i = 0; i < nObjects; i++)
        {
            //Buscamos un punto no utilizado
            int index;
            int tries = 0;
            do
            {
                index = Random.Range(0, points2D.Count);
                tries++;
                if (tries > 30)
                    break;
            } while (objIndexes.Contains(index));

            if (tries > 30)
                break;

            //Añadimos el punto a la lista de enemigos e instanciamos uno
            objIndexes.Add(index);
            Vector3 pos;
            if (Globals.GetGroundPoint(points2D[index], bounds, out pos))
            {
                Quaternion rot = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
                Instantiate(prefab, pos, rot);
            }
        }

        //Retiramos los puntos utilizados por los enemigos
        objIndexes.Sort();
        objIndexes.Reverse();
        foreach (int index in objIndexes)
        {
            points2D.RemoveAt(index);
        }
    }
}
