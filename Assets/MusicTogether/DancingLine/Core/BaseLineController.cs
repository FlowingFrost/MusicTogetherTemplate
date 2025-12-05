using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    public abstract class BaseLineController : SerializedMonoBehaviour, ILineController
    {
        protected Action<IDirection> OnDirectionChanged;
        [OdinSerialize]protected List<BaseDirection> Directions = new List<BaseDirection>();
        
        protected int CurrentDirectionIndex = 0;
        
        public virtual void RegisterTurn(Action<IDirection> callback)
        {
            OnDirectionChanged += callback;
        }
        public virtual IDirection CurrentDirection()
        {
            return Directions[CurrentDirectionIndex];
        }
        public virtual void SetCurrentDirection(int ID)
        {
            for (int i = 0; i < Directions.Count; i++)
            {
                if (Directions[i].ID == ID)
                {
                    CurrentDirectionIndex = i;
                    return;
                }
            }
            Debug.LogError($"Direction ID {ID} not found!");
        }
        public abstract void DetectInput();
    }
}