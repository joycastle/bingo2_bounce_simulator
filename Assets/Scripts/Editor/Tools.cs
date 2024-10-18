using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Tools
{
    [MenuItem("Tools/GeometryPlace")]
    public static void GeometryPlace()
    {
        // 获取所有选中的游戏对象
        var selectedObjects = Selection.gameObjects;

        // 没有选中对象时直接返回
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected!");
            return;
        }

        // 根据X坐标排序对象
        var sortedObjects = selectedObjects.OrderBy(obj => obj.transform.position.x).ToArray();

        // 获取最左和最右的对象
        var leftMost = sortedObjects.First();
        var rightMost = sortedObjects.Last();

        // 计算Y和Z坐标的平均值
        float averageY = (leftMost.transform.position.y + rightMost.transform.position.y) / 2;
        float averageZ = (leftMost.transform.position.z + rightMost.transform.position.z) / 2;

        // 设置所有对象的Y和Z坐标
        foreach (var obj in selectedObjects)
        {
            var pos = obj.transform.position;
            obj.transform.position = new Vector3(pos.x, averageY, averageZ);
        }

        // 均匀分布X坐标
        float leftX = leftMost.transform.position.x;
        float rightX = rightMost.transform.position.x;
        float step = (rightX - leftX) / (selectedObjects.Length - 1);
        for (int i = 0; i < sortedObjects.Length; i++)
        {
            var obj = sortedObjects[i];
            obj.transform.position = new Vector3(leftX + step * i, averageY, averageZ);
        }
    }

    [MenuItem("Tools/YAxisSymmetry")]
    public static void YAxisSymmetry()
    {
        // 获取所有当前选中的游戏对象
        YAxisSymmetry(Selection.gameObjects.ToList());
    }

    public static List<GameObject> YAxisSymmetry(List<GameObject> gos)
    {
        var ret = new List<GameObject>();
        foreach (GameObject original in gos)
        {
            GameObject clone;

            // 检查是否为Prefab实例
            if (PrefabUtility.IsPartOfPrefabInstance(original))
            {
                // 从Prefab复制实例
                clone = (GameObject)PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(original), original.transform.parent);
                clone.transform.localScale = original.transform.localScale;
                clone.transform.rotation = original.transform.rotation;
            }
            else
            {
                // 不是Prefab实例，常规复制
                clone = GameObject.Instantiate(original, original.transform.parent);
            }

            // 给复制的对象一个新的名字
            clone.name = original.name + "_mirrored";

            // 计算关于Y轴对称的位置
            Vector3 mirroredPosition = new Vector3(-original.transform.position.x, original.transform.position.y, original.transform.position.z);
            clone.transform.position = mirroredPosition;

            // 反转复制对象的X轴缩放，以实现镜像效果
            Vector3 scale = clone.transform.localScale;
            scale.x = -scale.x;
            clone.transform.localScale = scale;

            // 如果有需要，也可以反转物体的旋转（不一定需要，取决于具体情况）
            // Vector3 rotation = clone.transform.eulerAngles;
            // rotation.y = -rotation.y;
            // clone.transform.eulerAngles = rotation;

            // 保持原始和复制物体的层级关系不变
            clone.transform.SetSiblingIndex(original.transform.GetSiblingIndex() + 1);
            ret.Add(clone);
        }
        
        return ret;
    }


    [MenuItem("Tools/YAxisSymmetryParent")]
    public static void YAxisSymmetryParent()
    {
        var fromParent = Selection.gameObjects[0];
        var toParent = Selection.gameObjects[1];
        var children = new List<GameObject>();
        for (int i = 0; i < fromParent.transform.childCount; i++)
        {
            children.Add(fromParent.transform.GetChild(i).gameObject);
        }

        var clones = YAxisSymmetry(children);
        foreach (var clone in clones)
        {
            clone.transform.SetParent(toParent.transform);
        }
    }

}
