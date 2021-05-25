using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    //Spawn de comida, conchas y enemigos
    [Header("SCENE PREPARATION")]
    public Collider spawnZoneColl;
    public Collider navZoneColl;
    public GameObject foodPrefab, enemyPrefab;
    public GameObject[] shellPrefabs;
    public GameObject finalShell;
    public float distBetweenSpawns = 2f;
    //public float enemyProportion = 0.06f;
    //public float shellProportion = 0.04f;
    public int nEnemies, nShells, nFood;
    public Queue <GameObject> enemies;

    [Header("SIZE")]
    public float foodSizeIncr = 0.1f;
    public float scaleFactor = 0.5f; //Escala incrementada por unidad de size (cangrejos y conchas)
    public float sizeDiffAttackStep = 1; //Diferencia de tamaño que implica una una unidad mas de daño

    [Header("ENEMY PROGRESSION")]
    public float maxSizeDiff = 1; //Tamaño superior al jugador máximo de generación de un nuevo enemigo
    public float enemySpawnStepSize = 0.3f;//Se genera un enemigo cada este incremento de tamaño del cangrejo principal
    private CrabController enemyControllerPrefab;
    private float nextGenerationSize;
    public int maxEnemies = 30;

    [Header("SHELLS")]
    public float shellSizeTolerance = 0.5f;
    public float minShellSize = 0, maxShellSize = 10;
    public float crabShellProp = 0.5f;

    [Header("IU")]
    public GameObject victoryScreen;
    public GameObject loseScreen;
    public Button selectButtonWin, selectButtonLose, selectButtonPause;
    public GameObject pauseMenu;
    public bool paused = false;
    public bool finished = false;

    [Header("DEBUG")]
    public bool enabledGeneration = true;
    public static bool isQuitting = false;

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
        if(enabledGeneration)
            PrepareLevel();
        Time.timeScale = 1;
        finished = false;

        //Suscribirse al evento de cambio de tamaño del jugador para generar enemigos en funcion del tamaño del jugador
        PlayerCrabController.SizeCallback += ContinuousGeneration;
        nextGenerationSize = 1 / scaleFactor;
        enemyControllerPrefab = enemyPrefab.GetComponent<CrabController>();

        isQuitting = false;
        AudioListener.volume = 1f;
    }

    //Pausar/despausar
    public void SwitchPause(bool showMenu = true)
    {
        if(!finished)
        {
            paused = !paused;

            //Quitar pausa
            if (!paused)
            {
                Time.timeScale = 1;
                pauseMenu.SetActive(false);
                AudioListener.volume = 1f;
            }
            //Poner pausa
            else
            {
                Time.timeScale = 0;
                AudioListener.volume = 0f;

                if (showMenu)
                {
                    selectButtonPause.Select();
                    pauseMenu.SetActive(true);
                }
            }
        }
    }

    //Hacer spawn de comida y enemigos dentro de la zona indicada y con cierto espacio entre elementos
    void PrepareLevel()
    {
        Bounds bounds = spawnZoneColl.bounds;
        //Vector2 zoneSize = new Vector2(bounds.size.x, bounds.size.z)*2;

        List<Vector2> points2D = Globals.GeneratePoissonDiscPoints(distBetweenSpawns, bounds);
        List<int> enemiesIndexes = new List<int>();

        //Preparar caracola final
        ShellController fsController = finalShell.GetComponent<ShellController>();
        fsController.SetSize(maxShellSize + shellSizeTolerance);

        //Generar enemigos
        //int nEnemies = Mathf.FloorToInt(points2D.Count * enemyProportion);
        enemyPrefab.GetComponent<CrabController>().size = 0;
        enemies = GroupSpawn(nEnemies, enemyPrefab, ref points2D, bounds);

        //Generar conchas de distintos tipos a partes iguales
        //int nShells = Mathf.FloorToInt(points2D.Count * shellProportion /shellPrefabs.Length);
        foreach(GameObject shell in shellPrefabs)
        {
            GroupSpawn(nShells/shellPrefabs.Length, shell, ref points2D, bounds);
        }

        //Generar comida (dejando huecos vacíos)
        GroupSpawn(nFood, foodPrefab, ref points2D, bounds);
        //foreach (Vector2 point in points2D)
        //{
        //    Vector3 pos;
        //    if(Globals.GetGroundPoint(point, bounds, out pos))
        //    {
        //        Quaternion rot = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
        //        Instantiate(foodPrefab, pos, rot);
        //    }
        //}
    }

    public Queue<GameObject> GroupSpawn(int nObjects, GameObject prefab, ref List<Vector2> points2D, Bounds bounds)
    {
        List<int> objIndexes = new List<int>();
        Queue<GameObject> objs = new Queue<GameObject>();

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

            //Añadimos el punto a la lista de objetos e instanciamos uno
            objIndexes.Add(index);
            Vector3 pos;
            if (Globals.GetGroundPoint(points2D[index], bounds, out pos))
            {
                pos += Vector3.up * 0.15f;
                Quaternion rot = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
                objs.Enqueue(Instantiate(prefab, pos, rot));
            }
        }

        //Retiramos los puntos utilizados por estos objetos
        objIndexes.Sort();
        objIndexes.Reverse();
        foreach (int index in objIndexes)
        {
            points2D.RemoveAt(index);
        }

        return objs;
    }

    //Generamos un objeto en un punto aleatorio del terreno que no esté ocupado
    public GameObject SimpleSpawn(GameObject prefab, Bounds bounds)
    {
        bool valid = false;
        Vector3 point = Vector3.zero;
        int tries = 0;

        while(!valid)
        {
            Vector2 point2D = new Vector2(Random.Range(-bounds.extents.x, bounds.extents.x), Random.Range(-bounds.extents.z, bounds.extents.z)) + new Vector2(bounds.center.x, bounds.center.z);

            //Si el punto generado es válido y no hay nada, se genera el objeto
            if(Globals.GetGroundPoint(point2D, bounds, out point))
            {
                valid = true;
            }
            
            //Evitamos bucles infinitos si no hay huecos
            if(tries++ > 40)
            {
                return null;
            }
        }

        Quaternion rot = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
        return Instantiate(prefab, point, rot);
    }


    //Generación contínua de enemigos hasta alcanzar el tamaño máximo del cangrejo
    private void ContinuousGeneration (float size)
    {
        if (nextGenerationSize <= size && size < maxShellSize)
        {
            nextGenerationSize += enemySpawnStepSize;
            //Eliminamos el enemigo más antiguo (más pequeño) si sobrempasamos el límite de enemigos para evitar problemas de rendimiento
            if (enemies.Count >= maxEnemies)
            {
                GameObject e = enemies.Dequeue();
                Destroy(e);
            }

            enemyPrefab.GetComponent<CrabController>().size = Random.Range(size, size + maxSizeDiff);
            GameObject spawned = SimpleSpawn(enemyPrefab, spawnZoneColl.bounds);
            if(spawned != null)
                enemies.Enqueue(spawned);
        }
    }


    //Mostrar pantalla de derrota pausando el tiempo
    public void Lose()
    {
        SwitchPause(false);
        finished = true;
        loseScreen.SetActive(true);
        selectButtonLose.Select();
    }

    //Mostrar pantalla de victoria pausando el tiempo
    public void Win()
    {
        SwitchPause(false);
        finished = true;
        victoryScreen.SetActive(true);
        selectButtonWin.Select();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1;
        ParticleSystem[] parts = FindObjectsOfType<ParticleSystem>();
        for(int i=0; i<parts.Length; i++)
        {
            Destroy(parts[i].gameObject);
        }
    }

    //Evitamos errores al cerrar la aplicación
    protected void OnApplicationQuit()
    {
        isQuitting = true;
    }
}
