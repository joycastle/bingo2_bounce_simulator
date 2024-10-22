using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataTracker : MonoBehaviour
{
    public Vector3 Position;
    public Vector2 Velocity;
    // Start is called before the first frame update
    void Awake()
    {
        Position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var newPos = transform.position;
        Velocity.x = (newPos.x - Position.x) / Time.deltaTime;
        Velocity.y = (newPos.y - Position.y) / Time.deltaTime;
        Position = newPos;
    }
}
