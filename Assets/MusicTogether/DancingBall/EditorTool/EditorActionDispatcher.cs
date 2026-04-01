using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MusicTogether.DancingBall.EditorTool
{
    /// <summary>
    /// 简单的 EditorTool 方法调度器，支持 AfterAction 链。
    /// </summary>
    public class EditorActionDispatcher : MonoBehaviour
    {
        [SerializeField] private EditManager editManager;

        private Dictionary<string, MethodInfo> _methodCache;
        private HashSet<string> _executedInCurrentChain;

        private void Awake()
        {
            BuildMethodCache();
        }

        private void EnsureMethodCache()
        {
            if (_methodCache == null) BuildMethodCache();
        }

        private void BuildMethodCache()
        {
            _methodCache = new Dictionary<string, MethodInfo>();
            var type = typeof(EditManager);
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var ps = method.GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType == typeof(EditorActionContext))
                {
                    _methodCache[method.Name] = method;
                }
            }
        }

        public void Dispatch(string methodName, EditorActionContext ctx)
        {
            EnsureMethodCache();
            if (editManager == null)
            {
                Debug.LogError("[EditorActionDispatcher] editManager 未赋值。");
                return;
            }

            _executedInCurrentChain = new HashSet<string>();
            DispatchInternal(methodName, ctx);
            _executedInCurrentChain = null;
        }

        private void DispatchInternal(string methodName, EditorActionContext ctx)
        {
            if (_executedInCurrentChain.Contains(methodName))
            {
                Debug.LogWarning($"[EditorActionDispatcher] 循环后置调用，跳过: {methodName}");
                return;
            }

            if (!_methodCache.TryGetValue(methodName, out var method))
            {
                Debug.LogError($"[EditorActionDispatcher] 未找到方法: {methodName}");
                return;
            }

            _executedInCurrentChain.Add(methodName);
            method.Invoke(editManager, new object[] { ctx });

            foreach (var attr in method.GetCustomAttributes<AfterActionAttribute>())
            {
                DispatchInternal(attr.MethodName, ctx);
            }
        }
    }
}
