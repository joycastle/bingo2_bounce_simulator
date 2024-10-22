using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LaunchConfig", menuName = "SO/LaunchConfig")]
public class LaunchConfig : ScriptableObject
{
    public EMode Mode;
    
    [ShowIf("IsRecordOrNormalMode")]
    public float XInitSpeed = 0f;
    [ShowIf("IsRecordOrNormalMode")]
    [Header("出球入口Id，决定球的初始位置，从左往右，从1开始")]
    public int InletId = 1;
    [ShowIf("IsRecordOrNormalMode")]
    [BoxGroup("指定出球参数"), Header("球的x坐标的偏移量")]
    public float XOffset = 0f;
    [ShowIf("IsRecordOrNormalMode")]
    [BoxGroup("指定出球参数"), Header("球的y坐标的偏移量")]
    public float YOffset = 0f;
    [ShowIf("IsRecordOrNormalMode")]
    [BoxGroup("随机出球参数"), Header("球的x坐标偏移的随机范围（均匀随机）")]
    public Vector2 XOffsetRandomRange = new Vector2(-1, 1);

    [ShowIf("IsReplayMode")] 
    public int ReplayId = 1;

    //随机出球时，y坐标的随机范围(随x坐标变化而变化,均匀随机）
    public Vector2 GetYOffsetRange(float x)
    {
        return new Vector2(-1, 1);
    }

    public bool IsReplayMode()
    {
        return Mode == EMode.Replay;
    }
    
    public bool IsRecordOrNormalMode()
    {
        return Mode == EMode.Record || Mode == EMode.Normal;
    }
}
