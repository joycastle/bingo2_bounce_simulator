using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PositionTranslate : MonoBehaviour
{
    public GameObject go;
    public Vector2 ScreenSize = new Vector2(1080, 1920);
    public Vector2 RecordWorldPos;
    public Vector2 CurrentWorldPos;
    public Canvas Canvas;
    public Camera Camera;
    // Start is called before the first frame update
    void Start()
    {
        CalculatePositionTranslate(out _delat, out _scale);
    }

    // protected Vector2 ScreenLeftDownWorldPosition;
    public Vector2 GetModifiedPos(Vector2 recordPos)
    {
        return UIPosToUIWorldPos(WorldPosToUIPos(recordPos));
    }
    
    Vector2 WorldPosToUIPos(Vector2 worldPos)
    {
        return (worldPos + _delat) * _scale;
    }
    
    Vector2 UIPosToUIWorldPos(Vector2 uiPos)
    {
        return uiPos + ScreenSize / 2;
    }

    private Vector2 _delat;
    private float _scale;
    void CalculatePositionTranslate(out Vector2 delta, out float scale)
    {
        scale = 96;
        // var oriPosition = Vector2.zero;
        // var newPos = GetScreenCenterWorldPos();
        // delta = newPos / scale - oriPosition;
        delta = Vector2.zero;
    }

    Vector2 GetScreenCenterWorldPos()
    {
        Vector2 ret;
        // 确保 Canvas 和 Camera 已经被指定
        if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay && Camera != null)
        {
            // Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            // ret = Camera.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, Camera.nearClipPlane));
            // Debug.Log("World center in Screen Space Overlay: " + worldCenter);
            ret = new Vector2(Screen.width / 2, Screen.height / 2);
        }
        else
        {
            ret = Vector2.zero;
        }

        return ret;
    }

    [Button]
    // Update is called once per frame
    void UpdatePos()
    {
        Start();
        if (go != null)
        {
            RecordWorldPos = go.transform.position;
        }
        CurrentWorldPos = GetModifiedPos(RecordWorldPos);
        transform.position = CurrentWorldPos;
    }
    
    [Button]
    // Update is called once per frame
    void UpdateWorldPos()
    {
        CurrentWorldPos = transform.position;
    }
}
