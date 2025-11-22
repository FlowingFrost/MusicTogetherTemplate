using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// 脚本状态节点
    /// 
    /// 设计理念：
    /// - 节点本身不包含执行逻辑，纯粹作为脚本的代理
    /// - 将状态机的生命周期转发给场景中的 IStateHandler 脚本
    /// - 支持多个绑定字段，脚本通过 context.GetBinding() 访问
    /// 
    /// 使用方式：
    /// 1. 在 State Graph 中创建此节点
    /// 2. 在场景中挂载实现 IStateHandler 的 MonoBehaviour
    /// 3. 在 Inspector 中绑定脚本到节点的 "targetScript" 字段
    /// 4. 可选：绑定额外的对象（如 Animator、武器等）
    /// 5. 脚本通过 context.GetBinding<T>("fieldName") 获取绑定对象
    /// 6. 脚本完成后调用 context.RequestComplete() 触发下一状态
    /// </summary>
    [Serializable]
    [NodeMenuItem("State Machine/Script State")]
    public class ScriptStateNode : BaseStateNode
    {
        // ============================================
        // 序列化配置（在编辑器中显示）
        // ============================================
        
        [Tooltip("目标脚本的绑定字段名（默认为 'targetScript'）\n脚本必须实现 IStateHandler 接口")]
        public string targetScriptField = "targetScript";
        
        [Tooltip("额外绑定的对象字段名列表（可选）\n用于脚本中通过 context.GetBinding() 获取其他对象")]
        public string[] additionalBindingFields = new string[0];
        
        // ============================================
        // 运行时缓存
        // ============================================
        
        // 缓存的脚本引用（初始化时获取，避免每帧查找）
        private IStateHandler _cachedHandler;
        
        // 是否已初始化
        private bool _isInitialized;
        
        // 状态上下文（传递给脚本的执行环境）
        private StateContext _context;
        
        // ============================================
        // 节点属性
        // ============================================
        
        public override string name => "Script State";
        public override Color color => new Color(0.4f, 0.8f, 0.5f); // 绿色，表示脚本驱动
        
        // ============================================
        // 初始化逻辑
        // ============================================
        
        /// <summary>
        /// 确保节点已初始化（延迟初始化模式）
        /// 首次调用时从状态机获取绑定的脚本，并创建上下文
        /// </summary>
        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            // 检查状态机类型
            if (!(_stateMachine is NGPStateMachine ngpSM))
            {
                Debug.LogError($"[{name}] StateMachine is not NGPStateMachine! Node: {GUID}");
                return;
            }
            
            // 获取绑定的脚本（使用现有的 GetComponentBinding 机制）
            var scriptObj = ngpSM.GetComponentBinding<MonoBehaviour>(GUID, targetScriptField);
            
            if (scriptObj == null)
            {
                Debug.LogError($"[{name}] No script bound to field '{targetScriptField}'! Please bind a MonoBehaviour in the Inspector. Node: {GUID}");
                return;
            }
            
            // 检查脚本是否实现 IStateHandler 接口
            if (!(scriptObj is IStateHandler handler))
            {
                Debug.LogError($"[{name}] Script {scriptObj.GetType().Name} does not implement IStateHandler! Node: {GUID}");
                return;
            }
            
            _cachedHandler = handler;
            
            // 创建状态上下文（传递给脚本）
            _context = new StateContext
            {
                StateMachine = ngpSM,
                NodeId = GUID
            };
            
            Debug.Log($"[{name}] Initialized with script: {scriptObj.GetType().Name}, Node: {GUID}");
        }
        
        // ============================================
        // 生命周期转发（IStateNode 接口实现）
        // ============================================
        
        /// <summary>
        /// 接收 OnEnter 信号，转发给脚本
        /// </summary>
        public override void OnEnterSignal(string sourceId)
        {
            EnsureInitialized();
            
            if (_cachedHandler == null)
            {
                Debug.LogWarning($"[{name}] Cannot enter: handler not initialized. Node: {GUID}");
                return;
            }
            
            // 更新上下文
            _context.SourceId = sourceId;
            
            // 转发给脚本
            try
            {
                _cachedHandler.OnStateEnter(_context);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{name}] Error in OnStateEnter: {e.Message}\n{e.StackTrace}");
            }
        }
        
        /// <summary>
        /// 接收 OnExit 信号，转发给脚本
        /// </summary>
        public override void OnExitSignal(string sourceId)
        {
            EnsureInitialized();
            
            if (_cachedHandler == null) return;
            
            // 更新上下文
            _context.SourceId = sourceId;
            
            // 转发给脚本
            try
            {
                _cachedHandler.OnStateExit(_context);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{name}] Error in OnStateExit: {e.Message}\n{e.StackTrace}");
            }
            
            // 停止节点运行
            StopRunning();
        }
        
        /// <summary>
        /// 每帧更新，转发给脚本
        /// </summary>
        public override void OnUpdate()
        {
            if (_cachedHandler == null) return;
            
            try
            {
                _cachedHandler.OnStateUpdate(_context);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{name}] Error in OnStateUpdate: {e.Message}\n{e.StackTrace}");
            }
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Cleanup()
        {
            _isInitialized = false;
            _cachedHandler = null;
            _context = null;
        }
        
        // ============================================
        // 编辑器辅助信息
        // ============================================
        
        /// <summary>
        /// 获取节点的提示信息（在编辑器中显示）
        /// </summary>
        public string GetEditorTooltip()
        {
            var tooltip = "Script State Node\n\n";
            tooltip += $"Target Script Field: {targetScriptField}\n";
            
            if (additionalBindingFields != null && additionalBindingFields.Length > 0)
            {
                tooltip += $"Additional Bindings: {string.Join(", ", additionalBindingFields)}\n";
            }
            
            tooltip += "\nBindings can be accessed in script via:\n";
            tooltip += "context.GetBinding<T>(\"fieldName\")";
            
            return tooltip;
        }
    }
}
