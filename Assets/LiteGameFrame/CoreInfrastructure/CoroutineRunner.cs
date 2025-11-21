using System.Collections;
using UnityEngine;

namespace LiteGameFrame.CoreInfrastructure
{
    public class CoroutineRunner : Singleton<CoroutineRunner>
    {
        // 隐藏父类方法，保持调用习惯
        public new Coroutine StartCoroutine(IEnumerator routine)
        {
            return base.StartCoroutine(routine);
        }

        public Coroutine StartCoroutine(IEnumerator routine, float delay)
        {
            return base.StartCoroutine(routine);
        }

        public new void StopCoroutine(Coroutine coroutine)
        {
            if (coroutine != null) base.StopCoroutine(coroutine);
        }

        public new void StopCoroutine(IEnumerator routine)
        {
            base.StopCoroutine(routine);
        }

        public void StopAll()
        {
            base.StopAllCoroutines();
        }
    }
}