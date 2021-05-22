using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    //Cambiar de escena con los botones
    public void SwitchScene(string scene)
    {
        if (scene == "Credits")
            Debug.Log("Mostrar creditos (TODO)");
        else if(SceneManager.GetActiveScene().name == "Main")
        {
            GameManager.isQuitting = true;
            SceneManager.LoadScene(scene);
        }
        else
            SceneManager.LoadScene(scene);
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
