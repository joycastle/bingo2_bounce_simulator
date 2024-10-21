using System.Collections.Generic;
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
            HitID = "",
            HitType = EHitType.CollisionExit,
            Position = gameObject.transform.position,
            TimeElapsed = 0f
        });
        _timeElapsed = 0;
    }

    void Update()
    {
        _timeElapsed += Time.deltaTime;
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log($"OnCollisionEnter2D with {other.gameObject.name}, HitPos {gameObject.transform.position}");
        _data.PathPoints.Add(new HitPointData()
        {
            HitID = other.gameObject.name,
            HitType = EHitType.CollisionEnter,
            Position = gameObject.transform.position,
            TimeElapsed = _timeElapsed
        });
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        Debug.Log($"OnCollisionStay2D with {other.gameObject.name}, HitPos {gameObject.transform.position}");
        _data.PathPoints.Add(new HitPointData()
        {
            HitID = other.gameObject.name,
            HitType = EHitType.CollisionStay,
            Position = gameObject.transform.position,
            TimeElapsed = _timeElapsed
        });
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        Debug.Log($"OnCollisionExit2D with {other.gameObject.name}, HitPos {gameObject.transform.position}");
        _data.PathPoints.Add(new HitPointData()
        {
            HitID = other.gameObject.name,
            HitType = EHitType.CollisionExit,
            Position = gameObject.transform.position,
            TimeElapsed = _timeElapsed
        });
    }

    private void OnDestroy()
    {
        _data.RealRunDuration = _timeElapsed;
        Debug.Log($"OnDestroy, RealRunDuration {_data.RealRunDuration}");
        PathDataManager.AddData(1, _data);
        // var json = JsonUtility.ToJson(_data);
        // Debug.Log($"{json}");
    }
}


