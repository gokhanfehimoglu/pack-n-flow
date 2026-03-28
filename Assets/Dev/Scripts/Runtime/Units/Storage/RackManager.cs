using System.Collections.Generic;
using UnityEngine;

namespace PackNFlow
{
    public class RackManager : MonoBehaviour
    {
        [SerializeField] private SlotPiece rackSlotPrefab;
        
        public IReadOnlyList<SlotPiece> Racks { get; private set; }
        private SlotPiece[] _racks;
        
        public void Prepare(GridLayout unitGrid)
        {
            PurgeRacks();
            
            var levelData = LevelDirector.Instance.ActiveLevelData;
            var positions = GridMath.ComputeRackSlotPositions(levelData, unitGrid);
            _racks = new SlotPiece[positions.Length];

            for (var i = 0; i < positions.Length; i++)
            {
                var piece = Instantiate(rackSlotPrefab, transform);
                piece.transform.position = positions[i];
                // piece.transform.localScale = Vector3.one;
                _racks[i] = piece;
            }

            Racks = _racks;
        }
        
        private void PurgeRacks()
        {
            if (_racks == null) return;
            for (var i = _racks.Length - 1; i >= 0; i--)
                DestroyImmediate(_racks[i]);
        }
    }
}