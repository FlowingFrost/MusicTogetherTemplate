using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    /// <summary>
    /// 基本线尾抽象类
    /// </summary>
    public class BaseLineTail : MonoBehaviour, ILineTail
    {
        protected Vector3 BeginPosition;
        protected Vector3 DirectionVector;
        [SerializeField]protected GameObject _lineTailObject;
        
        
        protected BaseLineTail()
        {
        }


        public virtual void Init(Vector3 directionVector)
        {
            DirectionVector = directionVector;
            _lineTailObject.gameObject.SetActive(false);
            _lineTailObject.transform.localRotation = Quaternion.LookRotation(DirectionVector);
        }
        public virtual void SetBeginPosition(Vector3 position)
        {
            BeginPosition = position;
        }

        public virtual void SetActive(bool active)
        {
            if (active != _lineTailObject.activeSelf)
                _lineTailObject.SetActive(active);
        }

        public virtual void UpdateTail(float deltaTime)
        {
            _lineTailObject.transform.position = BeginPosition + DirectionVector * deltaTime/2;
            _lineTailObject.transform.localScale = new Vector3(1, 1, deltaTime*DirectionVector.magnitude + 1f);
            //Debug.Log($"BeginPosition: {BeginPosition}, DirectionVector: {DirectionVector}, deltaTime: {deltaTime}");
        }

        public virtual void DeleteTail()
        {
            Object.Destroy(_lineTailObject);
        }
    }
}
