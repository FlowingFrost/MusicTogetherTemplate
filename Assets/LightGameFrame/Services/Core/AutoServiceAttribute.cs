using System;

namespace LightGameFrame.Services
{
    /// <summary>
    /// 自动服务标记
    /// 标记此特性的 Service 类会被 ServiceManager 在启动时自动扫描，可声明是否自动创建
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AutoServiceAttribute : Attribute
    {
        /// <summary>
        /// 是否即使场景中已存在也强制创建新的（通常为 false）
        /// </summary>
        public bool ForceCreate { get; set; } = false;

        public AutoServiceAttribute(bool forceCreate = false)
        {
            ForceCreate = forceCreate;
        }
    }
}