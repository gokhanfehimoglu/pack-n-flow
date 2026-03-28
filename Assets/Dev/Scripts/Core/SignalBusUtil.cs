using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PackNFlow.Core
{
    public static class SignalBusUtil
    {
        private static IReadOnlyList<System.Type> _busTypes;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                PurgeAllBuses();
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            _busTypes = WarmUpBuses(AssemblyTypeScanner.GetTypes(typeof(ISignal)));
        }

        private static List<System.Type> WarmUpBuses(List<System.Type> signalTypes)
        {
            List<System.Type> busTypes = new List<System.Type>();
            var busTypeDef = typeof(SignalBus<>);

            for (int i = 0; i < signalTypes.Count; i++)
            {
                var busType = busTypeDef.MakeGenericType(signalTypes[i]);
                busTypes.Add(busType);
            }

            return busTypes;
        }

        private static void PurgeAllBuses()
        {
            for (int i = 0; i < _busTypes.Count; i++)
            {
                var busType = _busTypes[i];
                var resetMethod = busType.GetMethod("Reset", BindingFlags.Static | BindingFlags.NonPublic);
                resetMethod?.Invoke(null, null);
            }
        }
    }
}
