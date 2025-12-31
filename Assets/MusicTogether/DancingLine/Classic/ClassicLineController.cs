using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Classic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MusicTogether.DancingLine.Basic
{
    public class ClassicLineController : SerializedMonoBehaviour , ILineController
    {
        public event Action<int?> OnDirectionChanged;
        
        protected void InvokeDirectionChanged(int? newDirectionID = null)
        {
            OnDirectionChanged?.Invoke(newDirectionID);
        }
        
        public virtual void DetectInput(MotionType currentMotionType)
        {
            switch (currentMotionType)
            {
                case MotionType.Grounded:
                    goto enableTurn;
                case MotionType.FallingToGrounded:
                    goto enableTurn;
                default:
                    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                    {
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            //InvokeDirectionChanged();
                        }
                    }
                    break;
                enableTurn:
                    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                    {
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            //NextDirection();
                            InvokeDirectionChanged();
                        }
                    }
                    break;
            }
        }
    }
}