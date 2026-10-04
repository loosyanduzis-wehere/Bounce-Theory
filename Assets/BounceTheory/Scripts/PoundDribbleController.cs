using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BounceTheory
{
    public enum BallHand { Left, Right }
    public enum BallLogicalPhase { Controlled, Descending, FloorContact, Returning }
    public enum BounceMotionMode { Normal, Compressed, Unreachable }

    /// <summary>Rhythm input is authoritative; one judged follow-up may wait for Controlled.</summary>
    public sealed class PoundDribbleController : MonoBehaviour
    {
        [Header("Ball Ownership")]
        [SerializeField] private BallHand startingHand = BallHand.Left;
        [SerializeField] private Transform leftHandAnchor;
        [SerializeField] private Transform rightHandAnchor;

        [Header("Pound Dribble Readability")]
        [SerializeField, Min(0.1f)] private float visualBounceDuration = 0.72f;
        [SerializeField, Min(0.1f)] private float handHeight = 1.56f;
        [SerializeField] private float floorHeight = 0.47f;
        [SerializeField, Min(0f)] private float floorContactHold = 0.08f;

        [Header("Adaptive Contact Motion")]
        [SerializeField, Min(0.1f)] private float minimumFastBounceHeight = 1.0f;
        [SerializeField, Min(0.5f)] private float maximumDescentSpeed = 8.0f;
        [SerializeField, Range(0f, 1f)] private float trajectoryCompressionAmount = 0.65f;
        [SerializeField, Min(0.01f)] private float minimumReadableContactApproachTime = 0.08f;

        [Header("Rhythm Source")]
        [SerializeField] private RhythmClock rhythmClock;

        [Header("Timing Motion Multipliers")]
        [SerializeField, Min(0.1f)] private float perfectDurationMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float goodDurationMultiplier = 1.03f;
        [SerializeField, Min(0.1f)] private float earlyDurationMultiplier = 0.84f;
        [SerializeField, Min(0.1f)] private float lateDurationMultiplier = 1.18f;
        [SerializeField, Min(0.1f)] private float brokenDurationMultiplier = 1.38f;
        [SerializeField, Min(0f)] private float earlyLateralDrift = 0.045f;
        [SerializeField, Min(0f)] private float lateLateralDrift = 0.035f;
        [SerializeField, Min(0f)] private float brokenLateralDrift = 0.18f;
        [SerializeField, Min(0f)] private float brokenContactHoldMultiplier = 1.8f;

        [Header("Prototype Floor Sound")]
        [SerializeField] private AudioSource bounceAudioSource;
        [SerializeField] private AudioClip bounceSound;
        [SerializeField, Range(0f, 1f)] private float bounceVolume = 0.7f;
        [SerializeField, Range(0.5f, 1.5f)] private float perfectPitch = 1f;
        [SerializeField, Range(0.5f, 1.5f)] private float goodPitch = 0.98f;
        [SerializeField, Range(0.5f, 1.5f)] private float earlyPitch = 1.08f;
        [SerializeField, Range(0.5f, 1.5f)] private float latePitch = 0.91f;
        [SerializeField, Range(0.5f, 1.5f)] private float brokenPitch = 0.78f;

        [Header("Temporary Debug Feedback")]
        [SerializeField] private GameObject floorContactIndicator;
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logStateChanges = true;

        private BallHand currentHand;
        private BallLogicalPhase logicalPhase = BallLogicalPhase.Controlled;
        private float visualPhaseElapsed;
        private double simulatedElapsedCursor;
        private Vector3 handPosition;
        private Vector3 floorPosition;
        private float activeBounceDuration;
        private float activeContactHold;
        private float activeReturnDuration;
        private float activeLateralDrift;
        private float activeArcLift;
        private int completedDribbleCount;
        private int timingJudgmentCount;
        private TimingJudgment lastTimingJudgment = TimingJudgment.None;
        private ContactTimingPlan activeContactPlan;
        private double activeMotionStartDsp;
        private Vector3 activeMotionStartPosition;
        private BounceMotionMode motionMode = BounceMotionMode.Normal;
        private bool activeUsesSimulatedTime;
        private bool hasPendingInput;
        private ContactTimingPlan pendingContactPlan;
        private bool pendingUsesSimulatedTime;
        private double lastTargetFloorContactDsp;
        private double lastActualFloorContactDsp;
        private double lastContactErrorMilliseconds;
        private string lastInputDecision = "Waiting for input.";

        public BallHand StartingHand => startingHand;
        public BallHand CurrentHand => currentHand;
        public BallLogicalPhase LogicalPhase => logicalPhase;
        public bool IsDribbling => logicalPhase != BallLogicalPhase.Controlled;
        public bool HasPendingInput => hasPendingInput;
        public TimingJudgment PendingTimingJudgment => pendingContactPlan.Judgment;
        public double PendingInputDspTimestamp => pendingContactPlan.InputDspTimestamp;
        public double PendingInputElapsedSeconds => pendingContactPlan.InputElapsedSeconds;
        public double PendingTargetContactDspTimestamp => pendingContactPlan.TargetContactDspTimestamp;
        public string LastInputDecision => lastInputDecision;
        public TimingJudgment ActiveActionJudgment => activeContactPlan.Judgment;
        public ContactTimingPlan ActiveContactPlan => activeContactPlan;
        public BounceMotionMode MotionMode => motionMode;
        public double LastTargetFloorContactDsp => lastTargetFloorContactDsp;
        public double LastActualFloorContactDsp => lastActualFloorContactDsp;
        public double LastContactErrorMilliseconds => lastContactErrorMilliseconds;
        public int CompletedDribbleCount => completedDribbleCount;
        public int TimingJudgmentCount => timingJudgmentCount;
        public float BounceDuration => visualBounceDuration;
        public float HandHeight => handHeight;
        public float FloorHeight => floorHeight;
        public RhythmClock RhythmClock => rhythmClock;
        public TimingJudgment LastTimingJudgment => lastTimingJudgment;
        public float ActiveBounceDuration => activeBounceDuration;
        public float BounceVolume => bounceVolume;
        public float MinimumFastBounceHeight => minimumFastBounceHeight;
        public float MaximumDescentSpeed => maximumDescentSpeed;
        public float TrajectoryCompressionAmount => trajectoryCompressionAmount;
        public float MinimumReadableContactApproachTime => minimumReadableContactApproachTime;
        public float ReferenceDescentDurationSeconds => ReferenceDescentDuration;

        public event Action<BallHand> DribbleStarted;
        public event Action<BallHand> FloorContactReached;
        public event Action<BallHand> BallReturned;
        public event Action<TimingJudgment> TimingJudged;

        private void Awake() => ResetToStartingHand();

        private void OnEnable()
        {
            SetContactIndicator(false);
            if (!IsDribbling) SnapToCurrentHand();
        }

        private void OnValidate()
        {
            visualBounceDuration = Mathf.Max(0.1f, visualBounceDuration);
            handHeight = Mathf.Max(0.1f, handHeight);
            floorContactHold = Mathf.Clamp(floorContactHold, 0f, visualBounceDuration * 0.8f);
            minimumFastBounceHeight = Mathf.Clamp(minimumFastBounceHeight, floorHeight + .1f, handHeight);
            maximumDescentSpeed = Mathf.Max(.5f, maximumDescentSpeed);
            minimumReadableContactApproachTime = Mathf.Max(.01f, minimumReadableContactApproachTime);
            SyncAnchorHeights();
            if (!Application.isPlaying)
            {
                currentHand = startingHand;
                logicalPhase = BallLogicalPhase.Controlled;
                ClearPendingInput();
                SetContactIndicator(false);
                SnapToCurrentHand();
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
                ProcessInput(keyboard.wKey.wasPressedThisFrame, keyboard.upArrowKey.wasPressedThisFrame);
            Tick(Time.deltaTime);
        }

        public bool ProcessInput(bool leftPressed, bool rightPressed)
        {
            if (!TryIdentifyActiveInput(leftPressed, rightPressed)) return false;
            double dsp = AudioSettings.dspTime;
            double elapsed = rhythmClock != null ? rhythmClock.ElapsedSecondsAtDspTime(dsp) : 0;
            ContactTimingPlan plan = CreateContactPlan(elapsed, dsp, false);
            return DecideInput(plan, false);
        }

        public bool ProcessInputAtRhythmTime(bool leftPressed, bool rightPressed, double elapsed)
        {
            if (!TryIdentifyActiveInput(leftPressed, rightPressed)) return false;
            double dsp = rhythmClock != null ? rhythmClock.StartDspTime + elapsed : elapsed;
            simulatedElapsedCursor = Math.Max(simulatedElapsedCursor, elapsed);
            ContactTimingPlan plan = CreateContactPlan(elapsed, dsp, true);
            return DecideInput(plan, true);
        }

        public bool TryStartPoundDribble() => currentHand == BallHand.Left ? ProcessInput(true, false) : ProcessInput(false, true);

        public void Tick(float deltaTime)
        {
            if (!IsDribbling || deltaTime <= 0f) return;
            if (activeUsesSimulatedTime) simulatedElapsedCursor += deltaTime;
            visualPhaseElapsed += deltaTime;

            if (logicalPhase == BallLogicalPhase.Descending)
            {
                double nowDsp = CurrentDspTime();
                double duration = Math.Max(.001, activeContactPlan.TargetContactDspTimestamp - activeMotionStartDsp);
                float t = Mathf.Clamp01((float)((nowDsp - activeMotionStartDsp) / duration));
                float shaped = motionMode == BounceMotionMode.Compressed
                    ? Mathf.Lerp(Smooth(t), t, trajectoryCompressionAmount)
                    : Smooth(t);
                Vector3 descentPosition = Vector3.Lerp(activeMotionStartPosition, floorPosition, shaped);
                if (activeArcLift > 0f) descentPosition.y += Mathf.Sin(shaped * Mathf.PI) * activeArcLift;
                transform.position = ApplyMotionShape(descentPosition, t);
                if (nowDsp >= activeContactPlan.TargetContactDspTimestamp) EnterFloorContact(nowDsp);
                return;
            }
            if (logicalPhase == BallLogicalPhase.FloorContact)
            {
                transform.position = floorPosition;
                if (visualPhaseElapsed >= activeContactHold)
                {
                    logicalPhase = BallLogicalPhase.Returning;
                    visualPhaseElapsed = 0f;
                    SetContactIndicator(false);
                    if (TryExecutePending()) return;
                }
                return;
            }

            float returnT = Mathf.Clamp01(visualPhaseElapsed / Mathf.Max(.01f, activeReturnDuration));
            transform.position = ApplyMotionShape(Vector3.Lerp(floorPosition, handPosition, Smooth(returnT)), 1f - returnT);
            if (hasPendingInput && transform.position.y >= minimumFastBounceHeight && TryExecutePending()) return;
            if (returnT >= 1f) CompleteDribble();
        }

        public void SetStartingHand(BallHand hand)
        {
            if (IsDribbling) return;
            startingHand = hand;
            currentHand = hand;
            SnapToCurrentHand();
        }

        public void ResetToStartingHand()
        {
            logicalPhase = BallLogicalPhase.Controlled;
            visualPhaseElapsed = 0f;
            simulatedElapsedCursor = 0;
            currentHand = startingHand;
            activeContactPlan = default;
            motionMode = BounceMotionMode.Normal;
            ClearPendingInput();
            lastInputDecision = "Reset to starting hand; waiting for input.";
            SetContactIndicator(false);
            SyncAnchorHeights();
            SnapToCurrentHand();
        }

        public void Configure(Transform leftAnchor, Transform rightAnchor, GameObject contactIndicator)
        {
            leftHandAnchor = leftAnchor;
            rightHandAnchor = rightAnchor;
            floorContactIndicator = contactIndicator;
            ResetToStartingHand();
        }

        public void ConfigureRhythm(RhythmClock clock, AudioSource source, AudioClip clip)
        {
            rhythmClock = clock;
            bounceAudioSource = source;
            bounceSound = clip;
        }

        public void ConfigureReadability(float newHandHeight)
        {
            handHeight = Mathf.Max(0.1f, newHandHeight);
            SyncAnchorHeights();
            if (!IsDribbling) SnapToCurrentHand();
        }

        public void ConfigureAdaptiveMotion(float fastHeight, float maxSpeed, float compression, float minimumApproach)
        {
            minimumFastBounceHeight = Mathf.Clamp(fastHeight, floorHeight + .1f, handHeight);
            maximumDescentSpeed = Mathf.Max(.5f, maxSpeed);
            trajectoryCompressionAmount = Mathf.Clamp01(compression);
            minimumReadableContactApproachTime = Mathf.Max(.01f, minimumApproach);
        }

        private bool TryIdentifyActiveInput(bool leftPressed, bool rightPressed)
        {
            bool active = currentHand == BallHand.Left ? leftPressed : rightPressed;
            bool inactive = currentHand == BallHand.Left ? rightPressed : leftPressed;
            if (active) return true;
            if (inactive)
            {
                lastInputDecision = $"Rejected: inactive {Opposite(currentHand)}-hand input while {currentHand} owns the ball.";
                Log(lastInputDecision);
            }
            return false;
        }

        private ContactTimingPlan CreateContactPlan(double elapsed, double dsp, bool simulated)
        {
            if (rhythmClock == null)
            {
                TimingJudgment fallback = PerfectFallback();
                double reference = ReferenceDescentDuration;
                return new ContactTimingPlan(fallback, RhythmicIntervalKind.None, elapsed, dsp, 0,
                    elapsed + reference, dsp + reference, elapsed + reference);
            }
            bool preferHalfBeat = logicalPhase == BallLogicalPhase.Returning;
            double referenceOverride = (logicalPhase == BallLogicalPhase.Descending || logicalPhase == BallLogicalPhase.FloorContact) &&
                                       activeContactPlan.TargetContactBeat > 0
                ? activeContactPlan.TargetContactBeat
                : double.NaN;
            return rhythmClock.PlanNextContactAtElapsedTime(elapsed, dsp, ReferenceDescentDuration,
                minimumReadableContactApproachTime, preferHalfBeat, referenceOverride);
        }

        private bool DecideInput(ContactTimingPlan plan, bool simulated)
        {
            TimingJudgment judgment = plan.Judgment;
            lastTimingJudgment = judgment;
            timingJudgmentCount++;
            TimingJudged?.Invoke(judgment);

            if (logicalPhase == BallLogicalPhase.Controlled || logicalPhase == BallLogicalPhase.Returning)
            {
                BallLogicalPhase sourcePhase = logicalPhase;
                if (!BeginPoundDribble(plan, simulated)) return false;
                lastInputDecision = $"Accepted from {sourcePhase}; contact scheduled on the global grid.";
                return true;
            }
            if (!hasPendingInput)
            {
                hasPendingInput = true;
                pendingContactPlan = plan;
                pendingUsesSimulatedTime = simulated;
                lastInputDecision = $"Accepted and queued during {logicalPhase}; contact target preserved.";
                Log($"follow-up queued — target DSP {plan.TargetContactDspTimestamp:0.000000}, {RhythmicIntervalCatalog.Label(plan.Interval)}");
                return true;
            }

            lastInputDecision = $"Rejected: pending input slot already occupied during {logicalPhase}.";
            Log(lastInputDecision);
            return false;
        }

        private bool BeginPoundDribble(ContactTimingPlan plan, bool simulated)
        {
            Transform anchor = GetCurrentAnchor();
            if (anchor == null)
            {
                lastInputDecision = "Rejected: active hand anchor is missing.";
                motionMode = BounceMotionMode.Unreachable;
                return false;
            }

            double nowDsp = simulated
                ? (rhythmClock != null ? rhythmClock.StartDspTime + simulatedElapsedCursor : simulatedElapsedCursor)
                : AudioSettings.dspTime;
            double remaining = plan.TargetContactDspTimestamp - nowDsp;
            Vector3 startPosition = logicalPhase == BallLogicalPhase.Controlled ? anchor.position : transform.position;
            Vector3 targetPosition = new Vector3(startPosition.x, floorHeight, startPosition.z);
            activeArcLift = startPosition.y < minimumFastBounceHeight ? minimumFastBounceHeight - startPosition.y : 0f;
            float requiredDistance = Vector3.Distance(startPosition, targetPosition) + activeArcLift * 2f;
            if (remaining < minimumReadableContactApproachTime || requiredDistance / Math.Max(.001, remaining) > maximumDescentSpeed)
            {
                motionMode = BounceMotionMode.Unreachable;
                lastInputDecision = remaining <= 0
                    ? "Rejected: selected floor-contact target has already passed."
                    : $"Rejected: target is physically unreachable ({remaining * 1000.0:0.0} ms approach).";
                Log(lastInputDecision);
                return false;
            }

            activeContactPlan = plan;
            activeUsesSimulatedTime = simulated;
            motionMode = remaining < ReferenceDescentDuration * .9 || startPosition.y < minimumFastBounceHeight
                ? BounceMotionMode.Compressed
                : BounceMotionMode.Normal;
            ApplyTimingProfile(plan.Judgment.Result);
            handPosition = anchor.position;
            floorPosition = targetPosition;
            activeMotionStartPosition = startPosition;
            activeMotionStartDsp = nowDsp;
            activeBounceDuration = (float)remaining + activeContactHold + activeReturnDuration;
            visualPhaseElapsed = 0f;
            logicalPhase = BallLogicalPhase.Descending;
            SetContactIndicator(false);
            DribbleStarted?.Invoke(currentHand);
            Log($"dribble began — input DSP {plan.InputDspTimestamp:0.000000}, target contact DSP {plan.TargetContactDspTimestamp:0.000000}, mode {motionMode}");
            return true;
        }

        private void ApplyTimingProfile(TimingResult result)
        {
            float multiplier = perfectDurationMultiplier;
            activeContactHold = floorContactHold;
            activeLateralDrift = 0f;
            switch (result)
            {
                case TimingResult.Good: multiplier = goodDurationMultiplier; break;
                case TimingResult.Early: multiplier = earlyDurationMultiplier; activeContactHold *= .7f; activeLateralDrift = earlyLateralDrift; break;
                case TimingResult.Late: multiplier = lateDurationMultiplier; activeContactHold *= 1.25f; activeLateralDrift = lateLateralDrift; break;
                case TimingResult.BrokenRhythm: multiplier = brokenDurationMultiplier; activeContactHold *= brokenContactHoldMultiplier; activeLateralDrift = brokenLateralDrift; break;
            }
            activeReturnDuration = Mathf.Max(.05f, ReferenceDescentDuration * multiplier);
            activeContactHold = Mathf.Min(activeContactHold, visualBounceDuration * .8f);
        }

        private Vector3 ApplyMotionShape(Vector3 position, float distanceFromHand)
        {
            if (activeLateralDrift <= 0f) return position;
            float sign = currentHand == BallHand.Left ? -1f : 1f;
            position.x += Mathf.Sin(Mathf.Clamp01(distanceFromHand) * Mathf.PI) * activeLateralDrift * sign;
            return position;
        }

        private void EnterFloorContact(double actualDsp)
        {
            logicalPhase = BallLogicalPhase.FloorContact;
            visualPhaseElapsed = 0f;
            transform.position = floorPosition;
            lastTargetFloorContactDsp = activeContactPlan.TargetContactDspTimestamp;
            lastActualFloorContactDsp = actualDsp;
            lastContactErrorMilliseconds = (actualDsp - lastTargetFloorContactDsp) * 1000.0;
            completedDribbleCount++;
            PositionContactIndicator();
            SetContactIndicator(true);
            PlayBounceSound();
            RegisterFloorEvent(actualDsp);
            FloorContactReached?.Invoke(currentHand);
            Log($"floor contact — target {lastTargetFloorContactDsp:0.000000}, actual {actualDsp:0.000000}, error {lastContactErrorMilliseconds:+0.0;-0.0;0.0} ms");
        }

        private void RegisterFloorEvent(double actualDsp)
        {
            if (rhythmClock == null) return;
            rhythmClock.RegisterBallEventAtElapsedTime(rhythmClock.ElapsedSecondsAtDspTime(actualDsp));
        }

        private void PlayBounceSound()
        {
            if (bounceAudioSource == null || bounceSound == null) return;
            bounceAudioSource.pitch = PitchFor(activeContactPlan.Judgment.Result);
            bounceAudioSource.PlayOneShot(bounceSound, bounceVolume);
        }

        private float PitchFor(TimingResult result)
        {
            switch (result)
            {
                case TimingResult.Good: return goodPitch;
                case TimingResult.Early: return earlyPitch;
                case TimingResult.Late: return latePitch;
                case TimingResult.BrokenRhythm: return brokenPitch;
                default: return perfectPitch;
            }
        }

        private void CompleteDribble()
        {
            logicalPhase = BallLogicalPhase.Controlled;
            visualPhaseElapsed = 0f;
            SetContactIndicator(false);
            SnapToCurrentHand();
            BallReturned?.Invoke(currentHand);
            Log("ball returned to Controlled phase");

            TryExecutePending();
        }

        private bool TryExecutePending()
        {
            if (!hasPendingInput) return false;
            ContactTimingPlan plan = pendingContactPlan;
            bool simulated = pendingUsesSimulatedTime;
            ClearPendingInput();
            if (BeginPoundDribble(plan, simulated))
            {
                lastInputDecision = "Queued input executing without rejudgment; original contact target preserved.";
                return true;
            }
            return false;
        }

        private void ClearPendingInput()
        {
            hasPendingInput = false;
            pendingContactPlan = default;
            pendingUsesSimulatedTime = false;
        }

        private float ReferenceDescentDuration => Mathf.Max(.05f, (visualBounceDuration - floorContactHold) * .5f);
        private double CurrentDspTime() => activeUsesSimulatedTime && rhythmClock != null
            ? rhythmClock.StartDspTime + simulatedElapsedCursor
            : AudioSettings.dspTime;

        private Transform GetCurrentAnchor() => currentHand == BallHand.Left ? leftHandAnchor : rightHandAnchor;
        private static BallHand Opposite(BallHand hand) => hand == BallHand.Left ? BallHand.Right : BallHand.Left;
        private void SnapToCurrentHand() { Transform anchor = GetCurrentAnchor(); if (anchor != null) transform.position = anchor.position; }
        private void SyncAnchorHeights() { SetAnchorHeight(leftHandAnchor); SetAnchorHeight(rightHandAnchor); }
        private void SetAnchorHeight(Transform anchor) { if (anchor == null) return; Vector3 p = anchor.localPosition; p.y = handHeight; anchor.localPosition = p; }
        private void PositionContactIndicator() { if (floorContactIndicator == null) return; Vector3 p = floorPosition; p.y = .16f; floorContactIndicator.transform.position = p; }
        private void SetContactIndicator(bool visible) { if (floorContactIndicator != null && floorContactIndicator.activeSelf != visible) floorContactIndicator.SetActive(visible); }
        private static float Smooth(float t) => t * t * (3f - 2f * t);
        private static TimingJudgment PerfectFallback() => new TimingJudgment(TimingResult.Perfect, TimingDirection.OnBeat, 0, 0, 0);
        private void Log(string message) { if (logStateChanges) Debug.Log($"[Bounce Theory] {currentHand} hand: {message}.", this); }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;
            const float width = 540f;
            GUI.Box(new Rect(18, 18, width, 414), "Bounce Theory — Target Contact / Rhythm Debug");
            GUI.Label(new Rect(32, 45, width - 24, 22), $"Current hand: {currentHand}   Logical ball phase: {logicalPhase}");
            if (rhythmClock != null)
            {
                GUI.Label(new Rect(32, 67, width - 24, 22), $"BPM: {rhythmClock.Bpm:0.##}   Global beat: {rhythmClock.CurrentBeatPosition:0.000}   Phase: {rhythmClock.CurrentBeatPhase:0.000}");
                GUI.Label(new Rect(32, 89, width - 24, 22), $"Subdivision: {rhythmClock.CurrentSubdivision + 1}/{rhythmClock.SubdivisionsPerBeat}");
                string previous = rhythmClock.HasPreviousBallEvent ? $"{rhythmClock.PreviousBallEventElapsedSeconds:0.000}s (grid {rhythmClock.PreviousBallEventAlignedBeat:0.##})" : "None — first input uses global beat";
                GUI.Label(new Rect(32, 111, width - 24, 22), $"Previous floor event: {previous}");
            }
            string result = lastTimingJudgment.IsValid ? lastTimingJudgment.Result.ToString() : "Waiting";
            string error = lastTimingJudgment.IsValid ? $"{lastTimingJudgment.ErrorMilliseconds:+0.0;-0.0;0.0} ms" : "—";
            string direction = lastTimingJudgment.IsValid ? lastTimingJudgment.Direction.ToString() : "—";
            string interval = lastTimingJudgment.HasRhythmicInterval ? RhythmicIntervalCatalog.Label(lastTimingJudgment.Interval) : "Global beat / first input";
            GUI.Label(new Rect(32, 133, width - 24, 22), $"Selected interval: {interval}   Motion mode: {motionMode}");
            GUI.Label(new Rect(32, 155, width - 24, 22), $"Input timing: {result}   Error: {error}   Direction: {direction}");
            GUI.Label(new Rect(32, 177, width - 24, 22), $"Keypress DSP: {(activeContactPlan.InputDspTimestamp > 0 ? activeContactPlan.InputDspTimestamp.ToString("0.000000") : "—")}");
            GUI.Label(new Rect(32, 199, width - 24, 22), $"Target floor-contact DSP: {(activeContactPlan.TargetContactDspTimestamp > 0 ? activeContactPlan.TargetContactDspTimestamp.ToString("0.000000") : "—")}");
            GUI.Label(new Rect(32, 221, width - 24, 22), $"Actual floor-contact DSP: {(lastActualFloorContactDsp > 0 ? lastActualFloorContactDsp.ToString("0.000000") : "—")}");
            GUI.Label(new Rect(32, 243, width - 24, 22), $"Contact error: {(lastActualFloorContactDsp > 0 ? lastContactErrorMilliseconds.ToString("+0.0;-0.0;0.0") + " ms" : "—")}");
            GUI.Label(new Rect(32, 265, width - 24, 22), $"Pending input: {(hasPendingInput ? "Yes" : "No")}");
            GUI.Label(new Rect(32, 287, width - 24, 22), $"Pending keypress DSP: {(hasPendingInput ? pendingContactPlan.InputDspTimestamp.ToString("0.000000") : "—")}");
            GUI.Label(new Rect(32, 309, width - 24, 22), $"Pending target DSP: {(hasPendingInput ? pendingContactPlan.TargetContactDspTimestamp.ToString("0.000000") : "—")}   Interval: {(hasPendingInput ? RhythmicIntervalCatalog.Label(pendingContactPlan.Interval) : "—")}");
            GUI.Label(new Rect(32, 331, width - 24, 42), $"Last decision/failure: {lastInputDecision}");
            GUI.Label(new Rect(32, 375, width - 24, 22), currentHand == BallHand.Left ? "Active input: W" : "Active input: Up Arrow");
            GUI.Label(new Rect(32, 397, width - 24, 22), "Impact sound fires only at measured FloorContact.");
        }
    }
}
