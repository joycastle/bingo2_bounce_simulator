using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BounceBallPositionTranslate : MonoBehaviour
{
    public GameObject go;
    public Vector2 ScreenSize = new Vector2(1080, 1920);
    public Vector2 RecordWorldPos;
    public Vector2 CurrentWorldPos;
    public Canvas Canvas;
    public Camera MainCamera;
    public Camera UICamera;
    // Start is called before the first frame update
    void Start()
    {
        // CalculatePositionTranslate();
    }

    // protected Vector2 ScreenLeftDownWorldPosition;
    public Vector2 GetModifiedPos(Vector2 recordPos)
    {
        // return UIPosToUIWorldPos(WorldPosToUIPos(recordPos));
        return AlignUIElementWithWorldPoint(recordPos, MainCamera, UICamera, Canvas);
    }
    
    // Vector2 WorldPosToUIPos(Vector2 worldPos)
    // {
    //     return (worldPos + _delat) * _scale;
    // }
    //
    // Vector2 UIPosToUIWorldPos(Vector2 uiPos)
    // {
    //     return uiPos + ScreenSize / 2;
    // }
    //
    private Vector2 _delat;
    private float _scale;
    public void CalculatePositionTranslate()
    {
        // _scale = 96;
        // var oriPosition = Vector2.zero;
        // var newPos = GetScreenCenterWorldPos();
        // delta = newPos / scale - oriPosition;
        // _delat = Vector2.zero;
    }
    
    public Vector2 AlignUIElementWithWorldPoint(Vector3 worldPosition, Camera mainCamera, Camera uiCamera, Canvas canvas)
    {
        // 从世界坐标到主相机的屏幕坐标
        // Vector3 screenPositionMain = mainCamera.WorldToScreenPoint(worldPosition);
        Vector3 screenPositionMain = MyWorldToScreenPointOrtho(mainCamera, worldPosition);

        // 计算两个相机size的比例
        //float sizeRatio = uiCamera.orthographicSize / mainCamera.orthographicSize;

        // 转换坐标
        Vector3 screenPositionUI = screenPositionMain;//new Vector3(screenPositionMain.x * sizeRatio, screenPositionMain.y * sizeRatio, screenPositionMain.z);

        // 屏幕坐标转 Canvas 坐标
        Vector2 canvasPosition;
        // RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        // RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPositionUI, uiCamera, out canvasPosition);
        canvasPosition.x = screenPositionUI.x - ScreenSize.x / 2;
        canvasPosition.y = screenPositionUI.y - ScreenSize.y / 2;

        // 设置 UI 组件的位置
        return canvasPosition;
    }
    
    public Vector3 MyWorldToScreenPointOrtho(Camera camera, Vector3 worldPosition)
    {
        // Step 1: World to Camera
        Vector3 cameraPosition = worldPosition;//camera.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPosition);
    
        // Step 2: Camera to Screen
        float screenX = (cameraPosition.x / (camera.orthographicSize * camera.aspect) + 1f) * 0.5f * ScreenSize.x;
        float screenY = (cameraPosition.y / camera.orthographicSize + 1f) * 0.5f * ScreenSize.y;

        return new Vector3(screenX, screenY, cameraPosition.z);
    }

    // Vector2 GetScreenCenterWorldPos()
    // {
    //     Vector2 ret;
    //     // 确保 Canvas 和 Camera 已经被指定
    //     if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay && Camera != null)
    //     {
    //         // Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
    //         // ret = Camera.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, Camera.nearClipPlane));
    //         // Debug.Log("World center in Screen Space Overlay: " + worldCenter);
    //         ret = new Vector2(Screen.width / 2, Screen.height / 2);
    //     }
    //     else
    //     {
    //         ret = Vector2.zero;
    //     }
    //
    //     return ret;
    // }

    [Button]
    void UpdatePos()
    {
        Start();
        if (go != null)
        {
            RecordWorldPos = go.transform.position;
        }
        CurrentWorldPos = GetModifiedPos(RecordWorldPos);
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = CurrentWorldPos;
    }
    
    [Button]
    // Update is called once per frame
    void UpdateWorldPos()
    {
        CurrentWorldPos = transform.position;
    }
}
