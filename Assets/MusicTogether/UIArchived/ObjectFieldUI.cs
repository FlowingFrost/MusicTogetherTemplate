using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ObjectFieldUI : MonoBehaviour
{
    public Object storedObject;
    public TextMeshProUGUI nameText;
    void Start()
    {
        nameText.text = storedObject.name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SelectObject()
    {
        var filePath = EditorUtility.OpenFilePanel("选择打开文件", UnityEngine.Application.dataPath,"*");

    }
}
