using System.Collections.Generic;
using DG.Tweening;
using Dreamteck.Splines;
using TMPro;
using UnityEngine;

namespace PackNFlow
{
    public class ConveyorSystem : MonoBehaviour
    {
        [SerializeField] private ConveyorCarriage carriagePrefab;
        [SerializeField] private TextMeshPro capacityLabel;
        [SerializeField] private Transform carriageAnchor;
        [SerializeField] private SplineComputer spline;

        public Bounds? Bounds =>
            spline.TryGetComponent(out Renderer r) ? r.bounds : null;

        private readonly Queue<ConveyorCarriage> _standby = new();
        private readonly List<ConveyorCarriage> _fleet = new();
        private Sequence _warningSeq;
        private int _readyCount;

        public void Prepare(int carriageCount)
        {
            _standby.Clear();
            _readyCount = 0;
            PurgeFleet();
            SpawnCarriages(carriageCount);
            ReturnAllToStandby();
            ArrangeBay();
        }

        public bool TryGetReadyCarriage(out ConveyorCarriage carriage)
        {
            carriage = null;
            if (_standby.Count == 0) return false;
            if (!_standby.Peek().IsReady) return false;

            carriage = _standby.Peek();
            return true;
        }

        public void DispatchCarriage(ConveyorCarriage carriage)
        {
            _standby.Dequeue();
            _readyCount--;
            carriage.LaunchOntoSpline();
            ArrangeBay();
        }

        public void ReleaseCarriageForUnit(Unit unit)
        {
            foreach (var c in _fleet)
            {
                if (c.Occupant == unit)
                {
                    c.OnUnitDepleted();
                    return;
                }
            }
        }

        public void PlayCapacityWarning()
        {
            if (_warningSeq != null && _warningSeq.IsActive() && _warningSeq.IsPlaying())
                return;

            if (_warningSeq == null || !_warningSeq.IsActive())
            {
                _warningSeq = DOTween.Sequence();
                _warningSeq.Append(capacityLabel.DOColor(Color.crimson, 0.2f));
                _warningSeq.Append(capacityLabel.DOColor(Color.white, 0.2f));
                _warningSeq.Append(capacityLabel.DOColor(Color.crimson, 0.2f));
                _warningSeq.Append(capacityLabel.DOColor(Color.white, 0.2f));
                _warningSeq.OnKill(() => capacityLabel.color = Color.white);
            }
            else
            {
                _warningSeq.Play();
            }
        }

        private void SpawnCarriages(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var carriage = Instantiate(carriagePrefab, carriageAnchor);
                carriage.LinkToSpline(spline);
                carriage.PathFinished += OnCarriagePathDone;
                carriage.ReturnRequested += OnCarriageReturnRequested;
                _fleet.Add(carriage);
            }
        }

        private void ReturnAllToStandby()
        {
            foreach (var c in _fleet)
            {
                if (!c.IsReady || !c.HasCompletedPath)
                    c.SnapToEnd();

                _standby.Enqueue(c);
                _readyCount++;
            }
        }

        private void ArrangeBay()
        {
            int idx = 0;
            foreach (var c in _standby)
            {
                if (c.IsReady || c.HasCompletedPath)
                {
                    c.PositionInBay(idx);
                    idx++;
                }
            }

            capacityLabel.SetText($"{_standby.Count}/{_fleet.Count}");
        }

        private void OnCarriagePathDone(ConveyorCarriage c)
        {
            _standby.Enqueue(c);
            _readyCount++;
            ArrangeBay();
        }

        private void OnCarriageReturnRequested(ConveyorCarriage c)
        {
            _standby.Enqueue(c);
            _readyCount++;
            ArrangeBay();
        }

        private void PurgeFleet()
        {
            for (var i = _fleet.Count - 1; i >= 0; i--)
            {
                var c = _fleet[i];
                c.ReleaseUnit();
                DestroyImmediate(c.gameObject);
            }
            _fleet.Clear();
        }
    }
}
