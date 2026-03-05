using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Interfaces;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MusicTogether.DancingLine.Classic
{
    public class ClassicLineController : SerializedMonoBehaviour , ILineController
    {
        //[SerializeField]public UnityEvent OnInputDetected;
        [OdinSerialize] internal List<ILineComponent> lineComponents;
        [SerializeField] internal TextMeshProUGUI debugText;
        internal string debugInfo;
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    debugInfo += $"Input detected at time {Time.time}\n Events:";
                    debugText.text = debugInfo;
                    //OnInputDetected?.Invoke();
                    foreach (var lineComponent in lineComponents)
                    {
                        lineComponent.Turn();
                    }
                }
            }
        }
    }
}