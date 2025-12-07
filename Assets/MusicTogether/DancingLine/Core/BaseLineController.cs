using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    public abstract class BaseLineController : SerializedMonoBehaviour, ILineController
    {
        protected Action OnInputDetected;
        protected Action<IDirection> OnDirectionChanged;
        [OdinSerialize]protected List<BaseDirection> Directions = new List<BaseDirection>();
        
        protected int CurrentDirectionIndex = 0;
        
        public virtual void Register(Action turnCallback,Action<IDirection> changeDirCallback)
        {
            OnInputDetected += turnCallback;
            OnDirectionChanged += changeDirCallback;
        }
        public virtual IDirection CurrentDirection()
        {
            return Directions[CurrentDirectionIndex];
        }
        
        public virtual bool GetDirectionByID(int targetID, out IDirection direction)
        {
            foreach (var dir in Directions)
            {
                if (dir.ID == targetID)
                {
                    direction = dir;
                    return true;
                }
            }
            direction = null;
            return false;
        }
        
        public virtual void SetCurrentDirection(int targetID)
        {
            for (int i = 0; i < Directions.Count; i++)
            {
                if (Directions[i].ID == targetID)
                {
                    CurrentDirectionIndex = i;
                    return;
                }
            }
            Debug.LogError($"Direction ID {targetID} not found!");
        }
        public abstract void DetectInput();
    }
}