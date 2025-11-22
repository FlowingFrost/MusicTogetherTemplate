using MusicTogether.DancingLine.Basic;
using MusicTogether.LevelManagement;
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
    public abstract class BaseLineComponent<TController,TDirection,TPool,TNode,TTail> : MonoBehaviour 
        where TTail : BaseLineTail
        where TNode : BaseLineNode<TTail>
        where TPool : BaseLinePool<TNode,TTail>, new()
        where TDirection : BaseDirection
        where TController : BaseLineController<TDirection>, new()
    {
        public ILevelManager LevelManager => SimpleLevelManager.Instance;
        public TPool pool = new TPool();
        public TController controller = new TController();

        public abstract void Move();
        public abstract void Turn(TDirection direction);
    }
}