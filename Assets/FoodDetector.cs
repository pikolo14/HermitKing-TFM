using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodDetector : MonoBehaviour
{
    private Collider coll;
    private PlayerCrabController player;

    // Start is called before the first frame update
    void Start()
    {
        player = PlayerCrabController.player;
        coll = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Globals.tagFood))
        {
            player.Eat();
            Destroy(other.gameObject);
        }
    }
}
