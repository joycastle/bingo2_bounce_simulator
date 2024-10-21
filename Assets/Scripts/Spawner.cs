using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using LitJson;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    // [SerializeField]
    public Button BtnSpawnBallAtPos;
    public Button BtnSpawnBallRandom;
    public GameObject BallInstance;
    public GameObject RecordBallInstance;
    public GameObject ReplayBallInstance;
    public EMode Mode;
    
    public float XOffset = 0f;
    public float YOffset = 0f;
    [Button]
    public void SpawnBallAtGivenPos()
    {
        var position = new Vector3(transform.position.x + XOffset, transform.position.y + YOffset,
            transform.position.z);
        var go = Instantiate(GetInstance(), position, Quaternion.identity);
        PostProcess(go);
    }
    
    [PropertyOrder(2)]
    public Vector2 XOffsetRange = new Vector2(-1, 1);
    [PropertyOrder(2)]
    public Vector2 YOffsetRange = new Vector2(-1, 1);
    [Button, PropertyOrder(2)]
    public void SpawnBallAtRandomPos()
    {
        var xRandom = Random.Range(XOffsetRange.x, XOffsetRange.y);
        var yRandom = Random.Range(YOffsetRange.x, YOffsetRange.y);
        var position = new Vector3(transform.position.x + xRandom, transform.position.y + yRandom,
            transform.position.z);
        var go = Instantiate(GetInstance(), position, Quaternion.identity);
        PostProcess(go);
    }
    
    private void Awake()
    {
        LitJsonRegister.Register();
    }
    
    void Start()
    {
        BtnSpawnBallAtPos.onClick.AddListener(SpawnBallAtGivenPos);
        BtnSpawnBallRandom.onClick.AddListener(SpawnBallAtRandomPos);
    }

    void Update()
    {
        
    }
    
    public GameObject GetInstance()
    {
        switch (Mode)
        {
            case EMode.Normal:
                return BallInstance;
            case EMode.Record:
                return RecordBallInstance;
            case EMode.Replay:
                return ReplayBallInstance;
            default:
                return BallInstance;
        }
    }

    void PostProcess(GameObject go)
    {
        if (Mode == EMode.Replay)
        {
            var replayer = go.GetComponent<Replayer>();
            replayer.Init(PathDataManager.GetData(1));
        }
    }
}


public enum EMode
{
    Normal,
    Record,
    Replay
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