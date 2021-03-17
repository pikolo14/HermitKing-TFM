using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellController : MonoBehaviour
{
    //Stats
    public int health;
    public int weight;

    //Rango de tamaños que admite la concha
    public float minSize;
    public float maxSize;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
