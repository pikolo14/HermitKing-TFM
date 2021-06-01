using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodController : MonoBehaviour
{
    public float noDetectionTime = 1.5f;

    void Start()
    {
        StartCoroutine(NoDetectionCoroutine(noDetectionTime));
    }

    //Corrutina para evitar que se coma la comida inmediatamente conforme se crea
    IEnumerator NoDetectionCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.tag = Globals.tagFood;
    }
}
