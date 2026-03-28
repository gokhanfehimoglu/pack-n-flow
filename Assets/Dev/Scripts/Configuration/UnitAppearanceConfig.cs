using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace PackNFlow
{
    [CreateAssetMenu(fileName = "UnitAppearanceConfig", menuName = "Configuration/Unit Appearance Config")]
    public class UnitAppearanceConfig : ScriptableObject
    {
        private static UnitAppearanceConfig s_Runtime;

        public static UnitAppearanceConfig Instance
        {
            get
            {
#if UNITY_EDITOR
                return Application.isPlaying ? s_Runtime : EditorOnlyInstance;
#else
                return s_Runtime;
#endif
            }
        }

        public void Initialize()
        {
            // Debug.Assert(s_Runtime == null, "UnitAppearanceConfig already initialized!");
            s_Runtime = this;
        }

        [TitleGroup("Body Materials")]
        public Material defaultMaterial;

        [TitleGroup("Body Materials")]
        public Material concealed;

        [TitleGroup("Rope Materials")]
        public Material defaultRopeMaterial;

        [TitleGroup("Rope Materials")]
        public Material concealedRope;

#if UNITY_EDITOR
        private static UnitAppearanceConfig s_Editor;

        private static UnitAppearanceConfig EditorOnlyInstance
        {
            get
            {
                if (s_Editor == null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:UnitAppearanceConfig");
                    if (guids.Length > 0)
                    {
                        s_Editor = AssetDatabase.LoadAssetAtPath<UnitAppearanceConfig>(
                            AssetDatabase.GUIDToAssetPath(guids[0]));
                    }
                }
                return s_Editor;
            }
        }
#endif
    }
}
