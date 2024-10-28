using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using LitJson;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GameLib.Main.Modules.Campaigns.BounceBall.View
{
    public class Spawner : MonoBehaviour
    {
        public Button BtnSpawnBallAtPos;
        public Button BtnSpawnBallRandom;
        public List<GameObject> Inlets;
        public GameObject BallInstance;
        public GameObject RecordBallInstance;
        public GameObject ReplayBallInstance;
        public GameObject ReplayUIBallInstance;
        public LaunchConfig Config;
        public EMode Mode => Config.Mode;
        
        public float XOffset
        {
            get
            {
                if (Config.RecordFromFileName == null || Config.RecordFromFileName == "")
                {
                    return Config.XOffset;
                }
                else
                {
                    var data = PathDataManager.GetData(Config.RecordFromFileName, Config.RecordLineNum);
                    return (float) data.InitXOffset;
                }
            }
        }

        public float YOffset
        {
            get
            {
                if (Config.RecordFromFileName == null || Config.RecordFromFileName == "")
                {
                    return Config.YOffset;
                }
                else
                {
                    var data = PathDataManager.GetData(Config.RecordFromFileName, Config.RecordLineNum);
                    return (float) data.InitYOffset;
                }
            }
        }
        public float XInitSpeed => Config.XInitSpeed;
        
        public int InletId => Config.InletId;

        [Button]
        public void SpawnBallAtGivenPos()
        {
            SpawnBallAtGivenPos(Mode);
        }
        
        private void SpawnBallAtGivenPos(EMode mode)
        {
            var position = new Vector3(GetStartPosition(InletId).x + XOffset, GetStartPosition(InletId).y + YOffset);
            SpawnBall(mode, InletId, position, new Vector2(XInitSpeed, 0));
        }
        
        [PropertyOrder(2)]
        public Vector2 XOffsetRange => Config.XOffsetRandomRange;
        public static int ConcurrentBall = 0;
        [Button, PropertyOrder(2)]
        public IEnumerator SpawnBallAtRandomPos()
        {
            PathDataManager.StartRecord(InletId, XOffsetRange, XInitSpeed, Config.Count);
            var startTime = Time.realtimeSinceStartup;

            var spawnedBall = 0;
            while(spawnedBall < Config.Count)
            {
                if (ConcurrentBall < Config.MaxConcurrentBall)
                {
                    var xRandom = Random.Range(XOffsetRange.x, XOffsetRange.y);
                    var yRandom = Random.Range(Config.GetYOffsetRange(xRandom).x, Config.GetYOffsetRange(xRandom).y);
                    var position = new Vector3(GetStartPosition(InletId).x + xRandom, GetStartPosition(InletId).y + yRandom);
                    SpawnBall(Mode, InletId, position, new Vector2(XInitSpeed, 0));
                    spawnedBall++;
                    ConcurrentBall++;
                }
                else
                {
                    yield return new WaitUntil(() => ConcurrentBall < Config.MaxConcurrentBall);
                }
            }
            
            Debug.Log($"Simulate End Cost Time {Time.realtimeSinceStartup - startTime}s");
        }

        [Button]
        public void DoCompare()
        {
            SpawnBallAtGivenPos(EMode.Normal);
            SpawnBallAtGivenPos(EMode.Replay);
        }

        Vector3 GetStartPosition(int inletId)
        {
            var index = inletId - 1;
            return Inlets[index].transform.position;
        }
        
        void SpawnBall(EMode mode, int inletId, Vector2 position, Vector2 velocity)
        {
            var go = Instantiate(GetInstance(mode), position, Quaternion.identity);
            PostProcess(mode, go, velocity, inletId, position);
        }
        
        private void Awake()
        {
            // LitJsonRegister.Register();
        }
        
        void Start()
        {
            BtnSpawnBallAtPos.onClick.AddListener(SpawnBallAtGivenPos);
            BtnSpawnBallRandom.onClick.AddListener(() => StartCoroutine(SpawnBallAtRandomPos()));
        }

        void Update()
        {
            
        }
        
        public GameObject GetInstance(EMode mode)
        {
            switch (mode)
            {
                case EMode.Normal:
                    return BallInstance;
                case EMode.Record:
                    return RecordBallInstance;
                case EMode.Replay:
                    return ReplayBallInstance;
                case EMode.ReplayUI:
                    return ReplayUIBallInstance;
                default:
                    return BallInstance;
            }
        }

        void PostProcess(EMode mode, GameObject go, Vector2 velocity, int inletId, Vector2 initPosition)
        {
            if (mode == EMode.Replay)
            {
                var replayer = go.GetComponent<Replayer>();
                replayer.Init(PathDataManager.GetData(Config.ReplayFileName, Config.ReplayLineNum));
            }
            else if(mode == EMode.ReplayUI)
            {
                go.transform.SetParent(GameObject.Find("Canvas").transform);
                var replayer = go.GetComponent<Replayer>();
                replayer.Init(PathDataManager.GetData(Config.ReplayFileName, Config.ReplayLineNum));
            }
            else if(mode == EMode.Record)
            {
                var rigidbody2D = go.GetComponent<Rigidbody2D>();
                rigidbody2D.velocity = velocity;
                var recorder = go.GetComponent<Recorder>();
                var inletPos = GetStartPosition(inletId);
                recorder.RecordInParam(inletId, velocity.x, initPosition.x - inletPos.x, initPosition.y - inletPos.y);
            }
            else if(mode == EMode.Normal)
            {
                var rigidbody2D = go.GetComponent<Rigidbody2D>();
                rigidbody2D.velocity = velocity;
            }
        }
    }


    public enum EMode
    {
        Normal,
        Record,
        Replay,
        ReplayUI,
    }

    public static class LitJsonRegister
    {
        static bool registerd = false;
        public static void Register()
        {
            if (registerd) return;
            registerd = true;

            JsonMapper.RegisterExporter<float>((obj, writer) => writer.Write(Convert.ToDouble(obj)));
            JsonMapper.RegisterImporter<double, float>(input => Convert.ToSingle(input));

            // 注册Type类型的Exporter
            JsonMapper.RegisterExporter<Type>((v, w) =>
            {
                w.Write(v.FullName);
            });

            JsonMapper.RegisterImporter<string, Type>((s) =>
            {
                return Type.GetType(s);
            });

            // 注册Vector2类型的Exporter
            Action<Vector2, JsonWriter> writeVector2 = (v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.x);
                w.WriteProperty("y", v.y);
                w.WriteObjectEnd();
            };

            JsonMapper.RegisterExporter<Vector2>((v, w) =>
            {
                writeVector2(v, w);
            });

            // 注册Vector3类型的Exporter
            Action<Vector3, JsonWriter> writeVector3 = (v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.x);
                w.WriteProperty("y", v.y);
                w.WriteProperty("z", v.z);
                w.WriteObjectEnd();
            };

            JsonMapper.RegisterExporter<Vector3>((v, w) =>
            {
                writeVector3(v, w);
            });

            // 注册Vector4类型的Exporter
            JsonMapper.RegisterExporter<Vector4>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.x);
                w.WriteProperty("y", v.y);
                w.WriteProperty("z", v.z);
                w.WriteProperty("w", v.w);
                w.WriteObjectEnd();
            });

            // 注册Quaternion类型的Exporter
            JsonMapper.RegisterExporter<Quaternion>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.x);
                w.WriteProperty("y", v.y);
                w.WriteProperty("z", v.z);
                w.WriteProperty("w", v.w);
                w.WriteObjectEnd();
            });

            // 注册Color类型的Exporter
            JsonMapper.RegisterExporter<Color>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("r", v.r);
                w.WriteProperty("g", v.g);
                w.WriteProperty("b", v.b);
                w.WriteProperty("a", v.a);
                w.WriteObjectEnd();
            });

            // 注册Color32类型的Exporter
            JsonMapper.RegisterExporter<Color32>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("r", v.r);
                w.WriteProperty("g", v.g);
                w.WriteProperty("b", v.b);
                w.WriteProperty("a", v.a);
                w.WriteObjectEnd();
            });

            // 注册Bounds类型的Exporter
            JsonMapper.RegisterExporter<Bounds>((v, w) =>
            {
                w.WriteObjectStart();

                w.WritePropertyName("center");
                writeVector3(v.center, w);

                w.WritePropertyName("size");
                writeVector3(v.size, w);

                w.WriteObjectEnd();
            });

            // 注册Rect类型的Exporter
            JsonMapper.RegisterExporter<Rect>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("x", v.x);
                w.WriteProperty("y", v.y);
                w.WriteProperty("width", v.width);
                w.WriteProperty("height", v.height);
                w.WriteObjectEnd();
            });

            // 注册RectOffset类型的Exporter
            JsonMapper.RegisterExporter<RectOffset>((v, w) =>
            {
                w.WriteObjectStart();
                w.WriteProperty("top", v.top);
                w.WriteProperty("left", v.left);
                w.WriteProperty("bottom", v.bottom);
                w.WriteProperty("right", v.right);
                w.WriteObjectEnd();
            });

        }
        
        public static void WriteProperty(this JsonWriter w,string name,long value){
            w.WritePropertyName(name);
            w.Write(value);
        }

        public static void WriteProperty(this JsonWriter w,string name,string value){
            w.WritePropertyName(name);
            w.Write(value);
        }

        public static void WriteProperty(this JsonWriter w,string name,bool value){
            w.WritePropertyName(name);
            w.Write(value);
        }

        public static void WriteProperty(this JsonWriter w,string name,double value){
            w.WritePropertyName(name);
            w.Write(value);
        }
    }
}
