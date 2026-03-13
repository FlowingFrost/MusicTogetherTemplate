using System;

namespace MusicTogether.DancingBall.Archived_EditorTool
{
    /// <summary>
    /// 标注在 EditorTool 的方法上，声明该方法执行完毕后需要自动触发的后置方法。
    /// 可多次叠加，后置方法将按声明顺序依次执行。
    /// <example>
    /// <code>
    /// [AfterAction(nameof(RefreshBlockColor))]
    /// [AfterAction(nameof(RefreshBlockInfoDisplay))]
    /// public void UpdateBlockPlacementData(EditorActionContext ctx) { ... }
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class AfterActionAttribute : Attribute
    {
        /// <summary>完成后需要触发的后置方法名称（对应 EditorTool 中的方法名）</summary>
        public string MethodName { get; }

        public AfterActionAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}
