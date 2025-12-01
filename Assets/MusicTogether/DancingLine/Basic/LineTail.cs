using MusicTogether.DancingLine.Core;
using UnityEngine;

namespace MusicTogether.DancingLine.Basic
{
    public class LineTail : BaseLineTail
    {
        private readonly Transform _lineContainer;
        private readonly GameObject _lineTailObject;
        
        public LineTail(Vector3 directionVector, Transform lineContainer)
        {
            GameObject newTailObject = Resources.Load<GameObject>("DancingLine/Basic/LineTail");
            _lineContainer = lineContainer;
            _lineTailObject = Object.Instantiate(newTailObject, Vector3.zero, Quaternion.LookRotation(directionVector));
            _lineTailObject.transform.SetParent(_lineContainer, false);
        }

        public override void SetBeginPosition(Vector3 position)
        {
            base.SetBeginPosition(position);
            _lineTailObject.transform.position = position;
        }
        
        public override void SetActive(bool active)
        {
            if (active != _lineTailObject.activeSelf)
                _lineTailObject.SetActive(active);
        }
        public override void UpdateTail(float deltaTime)
        {
            _lineTailObject.transform.position = BeginPosition + DirectionVector * deltaTime/2;
            _lineTailObject.transform.localScale = new Vector3(1, 1, deltaTime*DirectionVector.magnitude + 1f);
        }
        public override void DeleteTail()
        {
            Object.Destroy(_lineTailObject);
        }
    }
}