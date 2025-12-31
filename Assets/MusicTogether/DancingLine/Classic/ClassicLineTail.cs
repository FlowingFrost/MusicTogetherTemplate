using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingLine.Classic
{
    /// <summary>
    /// Classic线尾 - 负责序列化配置
    /// 算法逻辑由BaseLineTail提供
    /// </summary>
    public class ClassicLineTail : SerializedMonoBehaviour, ILineTail
    {
        // 序列化字段
        [SerializeField] protected GameObject _lineTailObject;
        //
        protected Vector3 _beginPosition;
        protected Vector3 _directionVector;
        

        // 覆写抽象属性，提供序列化数据
        public void Init(IDirection direction)
        {
            _directionVector = direction.DirectionVector;
        }

        public void SetBeginPosition(Vector3 beginPosition)
        {
            _beginPosition = beginPosition;
            transform.localPosition = beginPosition;
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public Vector3 UpdateTail(float deltaTime)
        {
            Vector3 _endPosition = _beginPosition + _directionVector*deltaTime;
            transform.localPosition = (_beginPosition + _endPosition) / 2;
            transform.localScale = new Vector3(1, 1, _directionVector.magnitude*deltaTime + 1);
            transform.localRotation = Quaternion.LookRotation(_directionVector);
            return _endPosition;
        }

        public void DeleteTail()
        {
            Destroy(gameObject);
        }
    }
}
