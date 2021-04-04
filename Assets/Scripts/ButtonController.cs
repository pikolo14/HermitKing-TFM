using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    //Cambiar de escena con los botones
    public void SwitchScene(string scene)
    {
        //TODO:
        if (scene == "Credits")
            Debug.Log("Mostrar creditos (TODO)");
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
