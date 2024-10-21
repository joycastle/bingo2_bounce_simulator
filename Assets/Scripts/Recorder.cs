using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    // Start is called before the first frame update
    private PathData _data;
    private float _timeElapsed;

    void Awake()
    {
        _data = new PathData();
        _data.PathPoints = new List<HitPointData>();
        _data.PathPoints.Add(new HitPointData()
        {
            ID = "",
            Type = EHitType.CollisionExit,
            Pos = gameObject.transform.position,
            Time = 0f
        });
        _timeElapsed = 0;
    }

    void Update()
    {
        _timeElapsed += Time.deltaTime;
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        Log($"OnCollisionEnter2D with {other.gameObject.name}, HitPos {gameObject.transform.position}");
        _data.PathPoints.Add(new HitPointData()
        {
            ID = other.gameObject.name,
            Type = EHitType.CollisionEnter,
            Pos = gameObject.transform.position,
            Time = _timeElapsed
        });
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        Log($"OnCollisionStay2D with {other.gameObject.name}, HitPos {gameObject.transform.position}");
        _data.PathPoints.Add(new HitPointData()
        {
            ID = other.gameObject.name,
            Type = EHitType.CollisionStay,
            Pos = gameObject.transform.position,
            Time = _timeElapsed
        });
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        Log($"OnCollisionExit2D with {other.gameObject.name}, HitPos {gameObject.transform.position}");
        _data.PathPoints.Add(new HitPointData()
        {
            ID = other.gameObject.name,
            Type = EHitType.CollisionExit,
            Pos = gameObject.transform.position,
            Time = _timeElapsed
        });
    }

    private void OnDestroy()
    {
        _data.RealRunDuration = _timeElapsed;
        Log($"OnDestroy, RealRunDuration {_data.RealRunDuration}");
        PathDataManager.AddData(1, _data);
        // var json = JsonUtility.ToJson(_data);
        // Debug.Log($"{json}");
    }

    private void Log(string s)
    {
        // Debug.Log(s);
    }
}


