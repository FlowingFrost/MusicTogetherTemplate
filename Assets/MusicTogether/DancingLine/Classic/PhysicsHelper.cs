using UnityEngine;

namespace MusicTogether.DancingLine.Classic
{
    /// <summary>
    /// 物理计算辅助类，提供统一的运动学公式计算
    /// </summary>
    public static class PhysicsHelper
    {
        /// <summary>
        /// 计算匀加速运动的位移
        /// 公式: s = v0 * t + 0.5 * a * t^2
        /// </summary>
        /// <param name="initialVelocity">初速度 v0</param>
        /// <param name="acceleration">加速度 a</param>
        /// <param name="time">时间 t</param>
        /// <returns>位移 s</returns>
        public static Vector3 CalculateDisplacement(Vector3 initialVelocity, Vector3 acceleration, float time)
        {
            return initialVelocity * time + 0.5f * acceleration * time * time;
        }
        
        /// <summary>
        /// 计算匀加速运动的末速度
        /// 公式: v = v0 + a * t
        /// </summary>
        /// <param name="initialVelocity">初速度 v0</param>
        /// <param name="acceleration">加速度 a</param>
        /// <param name="time">时间 t</param>
        /// <returns>末速度 v</returns>
        public static Vector3 CalculateVelocity(Vector3 initialVelocity, Vector3 acceleration, float time)
        {
            return initialVelocity + acceleration * time;
        }
        
        /// <summary>
        /// 计算速度变化量
        /// 公式: Δv = a * t
        /// </summary>
        /// <param name="acceleration">加速度 a</param>
        /// <param name="time">时间 t</param>
        /// <returns>速度变化量 Δv</returns>
        public static Vector3 CalculateDeltaVelocity(Vector3 acceleration, float time)
        {
            return acceleration * time;
        }
    }
}
