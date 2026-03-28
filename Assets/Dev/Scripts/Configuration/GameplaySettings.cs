using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace PackNFlow
{
    [CreateAssetMenu(fileName = "NewGameplaySettings.asset", menuName = "Configuration/Gameplay Settings", order = 1)]
    public class GameplaySettings : ScriptableObject
    {
        private static GameplaySettings s_Runtime;

        public static GameplaySettings Instance
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
            // Debug.Assert(s_Runtime == null, "GameplaySettings already initialized!");
            s_Runtime = this;
        }

        [Serializable]
        public struct ConveyorConfig
        {
            public float gapBetweenCarriages;
            public float carriageToConveyorDuration;
            public float carriageFromConveyorDuration;
            public float carriageFollowSpeed;
        }

        [Serializable]
        public struct UnitConfig
        {
            public float unitGridZOffsetByCellSize;
            public float pullDuration;
            public float unitBoardDuration;
            public float unitBoardScale;
            public float unitBoardArcHeight;
            public float minDeployInterval;
        }

        [FoldoutGroup("CONVEYOR", Expanded = false)]
        [TitleGroup("CONVEYOR/Settings", alignment: TitleAlignments.Centered)]
        public ConveyorConfig conveyor;

        [FoldoutGroup("UNITS", Expanded = false)]
        [TitleGroup("UNITS/Settings", alignment: TitleAlignments.Centered)]
        public UnitConfig units;

#if UNITY_EDITOR
        private static GameplaySettings s_Editor;

        private static GameplaySettings EditorOnlyInstance
        {
            get
            {
                if (s_Editor == null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:GameplaySettings");
                    if (guids.Length > 0)
                    {
                        s_Editor = AssetDatabase.LoadAssetAtPath<GameplaySettings>(
                            AssetDatabase.GUIDToAssetPath(guids[0]));
                    }
                }
                return s_Editor;
            }
        }
#endif
    }
}
