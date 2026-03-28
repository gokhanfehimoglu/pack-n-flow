#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PackNFlow
{
    public static class RuntimeHelpers
    {
        public static void PurgeChildrenImmediate(this Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }
    }
}
#endif
