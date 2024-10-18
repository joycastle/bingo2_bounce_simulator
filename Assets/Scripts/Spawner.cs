using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    // [SerializeField]
    public Button BtnSpawnBallAtPos;
    public Button BtnSpawnBallRandom;
    public GameObject BallInstance;
    public Vector2 XOffsetRange = new Vector2(-1, 1);
    
    public Vector2 YOffsetRange = new Vector2(-1, 1);

    public float XOffset = 0f;
    public float YOffset = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        BtnSpawnBallAtPos.onClick.AddListener(SpawnBallAtPos);
        BtnSpawnBallRandom.onClick.AddListener(SpawnRandomBall);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    [Button]
    public void SpawnBallAtPos()
    {
        var position = new Vector3(transform.position.x + XOffset, transform.position.y + YOffset,
            transform.position.z);
        Instantiate(BallInstance, position, Quaternion.identity);
    }
    
    [Button]
    public void SpawnRandomBall()
    {
        var xRandom = Random.Range(XOffsetRange.x, XOffsetRange.y);
        var yRandom = Random.Range(YOffsetRange.x, YOffsetRange.y);
        var position = new Vector3(transform.position.x + xRandom, transform.position.y + yRandom,
            transform.position.z);
        Instantiate(BallInstance, position, Quaternion.identity);
    }
}
