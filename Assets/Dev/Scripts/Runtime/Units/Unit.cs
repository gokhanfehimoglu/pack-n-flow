using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using PackNFlow.AudioSystem;

namespace PackNFlow
{
    public enum UnitPhase
    {
        Queued,
        Active,
        Stored,
        Depleted
    }

    public class Unit : MonoBehaviour
    {
        [SerializeField] private TextMeshPro capacityText;
        [SerializeField] private UnitAppearance appearance;
        [SerializeField] private Animator animator;

        public UnitAppearance Appearance => appearance;
        public UnitScanData ScanData { get; private set; }
        public UnitData Data { get; private set; }

        public UnitPhase Phase { get; private set; } = UnitPhase.Queued;
        public bool IsTethered { get; private set; }
        public bool IsHidden { get; private set; }
        public bool IsScanReady => Phase == UnitPhase.Active && _isOnSpline && ScanData != null;
        public bool IsDeployable { get; private set; }
        public bool IsCapacityDepleted => Phase == UnitPhase.Depleted;
        public bool IsInConveyor => Phase == UnitPhase.Active;
        public bool IsInFront => _isFrontUnit;
        public TetherLine Tether { get; private set; }
        public Unit TetheredPartner { get; private set; }

        private int _remainingCapacity;
        public int RemainingCapacity => _remainingCapacity;
        private Transform _originalParent;
        private Vector3 _baseScale;
        private Tweener _shakeTween;
        private Tweener _recoilTween;
        private Sequence _boardSeq;
        private bool _isFrontUnit;
        private bool _isOnSpline;
        private Vector3 _activeScale;
        
        private static readonly int Jump = Animator.StringToHash("Jump");

        public event Action<Unit> OnDeployRequest;
        public event Action<Unit, ConveyorCarriage> OnBoardingCompleted;
        public event Action<Unit> OnPathFinished;
        public event Action<Unit> OnCapacityDepleted;

        public void Configure(UnitData data)
        {
            Data = data;
            _remainingCapacity = data.PullCapacity;
            ScanData = new UnitScanData();
            appearance.ApplyConfig(data);

            if (data.IsConcealed)
                Conceal();

            _originalParent = transform.parent;
            _baseScale = appearance.transform.localScale;
            _activeScale = _baseScale;
        }

        private void OnDestroy()
        {
            OnDeployRequest = null;
            OnBoardingCompleted = null;
            OnPathFinished = null;
            OnCapacityDepleted = null;
            _boardSeq?.Kill();
            DOTween.Kill(transform);
            DOTween.Kill(appearance.transform);
        }

        public void SetTethered(Unit partner, TetherLine tether)
        {
            IsTethered = true;
            TetheredPartner = partner;
            Tether = tether;
        }

        public void SeverTether()
        {
            IsTethered = false;
            TetheredPartner = null;
            Tether = null;
        }

        private void Conceal()
        {
            IsHidden = true;
            appearance.ApplyConcealed();
        }

        private void OnMouseDown()
        {
            OnDeployRequest?.Invoke(this);
        }

        public void BoardTheCarriage(ConveyorCarriage carriage)
        {
            Phase = UnitPhase.Active;
            IsDeployable = false;
            _boardSeq?.Kill();
            DOTween.Kill(transform);

            _activeScale = _baseScale * GameplaySettings.Instance.units.unitBoardScale;

            transform.SetParent(carriage.transform);

            var cfg = GameplaySettings.Instance.units;
            _boardSeq = DOTween.Sequence();
            _boardSeq.Insert(0f, transform.DOLocalJump(Vector3.zero, cfg.unitBoardArcHeight, 1, cfg.unitBoardDuration));
            _boardSeq.Insert(0f, appearance.transform.DOScale(Vector3.one * cfg.unitBoardScale, cfg.unitBoardDuration));
            // _boardSeq.Insert(0f, transform.DOLocalRotate(new Vector3(0, -90, 0), cfg.unitBoardDuration));
            _boardSeq.OnComplete(() =>
            {
                _boardSeq = null;
                _isOnSpline = true;
                OnBoardingCompleted?.Invoke(this, carriage);
            });
            _boardSeq.SetLink(gameObject);
            _boardSeq.SetEase(Ease.Linear);
            _boardSeq.OnStart(() =>
            {
                animator.SetTrigger(Jump);
            });

            carriage.AssignUnit(this);
            carriage.PathFinished += HandleCarriagePathEnd;
        }

        public void MoveToRackSlot(SlotPiece slot)
        {
            _boardSeq?.Kill();
            DOTween.Kill(transform);

            var targetParent = slot.transform;
            var targetPos = targetParent.position;
            var startPos = transform.position;

            transform.SetParent(_originalParent);

            var cfg = GameplaySettings.Instance.units;
            _boardSeq = DOTween.Sequence();
            _boardSeq.Append(transform.DOJump(targetPos, cfg.unitBoardArcHeight, 1, cfg.unitBoardDuration));
            _boardSeq.Join(appearance.transform.DOScale(Vector3.one * cfg.unitBoardScale, cfg.unitBoardDuration));
            _boardSeq.OnStart(() =>
            {
                animator.SetTrigger(Jump);
            });
            _boardSeq.OnComplete(() =>
            {
                _boardSeq = null;
                transform.SetParent(targetParent);
                transform.localPosition = Vector3.zero;
                transform.localEulerAngles = Vector3.zero;
                _activeScale = _baseScale;
                appearance.transform.localScale = _baseScale;
            });
            _boardSeq.SetLink(gameObject);
            _boardSeq.SetEase(Ease.Linear);
        }

        public void PromoteToFront()
        {
            _isFrontUnit = true;
            if (IsHidden)
                appearance.Reveal(ResolveColor());
        }

        public void LeaveConveyor()
        {
            Phase = UnitPhase.Stored;
            _isOnSpline = false;
            _activeScale = _baseScale;
        }

        public void TriggerPull(PixelBlock target, Edge side)
        {
            AudioManager.Instance.PlayAudio(AudioName.Pull);
            ScanData.SealLine(side, target.Data.Coordinates);
            _remainingCapacity--;

            if (_remainingCapacity <= 0)
            {
                Phase = UnitPhase.Depleted;
                _isFrontUnit = false;
                target.PullToward(appearance.Muzzle, GameplaySettings.Instance.units.pullDuration, OnLastBlockArrived);
                return;
            }

            float duration = GameplaySettings.Instance.units.pullDuration;
            target.PullToward(appearance.Muzzle, duration, () =>
            {
                PlayPullRecoil();
                appearance.UpdateCapacityText(_remainingCapacity);
            });
        }

        private void OnLastBlockArrived()
        {
            PlayPullRecoil();
            appearance.UpdateCapacityText(0);
            transform.SetParent(_originalParent);
            OnCapacityDepleted?.Invoke(this);
            PlayDepleteAnimation();
        }

        private void PlayDepleteAnimation()
        {
            _boardSeq?.Kill();
            DOTween.Kill(transform);

            var duration = GameplaySettings.Instance.units.depleteDuration * 2f;

            _boardSeq = DOTween.Sequence();
            _boardSeq.Append(transform.DOLocalRotate(new Vector3(0, 360, 0), duration, RotateMode.FastBeyond360));
            _boardSeq.Join(transform.DOLocalMoveY(1.5f, duration).SetEase(Ease.OutQuad));
            _boardSeq.Join(transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack));
            _boardSeq.OnComplete(() => Destroy(gameObject));
            _boardSeq.SetLink(gameObject);
        }

        private void PlayPullRecoil()
        {
            _shakeTween?.Kill();
            _recoilTween?.Kill();
            appearance.transform.localScale = _activeScale;
            _recoilTween = appearance.transform
                .DOShakeScale(0.1f, Vector3.one * 0.1f, 0, 90f, true, ShakeRandomnessMode.Harmonic)
                .OnComplete(() => { appearance.transform.localScale = _activeScale; })
                .OnKill(() => { appearance.transform.localScale = _activeScale; })
                .SetLink(gameObject);
        }

        public void PlayShakeOffAnim()
        {
            _recoilTween?.Kill();
            _shakeTween?.Kill();
            _shakeTween = transform
                .DOShakeRotation(0.3f, Vector3.up * 15, 20, 90f, true, ShakeRandomnessMode.Harmonic)
                .OnComplete(() => { transform.localRotation = Quaternion.identity; })
                .OnKill(() => { transform.localRotation = Quaternion.identity; })
                .SetLink(gameObject);
        }

        private void BecomeDepleted()
        {
            Phase = UnitPhase.Depleted;
            _isFrontUnit = false;
            gameObject.SetActive(false);
            OnCapacityDepleted?.Invoke(this);
        }

        public void SetDeployable(bool canDeploy)
        {
            if (IsDeployable == canDeploy) return;
            IsDeployable = canDeploy;

            if (IsHidden) return;

            appearance.ApplyDeployableVisual(canDeploy);
        }

        public void ResetParent() => transform.SetParent(_originalParent);

        private void HandleCarriagePathEnd(ConveyorCarriage carriage)
        {
            carriage.PathFinished -= HandleCarriagePathEnd;
            ScanData.Reset();
            OnPathFinished?.Invoke(this);
        }

        private Color32 ResolveColor()
        {
            var levelData = LevelDirector.Instance != null
                ? LevelDirector.Instance.ActiveLevelData
                : null;
            return levelData != null
                ? levelData.GetColorById(Data.ColorId)
                : new Color32(255, 255, 255, 255);
        }

        public void ApplyEditorVisuals(LevelData levelData)
        {
            appearance.ApplyEditorVisuals(levelData, Data);
            if (Data.IsConcealed)
                Conceal();
        }
    }
}
