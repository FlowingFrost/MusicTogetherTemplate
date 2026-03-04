using System;
using MusicTogether.DancingLine.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MusicTogether.DancingLine.Classic
{
    public class ClassicLineController : MonoBehaviour , ILineController
    {
        public event Action OnInputDetected;
        
        [SerializeField] internal TextMeshProUGUI debugText;
        internal string debugInfo;
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    debugInfo += $"Input detected at time {Time.time}\n Events:{OnInputDetected}";
                    debugText.text = debugInfo;
                    OnInputDetected?.Invoke();
                }
            }
        }
    }
}