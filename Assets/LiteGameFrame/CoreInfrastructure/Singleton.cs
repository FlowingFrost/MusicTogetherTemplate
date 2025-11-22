using UnityEngine;

namespace LiteGameFrame.CoreInfrastructure
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if(_instance == null)
                    EnsureExists();
                return _instance;
            }
        }
        public virtual bool Persistent => true;
        
        //Tool Functions
        private static void EnsureExists()
        {
            //场景里已有（域重载后静态丢但物体还在）
            T[] found = FindObjectsOfType<T>();
            if (found.Length > 0)
            {
                _instance = found[0];
                if (found.Length > 1)
                {
                    for (int i = 1; i < found.Length; i++)
                    {
                        _instance.OnDuplicate(found[i]);
                    }
                    Debug.LogWarning($"Multiple {typeof(T).Name} in scene!");
                }
            }
            else
            {
                new GameObject(typeof(T).Name).AddComponent<T>().OnRegister();
            }
        }

        protected virtual void OnRegister()
        {
            _instance = this as T;
            ServiceLocator.Register<T>(_instance);
            if (Persistent)
                DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDuplicate(Singleton<T> duplicate)
        {
            Destroy(duplicate.gameObject);
        }
        
        // 7. 编辑器退出 Play 模式时清引用（防止域重载后野指针）
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => _instance = null;
#endif
        
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                OnRegister();
            }
            else if (_instance != this)
            {
                OnDuplicate(this);
            }
        }
    }
}