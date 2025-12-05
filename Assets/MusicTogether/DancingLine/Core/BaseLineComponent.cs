using MusicTogether.DancingLine.Basic;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingLine.Core
{
    /// <summary>
    /// DancingLine 组件的抽象基类
    /// 整合线条的池管理、控制器和关卡管理器
    /// </summary>
    /// <typeparam name="TNode">节点类型</typeparam>
    /// <typeparam name="TTail">线尾类型</typeparam>
    public abstract class BaseLineComponent : SerializedMonoBehaviour 
    {
        public ILevelManager LevelManager => SimpleLevelManager.Instance;
        public ILinePool pool;
        public ILineController controller;
        //public ILineFactory factory;

        public abstract void Move();
        public abstract void Turn(IDirection direction);
    }
}