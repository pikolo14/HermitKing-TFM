using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabController : MonoBehaviour
{
    //MOVIMIENTO
    public float speed = 5f;
    public float turnSmoothTime = 0.2f;
    protected float turnSmoothVel;


    // Update is called once per frame
    protected virtual void Update()
    {
        Move();
    }

    protected virtual void Move()
    {
        //TODO: Comportamiento movimiento cangrejos IA
    }
}