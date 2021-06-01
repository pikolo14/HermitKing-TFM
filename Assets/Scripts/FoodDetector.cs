using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodDetector : MonoBehaviour
{
    private Collider coll;
    private PlayerCrabController player;

    void Start()
    {
        player = PlayerCrabController.player;
        coll = GetComponent<Collider>();
    }

    //Al entrar en el area de deteccion de la comida informa al jugador para que crezca y destruye la comida
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Globals.tagFood))
        {
            player.Eat();
            Destroy(other.gameObject);
        }
    }
}
