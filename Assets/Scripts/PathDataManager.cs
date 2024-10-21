using System.Collections.Generic;
using System.IO;
using LitJson;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace DefaultNamespace
{
    public class PathData
    {
        public float RealRunDuration;
        public List<HitPointData> PathPoints = new List<HitPointData>();
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
        // public Vector3 InVelocity;
        // public Vector3 OutVelocity;
    }
    
    public class PathDataManager
    {
        public static string GetStoragePath(int id)
        {
            return Path.Combine(Application.dataPath, $"ResourcesAB/BounceBall/Data/{id}.json");
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