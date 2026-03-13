using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MusicTogether.DancingBall.Archived_EditorTool
{
    /// <summary>
    /// 编辑操作调度器。
    /// 通过 <see cref="Dispatch"/> 调用 EditorTool 中的方法，
    /// 并在方法执行完毕后自动根据 <see cref="AfterActionAttribute"/> 的声明依次触发后置方法。
    ///
    /// <para>使用示例：</para>
    /// <code>
    /// var ctx = EditorActionContext.ForRoadAndBlock(map, roadIndex, blockIndex);
    /// dispatcher.Dispatch(nameof(EditorTool.OnBlockDisplacementRuleChanged), ctx);
    /// </code>
    /// </summary>
    public class EditorActionDispatcher : MonoBehaviour
    {
        [SerializeField] private EditorTool editorTool;

        // 缓存反射结果，避免每次调用都重复反射
        private Dictionary<string, MethodInfo> _methodCache;

        // ── 每条调用链共享的去重集合（防止循环后置触发）─────────────────────
        // 仅在顶层 Dispatch 创建，递归时传入同一个实例
        private HashSet<string> _executedInCurrentChain;

        private void Awake()
        {
            BuildMethodCache();
        }

        /// <summary>懒初始化缓存，确保编辑器模式下也能正常使用</summary>
        private void EnsureMethodCache()
        {
            if (_methodCache != null) return;
            BuildMethodCache();
        }

        private void BuildMethodCache()
        {
            _methodCache = new Dictionary<string, MethodInfo>();
            var type = typeof(EditorTool);
            // 只缓存接受单个 EditorActionContext 参数的公共/私有方法
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(EditorActionContext))
                {
                    _methodCache[method.Name] = method;
                }
            }
        }

        /// <summary>
        /// 执行指定方法，并根据其 <see cref="AfterActionAttribute"/> 自动触发后置链。
        /// 顶层入口，自动创建本次调用链的去重集合。
        /// </summary>
        /// <param name="methodName">EditorTool 中的方法名，建议使用 nameof()</param>
        /// <param name="ctx">本次操作的上下文</param>
        public void Dispatch(string methodName, EditorActionContext ctx)
        {
            EnsureMethodCache();

            if (editorTool == null)
            {
                Debug.LogError("[EditorActionDispatcher] editorTool 未赋值，请在 Inspector 中将 EditorTool 组件拖入。");
                return;
            }

            _executedInCurrentChain = new HashSet<string>();
            DispatchInternal(methodName, ctx);
            _executedInCurrentChain = null;
        }

        // 递归内部实现
        private void DispatchInternal(string methodName, EditorActionContext ctx)
        {
            if (_executedInCurrentChain.Contains(methodName))
            {
                Debug.LogWarning($"[EditorActionDispatcher] 检测到循环后置依赖，跳过重复执行: {methodName}");
                return;
            }

            if (!_methodCache.TryGetValue(methodName, out var method))
            {
                Debug.LogError($"[EditorActionDispatcher] 未找到方法: {methodName}。" +
                               $"请确认 EditorTool 中存在接受 EditorActionContext 参数的同名方法。");
                return;
            }

            // 标记为已执行（先标记，防止方法内部再次触发相同方法）
            _executedInCurrentChain.Add(methodName);

            // 执行方法本体
            method.Invoke(editorTool, new object[] { ctx });

            // 读取后置链并依次触发
            var afterActions = method.GetCustomAttributes<AfterActionAttribute>();
            foreach (var attr in afterActions)
            {
                DispatchInternal(attr.MethodName, ctx);
            }
        }
    }
}
