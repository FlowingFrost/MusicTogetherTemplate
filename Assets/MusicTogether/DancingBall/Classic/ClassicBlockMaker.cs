using System;
using System.Collections.Generic;
using MusicTogether.DancingBall.Interfaces;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace MusicTogether.DancingBall.Classic
{
    public class ClassicBlockMaker : MonoBehaviour, IBlockMaker
    {
        [SerializeField] protected IBlock block;
        [ReadOnly] protected bool hasTurn;
        [OnValueChanged(nameof(UpdateDisplacement))]
        [SerializeField] protected TurnType turnType;
        [OnValueChanged(nameof(UpdateDisplacement))]
        [SerializeField] protected bool hasRule;
        [OnValueChanged(nameof(UpdateDisplacement))]
        [SerializeField] protected DisplacementType displacementType;
        
        public IBlock Block => block;
        public bool HasTap
        {
            get => hasTurn;
            set => hasTurn = value;
        }
        public bool HasRule
        {
            get => hasRule;
            set { hasRule = value; UpdateDisplacement.Invoke(); }
        }

        public TurnType TurnType => turnType;
        public DisplacementType DisplacementType => displacementType;
        public Action UpdateDisplacement => () => EditManager.Instance.OnDisplacementChanged(block);
    }
}