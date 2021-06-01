using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CreditsController : MonoBehaviour
{
    public float backTime;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TimeCoroutine(backTime));
    }

    //Saltar creditos
    private void Update()
    {
        if (Input.anyKey)
        {
            SceneManager.LoadScene("Menu");
        }
    }

    //Volver al terminar los créditos
    IEnumerator TimeCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene("Menu");
    }
}
