using System.Collections.Generic;
using System.Linq;
using GameLib.Main.Modules.Campaigns.BounceBall.View;
using Sirenix.OdinInspector;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    private PathData _data;
    private float _timeElapsed;
    private const float MaxWaitTime = 30f;

    public void RecordInParam(int inletId, float initXSpeed, float initXOffset, float initYOffset)
    {
        _data.InletId = inletId;
        _data.InitXSpeed = initXSpeed;
        _data.InitXOffset = initXOffset;
        _data.InitYOffset = initYOffset;
    }
    
    void Awake()
    {
        _data = new PathData();
        _data.PathPoints = new List<HitPointData>();
        _data.PathPoints.Add(new HitPointData()
        {
            ID = "",
            Type = EHitType.CollisionExit,
            PosX = gameObject.transform.position.x,
            PosY = gameObject.transform.position.y,
            Time = 0f
        });
        _timeElapsed = 0;
    }

    void Update()
    {
        _timeElapsed += Time.deltaTime;
        if(_timeElapsed > MaxWaitTime)
        {
            Debug.LogError("Recorder Time out, destroy");
            Destroy(gameObject);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        Log($"OnCollisionEnter2D with {PathDataManager.GetIdentifier(other.gameObject)}, HitPos {gameObject.transform.position}");
        if (!TryGetAccuratePosition(other, out var position))
        {
            position = gameObject.transform.position;
        }
        _data.PathPoints.Add(new HitPointData()
        {
            ID = PathDataManager.GetIdentifier(other.gameObject),
            Type = EHitType.CollisionEnter,
            PosX = position.x,
            PosY = position.y,
            Time = _timeElapsed
        });
    }

    bool TryGetAccuratePosition(Collision2D collision, out Vector2 ret)
    {
        // 假设球体直径已知
        float diameter = 0.65f;  // 例如1.0单位
        float radius = diameter / 2.0f;
        
        if (collision.contacts.Length > 0)
        {
            // 遍历所有接触点（一般一个球体只有一个接触点）
            ContactPoint2D contact = collision.contacts[0];
            // 碰撞点位置
            Vector2 collisionPoint = contact.point;
            // 碰撞法线（指向碰撞物体外部，需要反向来指向球体中心）
            Vector2 normal = contact.normal;

            // 计算球体中心位置
            ret = collisionPoint + normal * radius;  // 使用+因为法线指向外部
            return true;
        }

        ret = Vector2.negativeInfinity;
        return false;
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
            if (!TryGetAccuratePosition(other, out var position))
            {
                position = gameObject.transform.position;
            }
            var nowData = new HitPointData()
            {
                ID = PathDataManager.GetIdentifier(other.gameObject),
                Type = EHitType.CollisionStay,
                PosX = position.x,
                PosY = position.y,
                Time = _timeElapsed
            };
        
            bool shouldAddThisData = true;
            if (_data.PathPoints.Count >= 2)
            {
                var secondLastData = _data.PathPoints[_data.PathPoints.Count - 2];
                var lastData = _data.PathPoints.Last();
        
                if (secondLastData.ID == lastData.ID && lastData.ID == nowData.ID)
                {
                    if(AreNearlyCollinear(secondLastData.GetPos(), lastData.GetPos(), nowData.GetPos()))
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
        if (!TryGetAccuratePosition(other, out var position))
        {
            position = gameObject.transform.position;
        }
        _data.PathPoints.Add(new HitPointData()
        {
            ID = PathDataManager.GetIdentifier(other.gameObject),
            Type = EHitType.CollisionExit,
            PosX = position.x,
            PosY = position.y,
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
                PosX = gameObject.transform.position.x,
                PosY = gameObject.transform.position.y,
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
        Debug.Log(s);
    }
}


