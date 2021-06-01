using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour
{
    private bool disabledButtons = true;
         
    private void Awake()
    {
        AudioListener.volume = 1.0f;
        StartCoroutine(InitCorr());
    }

    //FIX:evitar pulsar Jugar al volver de creditos
    IEnumerator InitCorr()
    {
        yield return new WaitForSeconds(0.1f);
        disabledButtons = false;
    }

    //Cambiar de escena con los botones
    public void SwitchScene(string scene)
    {
        if(!disabledButtons)
        {
            if(SceneManager.GetActiveScene().name == "Main")
            {
                GameManager.isQuitting = true;
                SceneManager.LoadScene(scene);
            }
            else
                SceneManager.LoadScene(scene);
        }
    }

    //Cerrar aplicacion
    public void ExitGame()
    {
        Debug.Log("Saliendo...");
        Application.Quit();
    }

    public void Unpause()
    {
        GameManager.gameManager.SwitchPause();
    }
}
