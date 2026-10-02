using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestError : MonoBehaviour
{
    // 故意制造编译错误：关键词拼写错误
    public int compileErrorField = 1;

    void Start()
    {
        // 故意使用 Editor API 且不加 #if UNITY_EDITOR 包裹
        // Selection.activeGameObject = this.gameObject;
    }

    void Update()
    {
        
    }
}
