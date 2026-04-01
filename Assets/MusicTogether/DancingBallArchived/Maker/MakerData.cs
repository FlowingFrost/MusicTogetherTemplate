using System;
using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBallArchived.Maker
{
    public enum BlockPlacementStyle{Classic,Circle,Free}//Chebyshev
    public enum ClassicPlacementType{Forward,BackWard,Left,Right,Up45,Up90,Down45,Down90}
    
    [Serializable]
    public struct BlockPlacementData
    {
        [OnValueChanged("CalculateDirection")]
        public Vector3 eulerAngles;
        public Vector3 forwardDirection,upDirection;
        public BlockPlacementStyle style;
        [ShowIf("@style == BlockPlacementStyle.Classic")]
        public ClassicPlacementType classicPlacementType;
        [BoxGroup("Anchor", ShowLabel = true,CenterLabel = true)]
        [HorizontalGroup("Anchor/Content")] 
        [ShowInInspector, HideLabel, ReadOnly, PreviewField(ObjectFieldAlignment.Left, Height = 50)]
        private Texture2D _anchorTexture;

        [VerticalGroup("Anchor/Content/Coordinates")][PropertySpace]
        [PropertyRange(-0.5f, 0.5f), OnValueChanged("UpdateCoordinateSystemTexture")]
        public float anchorX;
        [VerticalGroup("Anchor/Content/Coordinates")]
        [PropertyRange(-0.5f, 0.5f), OnValueChanged("UpdateCoordinateSystemTexture")]
        public float anchorY;
        
        public Vector3Direction tileDirection;
        public bool doubleFacing;
        [ShowIf("@doubleFacing==true")]public Vector3Direction tileDirection2;
        
        public static BlockPlacementData Default => new BlockPlacementData
        {
            forwardDirection = Vector3.forward,
            style = BlockPlacementStyle.Classic,
            anchorX = 0f,
            anchorY = 0f,
            // 其他字段为默认值
        };
        private void CalculateDirection()
        {
            Quaternion rotation = Quaternion.Euler(eulerAngles);
            forwardDirection = CorrectForwardDirection(rotation * Vector3.forward);
            upDirection = (rotation * Vector3.up).normalized;
        }

        public void ModifyEulerAngles(Vector3 eulerAngles)
        {
            this.eulerAngles = CorrectEulerAngles(eulerAngles);
            CalculateDirection();
        }

        public void CheckClassicType(BlockPlacementData lastPlacementData)
        {
            Vector3 lastUpDirection = lastPlacementData.upDirection;
            Vector3 lastForwardDirection = lastPlacementData.forwardDirection;
            float uuDot = Vector3.Cross(lastUpDirection, upDirection).sqrMagnitude;
            if (uuDot < Mathf.Epsilon)
            {
                float ffDot = Vector3.Dot(lastForwardDirection, forwardDirection);
                if (ffDot <= -1 + Mathf.Epsilon)
                {
                    classicPlacementType = ClassicPlacementType.BackWard;
                    return;
                }
                if (ffDot >= 1 - Mathf.Epsilon)
                {
                    classicPlacementType = ClassicPlacementType.Forward;
                    return;
                }
                Vector3 lastRight = Vector3.Cross(lastUpDirection, lastForwardDirection).normalized;
                float rfDot = Vector3.Dot(forwardDirection, lastRight);
                classicPlacementType = rfDot > 0 ? ClassicPlacementType.Right : ClassicPlacementType.Left;
                return;
            }
            float ufDot = Vector3.Dot(lastUpDirection, forwardDirection);
            if (ufDot <= -1 + Mathf.Epsilon)
            {
                classicPlacementType = ClassicPlacementType.Down90;
                return;
            }
            if (ufDot >= 1 - Mathf.Epsilon)
            {
                classicPlacementType = ClassicPlacementType.Up90;
                return;
            }

            if (Vector3.Cross((lastForwardDirection + lastUpDirection).normalized, forwardDirection).sqrMagnitude <
                Mathf.Epsilon)
            {
                classicPlacementType = ClassicPlacementType.Up45;
                return;
            }
            if (Vector3.Cross((lastForwardDirection + lastUpDirection).normalized, upDirection).sqrMagnitude <
                Mathf.Epsilon)
            {
                classicPlacementType = ClassicPlacementType.Down45;
            }
        }

        private Vector3 CorrectEulerAngles(Vector3 eulerAngles)
        {
            switch (style)
            {
                case BlockPlacementStyle.Classic:
                    return (eulerAngles / 45).Round()*45;
                default:
                    return eulerAngles;
            }
        }
        private Vector3 CorrectForwardDirection(Vector3 direction)
        {
            Vector3 finalDirection = direction.normalized;
            switch (style)
            {
                case BlockPlacementStyle.Classic:
                    finalDirection = (finalDirection).Round();
                    break;
                //case BlockPlacementStyle.Chebyshev:
                    //finalDirection = finalDirection.RoundMax();
                    //float maxComponent = Mathf.Max(
                    //    Mathf.Abs(finalDirection.x),
                    //    Mathf.Abs(finalDirection.y),
                    //    Mathf.Abs(finalDirection.z)
                    //);
                    //finalDirection/=maxComponent;备选方案
                    //break;
                case BlockPlacementStyle.Circle:
                    break;
            }
            return finalDirection;
        }
        
        private void UpdateCoordinateSystemTexture()
        {
            int size = 100;
            _anchorTexture = new Texture2D(size, size);

            // 透明背景
            Color transparent = new Color(0.219f, 0.219f, 0.219f, 1.0f);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    _anchorTexture.SetPixel(i, j, transparent);
                }
            }

            // 绘制坐标轴
            Color axisColor = Color.white;
            // X轴（从左到右）
            for (int i = 0; i < size; i++)
            {
                _anchorTexture.SetPixel(i, size / 2, axisColor);
            }

            // Y轴（从下到上）
            for (int j = 0; j < size; j++)
            {
                _anchorTexture.SetPixel(size / 2, j, axisColor);
            }

            // 绘制锚点
            int x = (int)((anchorX + 0.5f) * size);
            int y = (int)((anchorY + 0.5f) * size);
            Color anchorColor = new Color(0.8f, 0.6f, 0.0f, 1.0f);

            // 绘制一个3x3的锚点
            int r = 5;
            for (int i = -r; i <= r; i++)
            {
                for (int j = -r; j <= r; j++)
                {
                    int px = Mathf.Clamp(x + i, 0, size - 1);
                    int py = Mathf.Clamp(y + j, 0, size - 1);
                    _anchorTexture.SetPixel(px, py, anchorColor);
                }
            }
            _anchorTexture.Apply();
        }
    }
}

