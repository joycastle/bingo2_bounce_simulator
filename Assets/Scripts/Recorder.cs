using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    private PathData _data;
    private float _timeElapsed;

    public void RecordInParam(int inletId, float initXSpeed)
    {
        _data.InletId = inletId;
        _data.InitXSpeed = initXSpeed;
    }
    
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
            ID = PathDataManager.GetIdentifier(other.gameObject),
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
            ID = PathDataManager.GetIdentifier(other.gameObject),
            Type = EHitType.CollisionStay,
            Pos = gameObject.transform.position,
            Time = _timeElapsed
        });
        // if (_data.PathPoints.Count <= 0)
        // {
        //     Debug.LogError($"OnCollisionStay {other.gameObject.name}, HitPos {gameObject.transform.position} PathPoints is empty");
        // }
        // else
        // {
        //     var lastData = _data.PathPoints.Last();
        //     //stay状态只记录一个，减少存档大小
        //     if (lastData.ID != other.gameObject.name)
        //     {
        //         _data.PathPoints.Add(new HitPointData()
        //         {
        //             ID = other.gameObject.name,
        //             Type = EHitType.CollisionStay,
        //             Pos = gameObject.transform.position,
        //             Time = _timeElapsed
        //         });
        //     }
        // }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        Log($"OnCollisionExit2D with {other.gameObject.name}, HitPos {gameObject.transform.position}");
        _data.PathPoints.Add(new HitPointData()
        {
            ID = PathDataManager.GetIdentifier(other.gameObject),
            Type = EHitType.CollisionExit,
            Pos = gameObject.transform.position,
            Time = _timeElapsed
        });
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log("TriggerEnter " + other.gameObject.name);
        var identifier = other.gameObject.GetComponent<IdentifierComp>();
        if(identifier != null && identifier.Type == EFrameType.Outlet)
        {
            _data.OutletId = identifier.ID;
            _data.PathPoints.Add(new HitPointData()
            {
                ID = PathDataManager.GetIdentifier(other.gameObject),
                Type = EHitType.CollisionEnter,
                Pos = gameObject.transform.position,
                Time = _timeElapsed
            });
        }
    }

    private void OnDestroy()
    {
        _data.RealRunDuration = _timeElapsed;
        _data.Bumper1HitCount = _data.GetCollisionCount(EFrameType.Bumper, 1);
        _data.Bumper2HitCount = _data.GetCollisionCount(EFrameType.Bumper, 2);
        _data.Bumper3HitCount = _data.GetCollisionCount(EFrameType.Bumper, 3);
        Log($"OnDestroy, RealRunDuration {_data.RealRunDuration}");
        PathDataManager.AddData(_data);
        Spawner.ConcurrentBall--;
        // var json = JsonUtility.ToJson(_data);
        // Debug.Log($"{json}");
    }

    private void Log(string s)
    {
        // Debug.Log(s);
    }
}


