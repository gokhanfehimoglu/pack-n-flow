using System;
using DG.Tweening;
using Dreamteck.Splines;
using UnityEngine;

namespace PackNFlow
{
    public class ConveyorCarriage : MonoBehaviour, ICarriage
    {
        [SerializeField] private GameObject boardVisual;

        private SplineFollower _follower;
        private float _travelSpeed;
        private Sequence _transitionSeq;
        private Tweener _moveTween;

        public bool IsReady { get; private set; } = true;
        public bool HasCompletedPath { get; private set; } = true;
        public Unit Occupant { get; private set; }

        public event Action<ConveyorCarriage> PathFinished;
        public event Action<ConveyorCarriage> ReturnRequested;

        public void LinkToSpline(SplineComputer spline)
        {
            _follower = gameObject.AddComponent<SplineFollower>();
            _follower.follow = false;
            _follower.spline = spline;
            _follower.followSpeed = 0f;
            _travelSpeed = GameplaySettings.Instance.conveyor.carriageFollowSpeed;
            _follower.onEndReached += HandleSplineEnd;
        }

        public void SnapToEnd()
        {
            _follower.SetPercent(1.0f);
            InternalReset();
            HasCompletedPath = true;
        }

        public void AssignUnit(Unit unit) => Occupant = unit;

        public void ReleaseUnit()
        {
            if (Occupant != null) Occupant.LeaveConveyor();
            Occupant = null;
        }

        public void OnUnitDepleted()
        {
            InternalReset();
            ReturnRequested?.Invoke(this);
        }

        public void PositionInBay(int slotIndex)
        {
            var cfg = GameplaySettings.Instance.conveyor;
            var slotPos = new Vector3(-(slotIndex * cfg.gapBetweenCarriages), 0f, 0f);
            var slotRot = new Vector3(0, 90, 0);

            transform.localPosition = slotPos;
            transform.localEulerAngles = slotRot;
            boardVisual.transform.localPosition = Vector3.up * 0.75f;
            boardVisual.transform.localRotation = Quaternion.identity;
            IsReady = true;
        }

        public void BeginMovement() => _follower.followSpeed = _travelSpeed;

        internal void LaunchOntoSpline()
        {
            IsReady = false;
            HasCompletedPath = false;

            var splineStart = _follower.EvaluatePosition(0.0f);
            var duration = GameplaySettings.Instance.conveyor.carriageToConveyorDuration;
            KillActiveTweens();

            _transitionSeq = BuildTransitionToConveyor(duration);
            _moveTween = transform.DOMove(splineStart, duration).SetEase(Ease.Linear);
            _moveTween.OnComplete(() =>
            {
                _follower.SetPercent(0.0);
                _follower.follow = true;
            });
        }

        private Sequence BuildTransitionToConveyor(float duration)
        {
            var seq = DOTween.Sequence();
            seq.Insert(0f, transform.DOLocalRotate(new Vector3(0, 90, 0), duration));
            seq.Insert(0, boardVisual.transform.DOLocalRotate(
                new Vector3(-90, 0, 0), duration * 0.75f, RotateMode.LocalAxisAdd));
            seq.Insert(0, boardVisual.transform.DOLocalMove(Vector3.zero, duration));
            return seq;
        }

        private void HandleSplineEnd(double _)
        {
            if (IsReady || HasCompletedPath) return;
            InternalReset();
            PathFinished?.Invoke(this);
        }

        private void InternalReset()
        {
            ReleaseUnit();
            HasCompletedPath = true;
            _follower.follow = false;
            _follower.followSpeed = 0f;
        }

        private void KillActiveTweens()
        {
            _moveTween?.Kill(false);
            _transitionSeq?.Kill(false);
        }

        private void OnDestroy()
        {
            KillActiveTweens();
            PathFinished = null;
            ReturnRequested = null;
        }
    }
}
