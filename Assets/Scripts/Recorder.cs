using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Sirenix.OdinInspector;
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
        Log($"OnCollisionEnter2D with {PathDataManager.GetIdentifier(other.gameObject)}, HitPos {gameObject.transform.position}");
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
        Log($"OnCollisionStay2D with {PathDataManager.GetIdentifier(other.gameObject)}, HitPos {gameObject.transform.position}");
        // _data.PathPoints.Add(new HitPointData()
        // {
        //     ID = PathDataManager.GetIdentifier(other.gameObject),
        //     Type = EHitType.CollisionStay,
        //     Pos = gameObject.transform.position,
        //     Time = _timeElapsed
        // });
        // 与曲面碰撞时，stay点之间的连线并非直线，不能把直接删掉多个连续的stay点，除非做一个斜率的判断
        // 与平面碰撞时，需要考虑摩擦力导致的加速度小于重力分速度，故而不能直接删掉连续的stay点
        if (_data.PathPoints.Count <= 0)
        {
            Debug.LogError($"OnCollisionStay {PathDataManager.GetIdentifier(other.gameObject)}, HitPos {gameObject.transform.position} PathPoints is empty");
        }
        else
        {
            var nowData = new HitPointData()
            {
                ID = PathDataManager.GetIdentifier(other.gameObject),
                Type = EHitType.CollisionStay,
                Pos = gameObject.transform.position,
                Time = _timeElapsed
            };
        
            bool shouldAddThisData = true;
            if (_data.PathPoints.Count >= 2)
            {
                var secondLastData = _data.PathPoints[_data.PathPoints.Count - 2];
                var lastData = _data.PathPoints.Last();
        
                if (secondLastData.ID == lastData.ID && lastData.ID == nowData.ID)
                {
                    if(AreNearlyCollinear(secondLastData.Pos, lastData.Pos, nowData.Pos))
                    {
                        // _data.PathPoints.RemoveAt(_data.PathPoints.Count - 1);
                        Debug.Log("this collision is collinear, ignore");
                        shouldAddThisData = false;
                    }
                }
            }
            
            if(shouldAddThisData) _data.PathPoints.Add(nowData);
        }
    }
    
    public static bool AreNearlyCollinear(Vector2 A, Vector2 B, Vector2 C, float toleranceDegrees = 1f)
    {
        // 计算向量 AB 和 AC
        Vector2 AB = B - A;
        Vector2 AC = C - A;

        // 计算点积
        float dotProduct = Vector2.Dot(AB, AC);

        // 计算两向量的模长
        float magnitudeAB = AB.magnitude;
        float magnitudeAC = AC.magnitude;

        // 计算夹角余弦
        float cosTheta = dotProduct / (magnitudeAB * magnitudeAC);

        // 计算夹角（弧度）
        float angleRadians = Mathf.Acos(cosTheta);

        // 转换为角度
        float angleDegrees = angleRadians * Mathf.Rad2Deg;

        Debug.Log($"AngleBetween {angleDegrees}");
        // 判断夹角是否小于容忍角度
        return angleDegrees <= toleranceDegrees;
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        Log($"OnCollisionExit2D with {PathDataManager.GetIdentifier(other.gameObject)}, HitPos {gameObject.transform.position}");
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
        // Debug.Log("TriggerEnter " + PathDataManager.GetIdentifier(other.gameObject));
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


