using System.Collections.Generic;
using System.IO;
using LitJson;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace DefaultNamespace
{
    public class PathData
    {
        public int InletId;
        public float RealRunDuration;
        public float Bumper1HitCount;
        public float Bumper2HitCount;
        public float Bumper3HitCount;
        public int OutletId;

        public float InitXSpeed;
        public List<HitPointData> PathPoints = new List<HitPointData>();

        public int GetCollisionCount(EFrameType frameType, int id)
        {
            var ret = 0;
            var identifier = PathDataManager.GetIdentifier(frameType, id);
            for (int i = 0; i < PathPoints.Count; i++)
            {
                var currHitPointData = PathPoints[i];
                if (currHitPointData.ID.Equals(identifier) && currHitPointData.Type == EHitType.CollisionEnter)
                {
                    ret++;
                }
            }

            return ret;
        }
    }

    public enum EHitType
    {
        CollisionEnter,
        CollisionStay,
        CollisionExit
    }

    public struct HitPointData
    {
        //碰撞到的物体的ID
        public string ID;
        //碰撞类型
        public EHitType Type;
        //碰撞发生的位置
        public Vector2 Pos;
        //碰撞发生的时间
        public float Time;
    }
    
    public class PathDataManager
    {
        public static string GetStoragePath(int id)
        {
            return Path.Combine(Application.dataPath, $"ResourcesAB/BounceBall/Data/{id}.json");
        }
        
        public static string GetIdentifier(GameObject go)
        {
            if (go.GetComponent<IdentifierComp>() != null)
            {
                return go.GetComponent<IdentifierComp>().ToString();
            }
            
            return go.name;
        }
        
        public static string GetIdentifier(EFrameType type, int id)
        {
            return $"{type}#{id}";
        }
        
        public static void AddData(int id, PathData data)
        {
            var path = GetStoragePath(id);
            var json = JsonMapper.ToJson(data);
            File.WriteAllText(path, json);
        }
        
        public static PathData GetData(int id)
        {
            var path = GetStoragePath(id);
            if (!File.Exists(path))
            {
                return null;
            }

            var json = File.ReadAllText(path);
            return JsonMapper.ToObject<PathData>(json);
        }
    }
}