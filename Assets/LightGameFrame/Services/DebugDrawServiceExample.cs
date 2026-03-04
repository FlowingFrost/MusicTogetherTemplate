using UnityEngine;
using System.Collections.Generic;

namespace LightGameFrame.Services
{
    /// <summary>
    /// DebugDrawService 使用示例 - 演示三种绘制模式
    /// </summary>
    public class DebugDrawServiceExample : MonoBehaviour
    {
        [Header("Example Settings")]
        [SerializeField] private bool drawExamples = true;

        private DebugDrawService _debugDraw;
        private float _timer = 0f;

        private void Start()
        {
            // 获取 DebugDrawService 实例
            _debugDraw = DebugDrawService.Instance;
            
            // 示例：永久绘制 - 游戏开始时绘制一次，一直保持
            DrawPermanentExamples();
        }

        private void Update()
        {
            if (!drawExamples || _debugDraw == null) return;

            _timer += Time.deltaTime;

            // 示例1: 一次性绘制 - 每帧都需要调用
            DrawOnceExamples();

            // 示例2: 持续绘制 - 定期触发，持续一段时间
            if (_timer >= 2f)
            {
                DrawDurationExamples();
                _timer = 0f;
            }
        }

        /// <summary>
        /// 一次性绘制示例 - 只在当前帧显示
        /// 适用场景：需要实时跟随的调试线，每帧更新的动态内容
        /// </summary>
        private void DrawOnceExamples()
        {
            Vector3 origin = transform.position;

            // Mode = Once (默认)：每帧绘制，只显示一帧
            // 适合需要实时跟随的调试信息
            _debugDraw.DrawLine(origin, origin + Vector3.up * 2, Color.red, DrawMode.Once);
            
            // 绘制实时跟随的坐标轴
            _debugDraw.DrawAxes(origin + Vector3.right * 2, DrawMode.Once, 1f);
            
            // 绘制实时位置的球体
            _debugDraw.DrawSphere(origin + Vector3.left * 2, 0.5f, Color.yellow, DrawMode.Once);
            
            // 显示实时文本信息
            _debugDraw.DrawText(origin + Vector3.up * 3, 
                $"Frame: {Time.frameCount}", 
                Color.white, 
                DrawMode.Once, 
                14);
        }

        /// <summary>
        /// 持续绘制示例 - 持续显示指定时间
        /// 适用场景：临时标记、事件触发点、碰撞检测可视化
        /// </summary>
        private void DrawDurationExamples()
        {
            Vector3 basePos = transform.position + Vector3.forward * 5;

            // Mode = Duration：持续显示3秒
            // 适合标记某个瞬时事件，但希望保留一段时间供观察
            _debugDraw.DrawBox(basePos, Vector3.one, Color.green, DrawMode.Duration, 3f);
            
            // 绘制一个持续2秒的箭头，表示某个方向
            _debugDraw.DrawArrow(
                basePos + Vector3.up, 
                basePos + Vector3.up * 2, 
                Color.cyan, 
                DrawMode.Duration, 
                0.3f, 
                20f, 
                2f
            );
            
            // 绘制持续5秒的贝塞尔曲线
            Vector3 bezierStart = basePos + Vector3.left * 2;
            Vector3 bezierEnd = bezierStart + Vector3.forward * 3;
            _debugDraw.DrawBezier(
                bezierStart, 
                bezierStart + Vector3.up * 2,
                bezierEnd + Vector3.up * 2,
                bezierEnd,
                Color.magenta,
                DrawMode.Duration,
                30,
                5f
            );
            
            // 绘制持续1.5秒的圆柱体
            _debugDraw.DrawCylinder(
                basePos + Vector3.right * 2, 
                basePos + Vector3.right * 2 + Vector3.up * 2, 
                0.3f, 
                Color.blue, 
                DrawMode.Duration,
                16, 
                1.5f
            );
        }

        /// <summary>
        /// 永久绘制示例 - 一直显示直到手动清除
        /// 适用场景：地图边界、关卡参考线、导航网格、固定调试信息
        /// </summary>
        private void DrawPermanentExamples()
        {
            Vector3 basePos = transform.position + Vector3.back * 5;

            // Mode = Permanent：永久显示，不会自动消失
            // 适合绘制场景中的固定参考元素
            
            // 绘制永久的地面网格
            _debugDraw.DrawGrid(
                basePos, 
                10, 
                1f, 
                new Color(0.5f, 0.5f, 0.5f, 0.5f), 
                DrawMode.Permanent
            );
            
            // 绘制永久的世界坐标轴
            _debugDraw.DrawAxes(basePos + Vector3.up * 0.01f, DrawMode.Permanent, 2f);
            
            // 绘制永久的边界框
            Bounds bounds = new Bounds(basePos + Vector3.up * 2, Vector3.one * 3);
            _debugDraw.DrawBounds(bounds, Color.yellow, DrawMode.Permanent);
            
            // 绘制永久的圆形区域标记
            _debugDraw.DrawCircle(
                basePos, 
                5f, 
                Vector3.up, 
                Color.green, 
                DrawMode.Permanent,
                64
            );
            
            // 绘制永久的标签
            _debugDraw.DrawText(
                basePos + Vector3.up * 4, 
                "Permanent Reference Point", 
                Color.cyan, 
                DrawMode.Permanent,
                16
            );
        }

        private void OnGUI()
        {
            if (_debugDraw == null) return;

            // 显示当前绘制命令数量和控制面板
            GUILayout.BeginArea(new Rect(10, 10, 400, 150));
            
            GUILayout.Label($"Debug Draw Commands: {_debugDraw.GetTotalCommandCount()}");
            GUILayout.Label("绘制模式说明:");
            GUILayout.Label("• Once - 一次性绘制（每帧需要调用）");
            GUILayout.Label("• Duration - 持续绘制（指定时间后消失）");
            GUILayout.Label("• Permanent - 永久绘制（手动清除）");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("清除所有绘制"))
            {
                _debugDraw.ClearAll();
                // 重新绘制永久示例
                DrawPermanentExamples();
            }
            
            if (GUILayout.Button("重新绘制永久示例"))
            {
                DrawPermanentExamples();
            }
            
            GUILayout.EndArea();
        }
    }
}
