using Sirenix.OdinInspector;
using UnityEngine;

namespace PackNFlow
{
    public class LevelCreator : MonoBehaviour
    {
        [SerializeField] private LevelData _levelData;
        [SerializeField] private Unit _unitPrefab;
        [SerializeField] private PixelBlock _blockPrefab;
        [SerializeField] private Transform _unitParent;
        [SerializeField] private Transform _blockParent;
        [SerializeField] private ConveyorSystem _conveyorPrefab;

        public LevelData LevelData => _levelData;
    }
}
