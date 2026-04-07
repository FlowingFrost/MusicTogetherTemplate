using System;
using MusicTogether.DancingBall.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace MusicTogether.DancingBall.EditorTool.UIManager
{
    public class DisplacementOverlayManager : UIManagerBase
    {
        private Label _titleLabel;
        private Toggle _enableToggle;
        private Label _infoLabel;
        private Label _legendLabel;
        private VisualElement _graphElement;
        private VisualElement _bindedView;
        private VisualElement _unbindedView;
        private Button _retryButton;

        public Action<bool> EnableChanged { get; set; }
        public Action RetryBind { get; set; }

        private BallPlayer.DisplacementDebugData _debugData;
        private bool _hasData;

        public DisplacementOverlayManager(VisualElement root) : base(root)
        {
            BindElements();
        }

        public void SetTitle(string title)
        {
            if (_titleLabel != null)
            {
                _titleLabel.text = title;
            }
        }

        public void SetInfo(string info)
        {
            if (_infoLabel != null)
            {
                _infoLabel.text = info;
            }
        }

        public void SetLegend(string legend)
        {
            if (_legendLabel != null)
            {
                _legendLabel.text = legend;
            }
        }

        public void SetEnabledState(bool enabled)
        {
            if (_enableToggle != null)
            {
                _enableToggle.SetValueWithoutNotify(enabled);
                _enableToggle.MarkDirtyRepaint();
            }
        }

        public void SetToggleInteractable(bool enabled)
        {
            _enableToggle?.SetEnabled(enabled);
        }

        public void SetBindedViewVisible(bool isBinded)
        {
            if (_bindedView != null)
            {
                _bindedView.style.display = isBinded ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_unbindedView != null)
            {
                _unbindedView.style.display = isBinded ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        public void ClearData(string reason)
        {
            _hasData = false;
            SetInfo(reason);
            SetLegend(string.Empty);
            _graphElement?.MarkDirtyRepaint();
        }

        public void UpdateDebugData(BallPlayer player)
        {
            if (player == null || !player.TryGetDebugData(out var data) || !data.HasData)
            {
                _hasData = false;
                SetInfo("暂无可用数据");
                SetLegend(string.Empty);
                _graphElement?.MarkDirtyRepaint();
                return;
            }

            _debugData = data;
            _hasData = true;
            SetInfo($"Δt比例: {data.DeltaTimeRatio:F3} | 修正T: {data.CorrectionT:F3}");
            SetLegend($"标准位移: {data.StandardDisplacement:F1}({data.StandardDisplacement.magnitude:F3})\n" +
                      $"实际位移: {data.ActualDisplacement:F1}({data.ActualDisplacement.magnitude:F3})\n" +
                      $"标准方向分量: {data.ActualOnStandardVector:F1}({data.ActualOnStandardVector.magnitude:F3})\n" +
                      $"正交分量: {data.ActualOnOrthogonalVector:F1}({data.ActualOnOrthogonalVector.magnitude:F3})");
            _graphElement?.MarkDirtyRepaint();
        }

        private void BindElements()
        {
            if (Root == null)
            {
                Debug.LogWarning("[DisplacementOverlay] Root is null.");
                return;
            }

            _titleLabel = Root.Q<Label>("displacement-title");
            _enableToggle = Root.Q<Toggle>("displacement-enable");
            _infoLabel = Root.Q<Label>("displacement-info");
            _legendLabel = Root.Q<Label>("displacement-legend");
            _graphElement = Root.Q<VisualElement>("displacement-graph");
            _bindedView = Root.Q<VisualElement>("binded-view");
            _unbindedView = Root.Q<VisualElement>("unbinded-view");
            _retryButton = Root.Q<Button>("displacement-retry");

            if (_enableToggle != null)
            {
                _enableToggle.RegisterValueChangedCallback(evt => EnableChanged?.Invoke(evt.newValue));
            }

            if (_retryButton != null)
            {
                _retryButton.clicked += OnRetryClicked;
            }

            if (_graphElement != null)
            {
                _graphElement.generateVisualContent += DrawGraph;
            }
        }

        private void DrawGraph(MeshGenerationContext ctx)
        {
            if (_graphElement == null)
                return;

            Rect rect = _graphElement.contentRect;
            if (rect.width <= 0 || rect.height <= 0)
                return;

            Vector2 center = rect.center;
            var painter = ctx.painter2D;
            painter.lineWidth = 1.2f;
            painter.strokeColor = new Color(0.45f, 0.45f, 0.45f, 1f);

            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, center.y));
            painter.LineTo(new Vector2(rect.xMax, center.y));
            painter.MoveTo(new Vector2(center.x, rect.yMin));
            painter.LineTo(new Vector2(center.x, rect.yMax));
            painter.Stroke();

            if (!_hasData)
                return;

            float actualOnStandard = _debugData.ActualOnStandardVector.magnitude;
            float actualOnOrthogonal = _debugData.ActualOnOrthogonalVector.magnitude;
            float standardMagnitude = _debugData.StandardDisplacement.magnitude;
            float maxAbs = Mathf.Max(Mathf.Abs(actualOnStandard), Mathf.Abs(actualOnOrthogonal), standardMagnitude, 0.001f);
            float scale = Mathf.Min(rect.width, rect.height) * 0.4f / maxAbs;

            Vector2 standardEnd = center + new Vector2(standardMagnitude * scale, 0f);
            Vector2 actualEnd = center + new Vector2(actualOnStandard * scale, -actualOnOrthogonal * scale);
            Vector2 actualOrthogonalEnd = center + new Vector2(0, -actualOnOrthogonal * scale);

            painter.strokeColor = new Color(0.2f, 0.8f, 0.4f, 1f);
            painter.BeginPath();
            painter.MoveTo(center);
            painter.LineTo(standardEnd);
            painter.Stroke();

            painter.strokeColor = new Color(0.9f, 0.3f, 0.85f, 1f);
            painter.BeginPath();
            painter.MoveTo(center);
            painter.LineTo(actualEnd);
            painter.Stroke();
            
            painter.strokeColor = Color.red;
            painter.BeginPath();
            painter.MoveTo(center);
            painter.LineTo(actualOrthogonalEnd);
            painter.Stroke();
            painter.strokeColor = Color.blue;
            painter.BeginPath();
            painter.MoveTo(actualOrthogonalEnd);
            painter.LineTo(actualEnd);
            painter.Stroke();
        }

        private void OnRetryClicked()
        {
            RetryBind?.Invoke();
        }
    }
}
