using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

public class ToggleButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab; // 按钮Prefab
    [SerializeField] private Transform ButtonHolder; // 按钮的父物体
    [SerializeField] private string[] options; // 选项文字数组
    public int selectedOptionIndex = -1; // 当前选中的选项索引，-1表示未选中
    private int extraCount
    {
        get => ButtonHolder.childCount - options.Length;
    }

    //private List<GameObject> buttons = new List<GameObject>(); // 存储生成的按钮

    private void Start()
    {
        ReorganizeButtons();
    }
    public bool CheckResources()
    {
        bool result = true;
        if (buttonPrefab == null)
        {
            Debug.LogError("Button Prefab未设置！");
            result = true;
        }
        if (ButtonHolder == null)
        {
            Debug.LogError("Button Holder未设置！");
            result = true;
        }
        if (options == null || options.Length == 0)
        {
            Debug.LogError("选项数组为空！");
            result = true;
        }
        return result;
    }
    [Button]
    public void ReorganizeButtons()
    {
        if (!CheckResources())
            return;
#if UNITY_EDITOR
        if (PrefabUtility.IsPartOfPrefabAsset(ButtonHolder))
            return;
        if (Application.isPlaying) return; // 运行时不做处理
#endif
        if (extraCount > 0)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += (() => {
                for (int i = 0; i < extraCount; i++)
                {
                    DestroyImmediate(ButtonHolder.GetChild(0).gameObject);
                }
            });
#endif
            for (int i = 0; i < extraCount; i++)
            {
                Destroy(ButtonHolder.GetChild(0).gameObject);
            }
        }
        if(extraCount < 0)
        {
            GameObject button = null;
#if UNITY_EDITOR
            for (int i = extraCount; i < 0; i++)
            {
                button = (GameObject)PrefabUtility.InstantiatePrefab(buttonPrefab, ButtonHolder);
                if (button == null)
                {
                    Debug.LogError("按钮Prefab实例化失败！");
                    continue;
                }
            }
#endif
            for (int i = extraCount; i < 0; i++)
            {
                button = Instantiate(buttonPrefab, ButtonHolder);
                if (button == null)
                {
                    Debug.LogError("按钮Prefab实例化失败！");
                    continue;
                }
            }
        }

        UpdateButtonInformation();
    }
    public void UpdateButtonInformation()
    {
        if (extraCount != 0)
            ReorganizeButtons();
        for (int i = 0; i < options.Length; i++)
        {
            ButtonHolder.GetChild(i).name = "Option " + i + ":" + options[i];
            ButtonHolder.GetChild(i).GetComponent<ToggleButton>().Content = options[i];
            Button uiButton = ButtonHolder.GetChild(i).GetComponent<Button>();
            if (uiButton != null)
            {
                int currentIndex = i; // 捕获当前循环的索引，这一行不能省！！！
                uiButton.onClick.AddListener(() => OnButtonClicked(currentIndex));
            }
            else
            {
                Debug.LogError("按钮Prefab上没有Button组件！");
            }
        }
    }
    // 在编辑器中验证时动态生成按钮
    void OnValidate()
    {
        ReorganizeButtons();
    }

    // 按钮点击事件处理
    private void OnButtonClicked(int index)
    {
        // 如果点击的是同一个按钮，直接返回
        if (selectedOptionIndex == index)
        {
            return;
        }
        // 更新选中状态
        selectedOptionIndex = index;

        // 这里可以添加其他逻辑，比如更新UI显示选中状态
        UpdateButtonStates();
    }

    // 更新按钮状态，比如高亮选中的按钮
    private void UpdateButtonStates()
    {
        foreach(ToggleButton i in GetComponentsInChildren<ToggleButton>())
        {
            i.SetState(selectedOptionIndex == i.transform.GetSiblingIndex());
        }
    }
    // 如果需要重新设置选项，可以调用此方法
    public void SetOptions(string[] newOptions)
    {
        options = newOptions;
        ReorganizeButtons();
        UpdateButtonInformation();
    }
}