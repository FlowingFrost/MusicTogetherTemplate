using System;
using MusicTogether.DancingLine.Interfaces;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace MusicTogether.DancingLine.Classic
{
    public class ClassicLineComponent : SerializedMonoBehaviour, ILineComponent
    {
        //绑定信息
        [SerializeField]protected ILevelManager levelManager;
        [SerializeField]protected ILinePool pool;
        [SerializeField]protected ILineController controller;
        [SerializeField]protected Transform lineHeadTransform;

        //临时数据
        internal double cachedBeginTime;
        //运行参数
        protected double time => levelManager.LevelTime;
        public LevelState LevelState => levelManager.CurrentLevelState;

        [SerializeField] internal TextMeshProUGUI debugText;
        internal string debugInfo;
        
        public void Turn()
        {
            debugInfo += $"[{LevelState.ToString()}] Turn input received at time {time} in state {LevelState}\n";
            debugText.text = debugInfo;
            switch (LevelState)
            {
                case LevelState.Playing:
                    pool.AddNode(NodeInputType.Turn, time);
                    break;
                case LevelState.Previewing:
                    levelManager.SetLevelState(LevelState.Playing);
                    pool.ClearNodesAfterTime(time);
                    //goto case LevelState.Playing;
                    pool.AddNode(NodeInputType.Turn, time);
                    break;
                default:
                    break;        
            }
        }
        
        public void ClearNodesAfterTime(double? time)
        {
            pool.ClearNodesAfterTime(time);
        }

        public void ClearNodesAfterNow()
        {
            pool.ClearNodesAfterTime(time);
        }
        
        public void Move()
        {
            var currentMotion = pool.UpdatePool(time);
            lineHeadTransform.position = currentMotion.WorldSpacePosition;
            lineHeadTransform.rotation = currentMotion.WorldSpaceRotation;
        }
        
        //生命周期
        public virtual void AwakeUnion()
        {
            // 先取消订阅，防止重复订阅
            controller.OnInputDetected -= Turn;
            controller.OnInputDetected += Turn;
            //levelManager.OnLevelStateChanged += OnLevelStateChanged;
        }

        public void StartUnion(double startTime = 0d)
        {
            cachedBeginTime = startTime;
            /*var currentMotion = pool.Init();
            transform.localPosition = currentMotion.Position;
            lineHeadTransform.localRotation = currentMotion.Rotation;*/
        }

        public virtual void UpdateUnion()
        {
            //Move();
        }

        private void Update()
        {
            Move();
        }
    }
}