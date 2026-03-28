using UnityEngine;

namespace PackNFlow
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T s_Instance;

        public static T Instance
        {
            get
            {
                if (s_Instance != null) return s_Instance;

                s_Instance = FindFirstObjectByType<T>();
                if (s_Instance == null)
                {
                    var go = new GameObject { name = typeof(T).Name };
                    s_Instance = go.AddComponent<T>();
                }

                return s_Instance;
            }
        }

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(Application.isPlaying ? gameObject : null);
                if (!Application.isPlaying) DestroyImmediate(gameObject);
                return;
            }

            s_Instance = this as T;
            OnAfterAwake();
        }

        protected virtual void OnAfterAwake() { }
    }
}
