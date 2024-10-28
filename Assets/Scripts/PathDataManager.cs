using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LitJson;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace DefaultNamespace
{
    public class PathData
    {
        public int InletId;
        public double InitXOffset;
        public double InitYOffset;
        public double RealRunDuration;
        public double Bumper1HitCount;
        public double Bumper2HitCount;
        public double Bumper3HitCount;
        public int OutletId;

        public double InitXSpeed;
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

    public class HitPointData
    {
        //碰撞到的物体的ID
        public string ID;
        //碰撞类型
        public EHitType Type;
        //碰撞发生的位置
        // public Vector2 Pos
        // {
        //     get
        //     {
        //         return new Vector2((float)_posX, (float)_posY);
        //     }
        //     set
        //     {
        //         _posX = value.x;
        //         _posY = value.y;
        //     }
        // }
        public double PosX;
        public double PosY;
        // public float Time
        // {
        //     get => (float)_time;
        //     set => _time = value;
        // }
        public double Time;

        public Vector2 GetPos()
        {
            return new Vector2((float)PosX, (float)PosY);
        }

        public float GetTime()
        {
            return (float) Time;
        }

        public override string ToString()
        {
            return $"HitWith@{ID}_{Type}";
        }
    }
    
    public class PathDataManager
    {
        public static string GetStoragePath(string fileName)
        {
            return Path.Combine(Application.dataPath, $"ResourcesAB/BounceBall/Data/{fileName}.json");
        }
        
        public static string GetIdentifier(GameObject go)
        {
            if (go.GetComponent<IdentifierComp>() != null)
            {
                return go.GetComponent<IdentifierComp>().ToString();
            }
            
            return go.name;
        }

        public static void FromIdentifier(string identifier, out EFrameType type, out int id)
        {
            var parts = identifier.Split('#');
            try
            {
                type = (EFrameType) Enum.Parse(typeof(EFrameType), parts[0]);
                id = int.Parse(parts[1]);
            }
            catch (Exception e)
            {
                Debug.Log($"Parse Identifier {identifier} fail");
                type = EFrameType.None;
                id = -1;
            }
        }
        
        public static string GetIdentifier(EFrameType type, int id)
        {
            return $"{type}#{id}";
        }

        public static void AddData(PathData data)
        {
            if (inBatchMode)
            {
                AddDataBatchMode(data);
            }
            else
            {
                AddDataSimpleMode(data);
            }
        }

        private static void AddDataBatchMode(PathData data)
        {
            // var id = GetNextRecordId();
            remainCount--;
            Debug.Log($"AddData Remain {remainCount}");
            if (remainCount == 0)
            {
                StopRecord();
            }
            dataQueue.Enqueue(JsonMapper.ToJson(data));
        }

        private static void AddDataSimpleMode(PathData data)
        {
            var file = GetStoragePath("Replay");
            File.WriteAllText(file, JsonMapper.ToJson(data));
        }

        static ConcurrentQueue<string> dataQueue = new ConcurrentQueue<string>();
        static string filePath;
        static TaskCompletionSource<bool> tcs;
        static int remainCount = 0;
        static bool inBatchMode = false;

        public static async void StartRecord(int inletId, Vector2 xRange, float initXVelocity, int count)
        {
            var fileName = $"@{inletId}_Range_{xRange.x}-{xRange.y}_Vx{initXVelocity}_Num{count}";
            filePath = GetStoragePath(fileName);
            inBatchMode = true;
            remainCount = count;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            if (tcs != null)
            {
                tcs.SetCanceled();
            }
            tcs = new TaskCompletionSource<bool>();
            await ProcessDataAsync();
            // Task fileWriteTask = Task.Run(ProcessDataAsync);
            // await fileWriteTask;
        }
        
        static async Task ProcessDataAsync()
        {
            using (var writer = new StreamWriter(filePath, true))
            {
                while (!dataQueue.IsEmpty || !tcs.Task.IsCompleted)
                {
                    if (dataQueue.TryDequeue(out string data))
                    {
                        Debug.Log("Write Data: " + data);
                        await writer.WriteLineAsync(data);
                    }
                    else
                    {
                        await Task.Delay(10); // 短暂等待，避免CPU高速运行
                    }
                }
            }
        }
        
        public static void StopRecord()
        {
            Debug.Log("StopRecord");
            tcs.SetResult(true);
            inBatchMode = false;
        }
        
        public static PathData GetData(string fileName, int line)
        {
            var path = GetStoragePath(fileName);
            if (!File.Exists(path))
            {
                return null;
            }
            
            var json = File.ReadAllLines(path)[line - 1];
            return JsonMapper.ToObject<PathData>(json);
        }
    }
}