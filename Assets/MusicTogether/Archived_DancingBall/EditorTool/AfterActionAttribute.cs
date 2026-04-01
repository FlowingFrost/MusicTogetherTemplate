using System;

namespace MusicTogether.Archived_DancingBall.EditorTool
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AfterActionAttribute : Attribute
    {
        public string MethodName { get; }

        public AfterActionAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}
