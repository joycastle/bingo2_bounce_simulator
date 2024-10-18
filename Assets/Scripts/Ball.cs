using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TriggerEnter " + other.gameObject.name);
        if (other.gameObject.name.Contains("CollectionZone"))
        {
            Debug.Log("EnterZone " + other.gameObject.name);
            Destroy(gameObject);
        }
    }
}
