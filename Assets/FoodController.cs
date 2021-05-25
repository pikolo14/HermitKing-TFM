using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodController : MonoBehaviour
{
    public float noDetectionTime = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(NoDetectionCoroutine(noDetectionTime));
    }

    IEnumerator NoDetectionCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.tag = Globals.tagFood;
    }
}
