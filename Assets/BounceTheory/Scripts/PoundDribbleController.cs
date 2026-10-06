using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BounceTheory
{
    public enum BallHand { Left, Right }
    public enum BallLogicalPhase { Controlled, Descending, FloorContact, Returning }
    public enum BounceMotionMode { Normal, Compressed, Unreachable }
    public enum DribbleAction { Pound, Crossover, Hesitation, BehindTheBack }
    public enum PlayerStance { Low, Medium, High }
    public enum PossessionState { Active, Ended }
    public enum PossessionEndReason { None, ContinuationWindowExpired, Manual }
    public enum BallControlQuality { Secure, Recovering, Exposed }
    public enum FollowUpRelation { FirstAction, Repeat, SameHandVariation, Transfer, CounterTransfer }

    /// <summary>Rhythm input is authoritative; one judged follow-up may wait for Controlled.</summary>
    public sealed class PoundDribbleController : MonoBehaviour
    {
        [Header("Ball Ownership")]
        [SerializeField] private BallHand startingHand = BallHand.Left;
        [SerializeField] private Transform leftHandAnchor;
        [SerializeField] private Transform rightHandAnchor;

        [Header("Basic Crossover")]
        [SerializeField, Range(0.25f, 0.75f)] private float crossoverContactLateralProgress = 0.55f;

        [Header("Basic Hesitation")]
        [SerializeField, Range(0.15f, 0.6f)] private float hesitationHoldFraction = 0.36f;
        [SerializeField, Min(0f)] private float hesitationLift = 0.18f;

        [Header("Basic Behind The Back")]
        [SerializeField, Range(0.25f, 0.75f)] private float behindBackContactLateralProgress = 0.55f;
        [SerializeField, Min(0.1f)] private float behindBackDepthOffset = 0.65f;
        [SerializeField, Min(0f)] private float behindBackWrapDepth = 0.3f;

        [Header("Pound Dribble Readability")]
        [SerializeField, Min(0.1f)] private float visualBounceDuration = 0.72f;
        [SerializeField, Min(0.1f)] private float handHeight = 1.56f;
        [SerializeField] private float floorHeight = 0.47f;
        [SerializeField, Min(0f)] private float floorContactHold = 0.08f;

        [Header("Stance Pound Profile")]
        [SerializeField, Min(0f)] private float lowPoundMidpointDrop = 0.24f;
        [SerializeField, Min(0f)] private float highPoundArcLift = 0.38f;

        [Header("Stance Crossover Profile")]
        [SerializeField, Min(0f)] private float lowCrossoverMidpointDrop = 0.16f;
        [SerializeField, Min(0f)] private float highCrossoverArcLift = 0.28f;

        [Header("Stance Hesitation Profile")]
        [SerializeField, Range(0f, 0.2f)] private float lowHesitationHoldReduction = 0.08f;
        [SerializeField, Min(0f)] private float lowHesitationLiftReduction = 0.10f;
        [SerializeField, Range(0f, 0.2f)] private float highHesitationHoldAddition = 0.10f;
        [SerializeField, Min(0f)] private float highHesitationLiftAddition = 0.24f;

        [Header("Stance Behind-The-Back Profile")]
        [SerializeField, Min(0f)] private float lowBehindBackDepthReduction = 0.16f;
        [SerializeField, Min(0f)] private float lowBehindBackWrapReduction = 0.10f;
        [SerializeField, Min(0f)] private float lowBehindBackMidpointDrop = 0.12f;
        [SerializeField, Min(0f)] private float highBehindBackDepthAddition = 0.06f;
        [SerializeField, Min(0f)] private float highBehindBackWrapAddition = 0.04f;
        [SerializeField, Min(0f)] private float highBehindBackArcLift = 0.08f;

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

        [Header("Stance Controls")]
        [SerializeField, Min(0f)] private float stanceModifierGraceSeconds = 0.18f;

        [Header("Temporary Debug Feedback")]
        [SerializeField] private GameObject floorContactIndicator;
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logStateChanges = true;

        private BallHand currentHand;
        private PlayerStance currentStance = PlayerStance.Medium;
        private PlayerStance nextExtremeFromMedium = PlayerStance.Low;
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
        private PlayerStance activeActionStance = PlayerStance.Medium;
        private DribbleAction activeAction = DribbleAction.Pound;
        private BallControlQuality activeControlQuality = BallControlQuality.Secure;
        private FollowUpRelation activeFollowUpRelation = FollowUpRelation.FirstAction;
        private BallHand activeSourceHand;
        private BallHand activeTargetHand;
        private int completedDribbleCount;
        private int completedCrossoverCount;
        private int completedBehindBackCount;
        private int timingJudgmentCount;
        private TimingJudgment lastTimingJudgment = TimingJudgment.None;
        private ContactTimingPlan activeContactPlan;
        private double activeMotionStartDsp;
        private Vector3 activeMotionStartPosition;
        private BounceMotionMode motionMode = BounceMotionMode.Normal;
        private bool activeUsesSimulatedTime;
        private bool hasPendingInput;
        private ContactTimingPlan pendingContactPlan;
        private DribbleAction pendingAction = DribbleAction.Pound;
        private PlayerStance pendingActionStance = PlayerStance.Medium;
        private FollowUpRelation pendingFollowUpRelation = FollowUpRelation.FirstAction;
        private bool pendingUsesSimulatedTime;
        private double lastTargetFloorContactDsp;
        private double lastActualFloorContactDsp;
        private double lastContactErrorMilliseconds;
        private string lastInputDecision = "Waiting for input.";
        private PossessionState possessionState = PossessionState.Active;
        private PossessionEndReason possessionEndReason = PossessionEndReason.None;
        private bool hasPreviousAction;
        private DribbleAction previousAction = DribbleAction.Pound;
        private PlayerStance previousActionStance = PlayerStance.Medium;
        private TimingJudgment previousActionJudgment = TimingJudgment.None;
        private BallHand previousResolvedHand;
        private BallControlQuality previousControlQuality = BallControlQuality.Secure;
        private int sequenceActionCount;
        private bool spaceModifierConsumed;
        private bool pendingPlainSpace;
        private float pendingPlainSpaceDeadline;

        public BallHand StartingHand => startingHand;
        public BallHand CurrentHand => currentHand;
        public PlayerStance CurrentStance => currentStance;
        public PlayerStance NextExtremeFromMedium => nextExtremeFromMedium;
        public BallLogicalPhase LogicalPhase => logicalPhase;
        public bool IsDribbling => logicalPhase != BallLogicalPhase.Controlled;
        public bool HasPendingInput => hasPendingInput;
        public DribbleAction ActiveAction => activeAction;
        public DribbleAction PendingAction => pendingAction;
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
        public int CompletedCrossoverCount => completedCrossoverCount;
        public int CompletedBehindBackCount => completedBehindBackCount;
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
        public float HesitationHoldFraction => hesitationHoldFraction;
        public float HesitationLift => hesitationLift;
        public float BehindBackDepthOffset => behindBackDepthOffset;
        public float StanceModifierGraceSeconds => stanceModifierGraceSeconds;
        public bool HasPendingPlainSpace => pendingPlainSpace;
        public PlayerStance ActiveActionStance => activeActionStance;
        public PlayerStance PendingActionStance => pendingActionStance;
        public PlayerStance ActivePoundStance => activeActionStance;
        public PlayerStance PendingPoundStance => pendingActionStance;
        public float LowPoundMidpointDrop => lowPoundMidpointDrop;
        public float HighPoundArcLift => highPoundArcLift;
        public float CrossoverContactLateralProgress => crossoverContactLateralProgress;
        public float LowCrossoverMidpointDrop => lowCrossoverMidpointDrop;
        public float HighCrossoverArcLift => highCrossoverArcLift;
        public float LowHesitationHoldReduction => lowHesitationHoldReduction;
        public float LowHesitationLiftReduction => lowHesitationLiftReduction;
        public float HighHesitationHoldAddition => highHesitationHoldAddition;
        public float HighHesitationLiftAddition => highHesitationLiftAddition;
        public float BehindBackContactLateralProgress => behindBackContactLateralProgress;
        public float BehindBackWrapDepth => behindBackWrapDepth;
        public float LowBehindBackDepthReduction => lowBehindBackDepthReduction;
        public float LowBehindBackWrapReduction => lowBehindBackWrapReduction;
        public float LowBehindBackMidpointDrop => lowBehindBackMidpointDrop;
        public float HighBehindBackDepthAddition => highBehindBackDepthAddition;
        public float HighBehindBackWrapAddition => highBehindBackWrapAddition;
        public float HighBehindBackArcLift => highBehindBackArcLift;
        public PossessionState CurrentPossessionState => possessionState;
        public PossessionEndReason CurrentPossessionEndReason => possessionEndReason;
        public bool HasPreviousAction => hasPreviousAction;
        public DribbleAction PreviousAction => previousAction;
        public PlayerStance PreviousActionStance => previousActionStance;
        public TimingJudgment PreviousActionJudgment => previousActionJudgment;
        public BallHand PreviousResolvedHand => previousResolvedHand;
        public BallControlQuality PreviousControlQuality => previousControlQuality;
        public BallControlQuality ActiveControlQuality => activeControlQuality;
        public FollowUpRelation ActiveFollowUpRelation => activeFollowUpRelation;
        public FollowUpRelation PendingFollowUpRelation => pendingFollowUpRelation;
        public int SequenceActionCount => sequenceActionCount;

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
            crossoverContactLateralProgress = Mathf.Clamp(crossoverContactLateralProgress, .25f, .75f);
            hesitationHoldFraction = Mathf.Clamp(hesitationHoldFraction, .15f, .6f);
            hesitationLift = Mathf.Max(0f, hesitationLift);
            behindBackContactLateralProgress = Mathf.Clamp(behindBackContactLateralProgress, .25f, .75f);
            behindBackDepthOffset = Mathf.Max(.1f, behindBackDepthOffset);
            behindBackWrapDepth = Mathf.Max(0f, behindBackWrapDepth);
            lowPoundMidpointDrop = Mathf.Max(0f, lowPoundMidpointDrop);
            highPoundArcLift = Mathf.Max(0f, highPoundArcLift);
            lowCrossoverMidpointDrop = Mathf.Max(0f, lowCrossoverMidpointDrop);
            highCrossoverArcLift = Mathf.Max(0f, highCrossoverArcLift);
            lowHesitationHoldReduction = Mathf.Clamp(lowHesitationHoldReduction, 0f, hesitationHoldFraction - .15f);
            lowHesitationLiftReduction = Mathf.Clamp(lowHesitationLiftReduction, 0f, hesitationLift);
            highHesitationHoldAddition = Mathf.Clamp(highHesitationHoldAddition, 0f, .6f - hesitationHoldFraction);
            highHesitationLiftAddition = Mathf.Max(0f, highHesitationLiftAddition);
            lowBehindBackDepthReduction = Mathf.Clamp(lowBehindBackDepthReduction, 0f, behindBackDepthOffset - .1f);
            lowBehindBackWrapReduction = Mathf.Clamp(lowBehindBackWrapReduction, 0f, behindBackWrapDepth);
            lowBehindBackMidpointDrop = Mathf.Max(0f, lowBehindBackMidpointDrop);
            highBehindBackDepthAddition = Mathf.Max(0f, highBehindBackDepthAddition);
            highBehindBackWrapAddition = Mathf.Max(0f, highBehindBackWrapAddition);
            highBehindBackArcLift = Mathf.Max(0f, highBehindBackArcLift);
            stanceModifierGraceSeconds = Mathf.Max(0f, stanceModifierGraceSeconds);
            SyncAnchorHeights();
            if (!Application.isPlaying)
            {
                currentHand = startingHand;
                currentStance = PlayerStance.Medium;
                nextExtremeFromMedium = PlayerStance.Low;
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
            {
                if (keyboard.rKey.wasPressedThisFrame)
                {
                    RestartPossession();
                    return;
                }

                if (possessionState == PossessionState.Ended)
                    return;

                bool leftPoundPressed = keyboard.wKey.wasPressedThisFrame;
                bool rightPoundPressed = keyboard.upArrowKey.wasPressedThisFrame;
                bool leftCrossoverPressed = keyboard.dKey.wasPressedThisFrame;
                bool rightCrossoverPressed = keyboard.leftArrowKey.wasPressedThisFrame;
                bool leftHesitationPressed = keyboard.aKey.wasPressedThisFrame;
                bool rightHesitationPressed = keyboard.rightArrowKey.wasPressedThisFrame;
                bool leftBehindBackPressed = keyboard.sKey.wasPressedThisFrame;
                bool rightBehindBackPressed = keyboard.downArrowKey.wasPressedThisFrame;

                if (keyboard.spaceKey.wasPressedThisFrame)
                {
                    if (pendingPlainSpace)
                        ResolvePendingPlainSpace();

                    spaceModifierConsumed = false;
                }

                if (keyboard.spaceKey.wasReleasedThisFrame && !spaceModifierConsumed)
                {
                    if (stanceModifierGraceSeconds <= 0f)
                    {
                        AdvanceStance();
                    }
                    else
                    {
                        pendingPlainSpace = true;
                        pendingPlainSpaceDeadline = Time.unscaledTime + stanceModifierGraceSeconds;
                    }
                }

                bool stanceModifierActive = keyboard.spaceKey.isPressed ||
                    (pendingPlainSpace && Time.unscaledTime <= pendingPlainSpaceDeadline);

                PlayerStance acceptedPoundStance = stanceModifierActive ? PlayerStance.Medium : currentStance;
                bool actionAccepted = ProcessInput(leftPoundPressed, rightPoundPressed, acceptedPoundStance);
                if (actionAccepted && stanceModifierActive)
                {
                    SetStance(PlayerStance.Medium);
                    ConsumeSpaceModifier();
                }

                if (!actionAccepted)
                {
                    PlayerStance acceptedCrossoverStance = stanceModifierActive ? PlayerStance.Low : currentStance;
                    actionAccepted = ProcessCrossoverInput(leftCrossoverPressed, rightCrossoverPressed,
                        acceptedCrossoverStance);
                    if (actionAccepted && stanceModifierActive)
                    {
                        SetStance(PlayerStance.Low);
                        ConsumeSpaceModifier();
                    }
                }

                if (!actionAccepted)
                {
                    PlayerStance acceptedHesitationStance = stanceModifierActive ? PlayerStance.High : currentStance;
                    actionAccepted = ProcessHesitationInput(leftHesitationPressed, rightHesitationPressed,
                        acceptedHesitationStance);
                    if (actionAccepted && stanceModifierActive)
                    {
                        SetStance(PlayerStance.High);
                        ConsumeSpaceModifier();
                    }
                }

                if (!actionAccepted)
                    ProcessBehindBackInput(leftBehindBackPressed, rightBehindBackPressed);

                if (pendingPlainSpace && Time.unscaledTime > pendingPlainSpaceDeadline)
                    ResolvePendingPlainSpace();
            }
            Tick(Time.deltaTime);
        }
        public bool ProcessInput(bool leftPressed, bool rightPressed)
        {
            return ProcessInput(leftPressed, rightPressed, currentStance);
        }

        private bool ProcessInput(bool leftPressed, bool rightPressed, PlayerStance acceptedPoundStance)
        {
            if (!TryIdentifyActiveInput(leftPressed, rightPressed)) return false;
            double dsp = AudioSettings.dspTime;
            double elapsed = rhythmClock != null ? rhythmClock.ElapsedSecondsAtDspTime(dsp) : 0;
            if (!TryCreateContactPlan(elapsed, dsp, false, out ContactTimingPlan plan)) return false;
            return DecideInput(DribbleAction.Pound, plan, false, acceptedPoundStance);
        }

        public bool ProcessInputAtRhythmTime(bool leftPressed, bool rightPressed, double elapsed)
        {
            if (!TryIdentifyActiveInput(leftPressed, rightPressed)) return false;
            double dsp = rhythmClock != null ? rhythmClock.StartDspTime + elapsed : elapsed;
            simulatedElapsedCursor = Math.Max(simulatedElapsedCursor, elapsed);
            if (!TryCreateContactPlan(elapsed, dsp, true, out ContactTimingPlan plan)) return false;
            return DecideInput(DribbleAction.Pound, plan, true, currentStance);
        }

        public bool TryStartPoundDribble() => currentHand == BallHand.Left ? ProcessInput(true, false) : ProcessInput(false, true);

        public bool ProcessCrossoverInput(bool leftToRightPressed, bool rightToLeftPressed)
        {
            return ProcessCrossoverInput(leftToRightPressed, rightToLeftPressed, currentStance);
        }

        private bool ProcessCrossoverInput(bool leftToRightPressed, bool rightToLeftPressed,
            PlayerStance acceptedStance)
        {
            if (!TryIdentifyActiveInput(leftToRightPressed, rightToLeftPressed)) return false;
            double dsp = AudioSettings.dspTime;
            double elapsed = rhythmClock != null ? rhythmClock.ElapsedSecondsAtDspTime(dsp) : 0;
            if (!TryCreateContactPlan(elapsed, dsp, false, out ContactTimingPlan plan)) return false;
            return DecideInput(DribbleAction.Crossover, plan, false, acceptedStance);
        }

        public bool ProcessCrossoverInputAtRhythmTime(bool leftToRightPressed, bool rightToLeftPressed, double elapsed)
        {
            if (!TryIdentifyActiveInput(leftToRightPressed, rightToLeftPressed)) return false;
            double dsp = rhythmClock != null ? rhythmClock.StartDspTime + elapsed : elapsed;
            simulatedElapsedCursor = Math.Max(simulatedElapsedCursor, elapsed);
            if (!TryCreateContactPlan(elapsed, dsp, true, out ContactTimingPlan plan)) return false;
            return DecideInput(DribbleAction.Crossover, plan, true, currentStance);
        }

        public bool TryStartCrossover() => currentHand == BallHand.Left
            ? ProcessCrossoverInput(true, false)
            : ProcessCrossoverInput(false, true);

        public bool ProcessHesitationInput(bool leftPressed, bool rightPressed)
        {
            return ProcessHesitationInput(leftPressed, rightPressed, currentStance);
        }

        private bool ProcessHesitationInput(bool leftPressed, bool rightPressed, PlayerStance acceptedStance)
        {
            if (!TryIdentifyActiveInput(leftPressed, rightPressed)) return false;
            double dsp = AudioSettings.dspTime;
            double elapsed = rhythmClock != null ? rhythmClock.ElapsedSecondsAtDspTime(dsp) : 0;
            if (!TryCreateContactPlan(elapsed, dsp, false, out ContactTimingPlan plan)) return false;
            return DecideInput(DribbleAction.Hesitation, plan, false, acceptedStance);
        }

        public bool ProcessHesitationInputAtRhythmTime(bool leftPressed, bool rightPressed, double elapsed)
        {
            if (!TryIdentifyActiveInput(leftPressed, rightPressed)) return false;
            double dsp = rhythmClock != null ? rhythmClock.StartDspTime + elapsed : elapsed;
            simulatedElapsedCursor = Math.Max(simulatedElapsedCursor, elapsed);
            if (!TryCreateContactPlan(elapsed, dsp, true, out ContactTimingPlan plan)) return false;
            return DecideInput(DribbleAction.Hesitation, plan, true, currentStance);
        }

        public bool TryStartHesitation() => currentHand == BallHand.Left
            ? ProcessHesitationInput(true, false)
            : ProcessHesitationInput(false, true);

        public bool ProcessBehindBackInput(bool leftToRightPressed, bool rightToLeftPressed)
        {
            if (!TryIdentifyActiveInput(leftToRightPressed, rightToLeftPressed)) return false;
            double dsp = AudioSettings.dspTime;
            double elapsed = rhythmClock != null ? rhythmClock.ElapsedSecondsAtDspTime(dsp) : 0;
            if (!TryCreateContactPlan(elapsed, dsp, false, out ContactTimingPlan plan)) return false;
            return DecideInput(DribbleAction.BehindTheBack, plan, false, currentStance);
        }

        public bool ProcessBehindBackInputAtRhythmTime(bool leftToRightPressed, bool rightToLeftPressed, double elapsed)
        {
            if (!TryIdentifyActiveInput(leftToRightPressed, rightToLeftPressed)) return false;
            double dsp = rhythmClock != null ? rhythmClock.StartDspTime + elapsed : elapsed;
            simulatedElapsedCursor = Math.Max(simulatedElapsedCursor, elapsed);
            if (!TryCreateContactPlan(elapsed, dsp, true, out ContactTimingPlan plan)) return false;
            return DecideInput(DribbleAction.BehindTheBack, plan, true, currentStance);
        }

        public bool TryStartBehindBack() => currentHand == BallHand.Left
            ? ProcessBehindBackInput(true, false)
            : ProcessBehindBackInput(false, true);

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
                Vector3 descentPosition = activeAction == DribbleAction.Hesitation
                    ? EvaluateHesitationDescent(t)
                    : activeAction == DribbleAction.BehindTheBack
                        ? EvaluateBehindBackDescent(shaped)
                        : activeAction == DribbleAction.Pound
                            ? EvaluatePoundDescent(shaped)
                            : EvaluateCrossoverDescent(shaped);
                if (activeAction != DribbleAction.Hesitation && activeArcLift > 0f)
                    descentPosition.y += Mathf.Sin(shaped * Mathf.PI) * activeArcLift;
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

        private void ConsumeSpaceModifier()
        {
            spaceModifierConsumed = true;
            pendingPlainSpace = false;
        }

        private void ResolvePendingPlainSpace()
        {
            pendingPlainSpace = false;
            AdvanceStance();
        }

        public void SetStance(PlayerStance stance)
        {
            currentStance = stance;
            Log($"stance changed to {currentStance}");
        }

        public void AdvanceStance()
        {
            if (currentStance == PlayerStance.Medium)
            {
                currentStance = nextExtremeFromMedium;
                nextExtremeFromMedium = currentStance == PlayerStance.Low ? PlayerStance.High : PlayerStance.Low;
            }
            else
            {
                currentStance = PlayerStance.Medium;
            }

            Log($"stance changed to {currentStance}");
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
            currentStance = PlayerStance.Medium;
            nextExtremeFromMedium = PlayerStance.Low;
            activeContactPlan = default;
            activeAction = DribbleAction.Pound;
            activeActionStance = PlayerStance.Medium;
            activeControlQuality = BallControlQuality.Secure;
            activeFollowUpRelation = FollowUpRelation.FirstAction;
            activeSourceHand = currentHand;
            activeTargetHand = currentHand;
            motionMode = BounceMotionMode.Normal;
            ClearPendingInput();
            lastInputDecision = "Reset to starting hand; waiting for input.";
            possessionState = PossessionState.Active;
            possessionEndReason = PossessionEndReason.None;
            hasPreviousAction = false;
            previousAction = DribbleAction.Pound;
            previousActionStance = PlayerStance.Medium;
            previousActionJudgment = TimingJudgment.None;
            previousResolvedHand = currentHand;
            previousControlQuality = BallControlQuality.Secure;
            sequenceActionCount = 0;
            spaceModifierConsumed = false;
            pendingPlainSpace = false;
            pendingPlainSpaceDeadline = 0f;
            SetContactIndicator(false);
            SyncAnchorHeights();
            SnapToCurrentHand();
        }

        public void RestartPossession()
        {
            ResetToStartingHand();
            if (rhythmClock != null)
                rhythmClock.RestartClock();
            lastInputDecision = "Possession restarted; waiting for input.";
            Log("possession restarted");
        }

        public void EndPossession(PossessionEndReason reason)
        {
            if (possessionState == PossessionState.Ended) return;

            possessionState = PossessionState.Ended;
            possessionEndReason = reason == PossessionEndReason.None ? PossessionEndReason.Manual : reason;
            ClearPendingInput();
            pendingPlainSpace = false;
            pendingPlainSpaceDeadline = 0f;
            spaceModifierConsumed = false;
            lastInputDecision = $"Possession ended: {possessionEndReason}.";
            Log(lastInputDecision);
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

        private bool TryCreateContactPlan(double elapsed, double dsp, bool simulated, out ContactTimingPlan plan)
        {
            plan = default;
            if (possessionState == PossessionState.Ended)
            {
                lastInputDecision = $"Rejected: possession ended ({possessionEndReason}). Restart to continue.";
                return false;
            }

            double referenceOverride = CurrentReferenceContactBeatOverride();
            if (rhythmClock != null &&
                !rhythmClock.HasAvailableContinuationTargetAtElapsedTime(
                    elapsed, minimumReadableContactApproachTime, referenceOverride))
            {
                EndPossession(PossessionEndReason.ContinuationWindowExpired);
                return false;
            }

            plan = CreateContactPlan(elapsed, dsp, simulated);
            return true;
        }

        private double CurrentReferenceContactBeatOverride()
        {
            return (logicalPhase == BallLogicalPhase.Descending || logicalPhase == BallLogicalPhase.FloorContact) &&
                   activeContactPlan.TargetContactBeat > 0
                ? activeContactPlan.TargetContactBeat
                : double.NaN;
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
            double referenceOverride = CurrentReferenceContactBeatOverride();
            return rhythmClock.PlanNextContactAtElapsedTime(elapsed, dsp, ReferenceDescentDuration,
                minimumReadableContactApproachTime, preferHalfBeat, referenceOverride);
        }

        private bool DecideInput(DribbleAction action, ContactTimingPlan plan, bool simulated,
            PlayerStance acceptedStance)
        {
            TimingJudgment judgment = plan.Judgment;
            FollowUpRelation relation = DetermineFollowUpRelation(action);
            lastTimingJudgment = judgment;
            timingJudgmentCount++;
            TimingJudged?.Invoke(judgment);

            if (logicalPhase == BallLogicalPhase.Controlled || logicalPhase == BallLogicalPhase.Returning)
            {
                BallLogicalPhase sourcePhase = logicalPhase;
                if (!BeginDribble(action, plan, simulated, acceptedStance, relation)) return false;
                lastInputDecision = $"Accepted {action} from {sourcePhase} as {relation}; contact scheduled on the global grid.";
                return true;
            }
            if (!hasPendingInput)
            {
                hasPendingInput = true;
                pendingContactPlan = plan;
                pendingAction = action;
                pendingActionStance = acceptedStance;
                pendingFollowUpRelation = relation;
                pendingUsesSimulatedTime = simulated;
                lastInputDecision = $"Accepted {action} as {relation} and queued during {logicalPhase}; contact target preserved.";
                Log($"{action} {relation} follow-up queued — target DSP {plan.TargetContactDspTimestamp:0.000000}, {RhythmicIntervalCatalog.Label(plan.Interval)}");
                return true;
            }

            lastInputDecision = $"Rejected: pending input slot already occupied during {logicalPhase}.";
            Log(lastInputDecision);
            return false;
        }

        private bool BeginDribble(DribbleAction action, ContactTimingPlan plan, bool simulated,
            PlayerStance acceptedStance, FollowUpRelation followUpRelation)
        {
            BallHand sourceHand = currentHand;
            bool transfersHand = action == DribbleAction.Crossover || action == DribbleAction.BehindTheBack;
            BallHand targetHand = transfersHand ? Opposite(sourceHand) : sourceHand;
            Transform sourceAnchor = GetAnchor(sourceHand);
            Transform targetAnchor = GetAnchor(targetHand);
            if (sourceAnchor == null || targetAnchor == null)
            {
                lastInputDecision = "Rejected: required hand anchor is missing.";
                motionMode = BounceMotionMode.Unreachable;
                return false;
            }

            double nowDsp = simulated
                ? (rhythmClock != null ? rhythmClock.StartDspTime + simulatedElapsedCursor : simulatedElapsedCursor)
                : AudioSettings.dspTime;
            double remaining = plan.TargetContactDspTimestamp - nowDsp;
            Vector3 startPosition = logicalPhase == BallLogicalPhase.Controlled ? sourceAnchor.position : transform.position;
            Vector3 targetPosition;
            if (action == DribbleAction.Crossover)
            {
                Vector3 lateralContact = Vector3.Lerp(sourceAnchor.position, targetAnchor.position, crossoverContactLateralProgress);
                targetPosition = new Vector3(lateralContact.x, floorHeight, lateralContact.z);
            }
            else if (action == DribbleAction.BehindTheBack)
            {
                Vector3 lateralContact = Vector3.Lerp(sourceAnchor.position, targetAnchor.position, behindBackContactLateralProgress);
                targetPosition = new Vector3(lateralContact.x, floorHeight,
                    lateralContact.z - BehindBackDepthFor(acceptedStance));
            }
            else
            {
                targetPosition = new Vector3(startPosition.x, floorHeight, startPosition.z);
            }
            activeArcLift = startPosition.y < minimumFastBounceHeight ? minimumFastBounceHeight - startPosition.y : 0f;
            float actionLift = action == DribbleAction.Hesitation ? HesitationLiftFor(acceptedStance) : 0f;
            float pathDepth = action == DribbleAction.BehindTheBack ? BehindBackWrapFor(acceptedStance) : 0f;
            float stanceVerticalExcursion = StanceVerticalExcursionFor(action, acceptedStance);
            float requiredDistance = Vector3.Distance(startPosition, targetPosition) +
                                     (activeArcLift + actionLift + pathDepth + stanceVerticalExcursion) * 2f;
            double movementTime = action == DribbleAction.Hesitation
                ? remaining * Math.Max(.1, 1.0 - HesitationHoldFractionFor(acceptedStance) * .5)
                : remaining;
            if (remaining < minimumReadableContactApproachTime || requiredDistance / Math.Max(.001, movementTime) > maximumDescentSpeed)
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
            activeAction = action;
            activeActionStance = acceptedStance;
            activeControlQuality = ControlQualityFor(plan.Judgment.Result);
            activeFollowUpRelation = followUpRelation;
            activeSourceHand = sourceHand;
            activeTargetHand = targetHand;
            motionMode = remaining < ReferenceDescentDuration * .9 || startPosition.y < minimumFastBounceHeight
                ? BounceMotionMode.Compressed
                : BounceMotionMode.Normal;
            ApplyTimingProfile(plan.Judgment.Result);
            handPosition = targetAnchor.position;
            floorPosition = targetPosition;
            activeMotionStartPosition = startPosition;
            activeMotionStartDsp = nowDsp;
            activeBounceDuration = (float)remaining + activeContactHold + activeReturnDuration;
            visualPhaseElapsed = 0f;
            logicalPhase = BallLogicalPhase.Descending;
            SetContactIndicator(false);
            DribbleStarted?.Invoke(activeSourceHand);
            Log($"{action} began — input DSP {plan.InputDspTimestamp:0.000000}, target contact DSP {plan.TargetContactDspTimestamp:0.000000}, mode {motionMode}, stance {activeActionStance}, relation {activeFollowUpRelation}, control {activeControlQuality}");
            return true;
        }

        public static BallControlQuality ControlQualityFor(TimingResult result)
        {
            switch (result)
            {
                case TimingResult.Early:
                case TimingResult.Late:
                    return BallControlQuality.Recovering;
                case TimingResult.BrokenRhythm:
                    return BallControlQuality.Exposed;
                default:
                    return BallControlQuality.Secure;
            }
        }

        private FollowUpRelation DetermineFollowUpRelation(DribbleAction nextAction)
        {
            if (!TryGetImmediatePreviousAction(out DribbleAction previous))
                return FollowUpRelation.FirstAction;

            if (previous == nextAction)
                return FollowUpRelation.Repeat;

            bool nextTransfers = TransfersHand(nextAction);
            if (!nextTransfers)
                return FollowUpRelation.SameHandVariation;

            return TransfersHand(previous)
                ? FollowUpRelation.CounterTransfer
                : FollowUpRelation.Transfer;
        }

        private bool TryGetImmediatePreviousAction(out DribbleAction previous)
        {
            if (logicalPhase == BallLogicalPhase.Descending || logicalPhase == BallLogicalPhase.FloorContact)
            {
                previous = activeAction;
                return true;
            }

            if (hasPreviousAction)
            {
                previous = previousAction;
                return true;
            }

            previous = DribbleAction.Pound;
            return false;
        }

        private static bool TransfersHand(DribbleAction action) =>
            action == DribbleAction.Crossover || action == DribbleAction.BehindTheBack;

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
            float sign = activeSourceHand == BallHand.Left ? -1f : 1f;
            position.x += Mathf.Sin(Mathf.Clamp01(distanceFromHand) * Mathf.PI) * activeLateralDrift * sign;
            return position;
        }

        private Vector3 EvaluatePoundDescent(float progress)
        {
            Vector3 position = Vector3.Lerp(activeMotionStartPosition, floorPosition, progress);
            float profileWeight = Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI);
            if (activeActionStance == PlayerStance.Low)
                position.y -= profileWeight * lowPoundMidpointDrop;
            else if (activeActionStance == PlayerStance.High)
                position.y += profileWeight * highPoundArcLift;
            return position;
        }

        private Vector3 EvaluateCrossoverDescent(float progress)
        {
            Vector3 position = Vector3.Lerp(activeMotionStartPosition, floorPosition, progress);
            float profileWeight = Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI);
            if (activeActionStance == PlayerStance.Low)
                position.y -= profileWeight * lowCrossoverMidpointDrop;
            else if (activeActionStance == PlayerStance.High)
                position.y += profileWeight * highCrossoverArcLift;
            return position;
        }

        private Vector3 EvaluateHesitationDescent(float elapsedFraction)
        {
            float holdEnd = HesitationHoldFractionFor(activeActionStance);
            float liftEnd = holdEnd * .5f;
            Vector3 heldPosition = activeMotionStartPosition +
                                   Vector3.up * (HesitationLiftFor(activeActionStance) + activeArcLift);
            if (elapsedFraction < liftEnd)
                return Vector3.Lerp(activeMotionStartPosition, heldPosition, Smooth(elapsedFraction / Mathf.Max(.001f, liftEnd)));
            if (elapsedFraction < holdEnd) return heldPosition;

            float descentT = Mathf.Clamp01((elapsedFraction - holdEnd) / Mathf.Max(.001f, 1f - holdEnd));
            float shaped = motionMode == BounceMotionMode.Compressed
                ? Mathf.Lerp(Smooth(descentT), descentT, trajectoryCompressionAmount)
                : Smooth(descentT);
            return Vector3.Lerp(heldPosition, floorPosition, shaped);
        }

        private Vector3 EvaluateBehindBackDescent(float progress)
        {
            Vector3 control = Vector3.Lerp(activeMotionStartPosition, floorPosition, .5f);
            control.z = Mathf.Min(activeMotionStartPosition.z, floorPosition.z) -
                        BehindBackWrapFor(activeActionStance);
            if (activeActionStance == PlayerStance.Low)
                control.y -= lowBehindBackMidpointDrop * 2f;
            else if (activeActionStance == PlayerStance.High)
                control.y += highBehindBackArcLift * 2f;
            float inverse = 1f - progress;
            return inverse * inverse * activeMotionStartPosition + 2f * inverse * progress * control + progress * progress * floorPosition;
        }

        private float HesitationHoldFractionFor(PlayerStance stance)
        {
            if (stance == PlayerStance.Low)
                return Mathf.Clamp(hesitationHoldFraction - lowHesitationHoldReduction, .15f, .6f);
            if (stance == PlayerStance.High)
                return Mathf.Clamp(hesitationHoldFraction + highHesitationHoldAddition, .15f, .6f);
            return hesitationHoldFraction;
        }

        private float HesitationLiftFor(PlayerStance stance)
        {
            if (stance == PlayerStance.Low)
                return Mathf.Max(0f, hesitationLift - lowHesitationLiftReduction);
            if (stance == PlayerStance.High)
                return hesitationLift + highHesitationLiftAddition;
            return hesitationLift;
        }

        private float BehindBackDepthFor(PlayerStance stance)
        {
            if (stance == PlayerStance.Low)
                return Mathf.Max(.1f, behindBackDepthOffset - lowBehindBackDepthReduction);
            if (stance == PlayerStance.High)
                return behindBackDepthOffset + highBehindBackDepthAddition;
            return behindBackDepthOffset;
        }

        private float BehindBackWrapFor(PlayerStance stance)
        {
            if (stance == PlayerStance.Low)
                return Mathf.Max(0f, behindBackWrapDepth - lowBehindBackWrapReduction);
            if (stance == PlayerStance.High)
                return behindBackWrapDepth + highBehindBackWrapAddition;
            return behindBackWrapDepth;
        }

        private float StanceVerticalExcursionFor(DribbleAction action, PlayerStance stance)
        {
            if (action == DribbleAction.Pound)
                return stance == PlayerStance.High ? highPoundArcLift : stance == PlayerStance.Low ? lowPoundMidpointDrop : 0f;
            if (action == DribbleAction.Crossover)
                return stance == PlayerStance.High ? highCrossoverArcLift : stance == PlayerStance.Low ? lowCrossoverMidpointDrop : 0f;
            if (action == DribbleAction.BehindTheBack)
                return stance == PlayerStance.High ? highBehindBackArcLift : stance == PlayerStance.Low ? lowBehindBackMidpointDrop : 0f;
            return 0f;
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
            previousAction = activeAction;
            previousActionStance = activeActionStance;
            previousActionJudgment = activeContactPlan.Judgment;
            previousControlQuality = activeControlQuality;
            hasPreviousAction = true;
            sequenceActionCount++;
            if (activeAction == DribbleAction.Crossover)
            {
                currentHand = activeTargetHand;
                completedCrossoverCount++;
            }
            else if (activeAction == DribbleAction.BehindTheBack)
            {
                currentHand = activeTargetHand;
                completedBehindBackCount++;
            }
            previousResolvedHand = currentHand;
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
            DribbleAction action = pendingAction;
            PlayerStance acceptedStance = pendingActionStance;
            FollowUpRelation relation = pendingFollowUpRelation;
            bool simulated = pendingUsesSimulatedTime;
            ClearPendingInput();
            if (BeginDribble(action, plan, simulated, acceptedStance, relation))
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
            pendingAction = DribbleAction.Pound;
            pendingActionStance = PlayerStance.Medium;
            pendingFollowUpRelation = FollowUpRelation.FirstAction;
            pendingUsesSimulatedTime = false;
        }

        private float ReferenceDescentDuration => Mathf.Max(.05f, (visualBounceDuration - floorContactHold) * .5f);
        private double CurrentDspTime() => activeUsesSimulatedTime && rhythmClock != null
            ? rhythmClock.StartDspTime + simulatedElapsedCursor
            : AudioSettings.dspTime;

        private Transform GetCurrentAnchor() => GetAnchor(currentHand);
        private Transform GetAnchor(BallHand hand) => hand == BallHand.Left ? leftHandAnchor : rightHandAnchor;
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
            if (showDebugOverlay)
            {
                const float width = 540f;
            GUI.Box(new Rect(18, 18, width, 480), "Bounce Theory — Target Contact / Rhythm Debug");
            GUI.Label(new Rect(32, 45, width - 24, 22), $"Possession: {possessionState}   End reason: {possessionEndReason}");
            GUI.Label(new Rect(32, 67, width - 24, 22), $"Hand: {currentHand}   Stance: {currentStance}   Action: {activeAction}   Phase: {logicalPhase}   Control: {activeControlQuality}");
            if (rhythmClock != null)
            {
                GUI.Label(new Rect(32, 89, width - 24, 22), $"BPM: {rhythmClock.Bpm:0.##}   Global beat: {rhythmClock.CurrentBeatPosition:0.000}   Phase: {rhythmClock.CurrentBeatPhase:0.000}");
                GUI.Label(new Rect(32, 111, width - 24, 22), $"Subdivision: {rhythmClock.CurrentSubdivision + 1}/{rhythmClock.SubdivisionsPerBeat}");
                string previous = rhythmClock.HasPreviousBallEvent ? $"{rhythmClock.PreviousBallEventElapsedSeconds:0.000}s (grid {rhythmClock.PreviousBallEventAlignedBeat:0.##})" : "None — first input uses global beat";
                GUI.Label(new Rect(32, 133, width - 24, 22), $"Previous floor event: {previous}");
            }
            string result = lastTimingJudgment.IsValid ? lastTimingJudgment.Result.ToString() : "Waiting";
            string error = lastTimingJudgment.IsValid ? $"{lastTimingJudgment.ErrorMilliseconds:+0.0;-0.0;0.0} ms" : "—";
            string direction = lastTimingJudgment.IsValid ? lastTimingJudgment.Direction.ToString() : "—";
            string interval = lastTimingJudgment.HasRhythmicInterval ? RhythmicIntervalCatalog.Label(lastTimingJudgment.Interval) : "Global beat / first input";
            GUI.Label(new Rect(32, 155, width - 24, 22), $"Selected interval: {interval}   Motion: {motionMode}   Action profile: {activeActionStance}");
            GUI.Label(new Rect(32, 177, width - 24, 22), $"Input timing: {result}   Error: {error}   Direction: {direction}");
            GUI.Label(new Rect(32, 199, width - 24, 22), $"Keypress DSP: {(activeContactPlan.InputDspTimestamp > 0 ? activeContactPlan.InputDspTimestamp.ToString("0.000000") : "—")}");
            GUI.Label(new Rect(32, 221, width - 24, 22), $"Target floor-contact DSP: {(activeContactPlan.TargetContactDspTimestamp > 0 ? activeContactPlan.TargetContactDspTimestamp.ToString("0.000000") : "—")}");
            GUI.Label(new Rect(32, 243, width - 24, 22), $"Actual floor-contact DSP: {(lastActualFloorContactDsp > 0 ? lastActualFloorContactDsp.ToString("0.000000") : "—")}");
            GUI.Label(new Rect(32, 265, width - 24, 22), $"Contact error: {(lastActualFloorContactDsp > 0 ? lastContactErrorMilliseconds.ToString("+0.0;-0.0;0.0") + " ms" : "—")}");
            GUI.Label(new Rect(32, 287, width - 24, 22), $"Pending input: {(hasPendingInput ? pendingAction + " / " + pendingActionStance + " / " + pendingFollowUpRelation : "No")}");
            GUI.Label(new Rect(32, 309, width - 24, 22), $"Pending keypress DSP: {(hasPendingInput ? pendingContactPlan.InputDspTimestamp.ToString("0.000000") : "—")}");
            GUI.Label(new Rect(32, 331, width - 24, 22), $"Pending target DSP: {(hasPendingInput ? pendingContactPlan.TargetContactDspTimestamp.ToString("0.000000") : "—")}   Interval: {(hasPendingInput ? RhythmicIntervalCatalog.Label(pendingContactPlan.Interval) : "—")}");
            GUI.Label(new Rect(32, 353, width - 24, 42), $"Last decision/failure: {lastInputDecision}");
            GUI.Label(new Rect(32, 397, width - 24, 22), currentHand == BallHand.Left
                ? "Inputs: W pound / D cross / A hesi / S behind-back / Space stance"
                : "Inputs: Up pound / Left cross / Right hesi / Down behind-back / Space stance");
            GUI.Label(new Rect(32, 419, width - 24, 22), $"Stance modifier grace: {stanceModifierGraceSeconds * 1000f:0} ms   Pending Space: {(pendingPlainSpace ? "Yes" : "No")}");
            GUI.Label(new Rect(32, 441, width - 24, 22), hasPreviousAction
                ? $"Resolved #{sequenceActionCount}: {previousAction} / {previousActionStance} / {previousActionJudgment.Result} / {previousControlQuality} / hand {previousResolvedHand}"
                : $"Resolved sequence: None   Active relation: {activeFollowUpRelation}   R = restart");
                GUI.Label(new Rect(32, 463, width - 24, 22), $"Active relation: {activeFollowUpRelation}   Impact sound fires at FloorContact.");
            }

            if (possessionState == PossessionState.Ended)
            {
                const float restartWidth = 360f;
                const float restartHeight = 150f;
                Rect restartRect = new Rect((Screen.width - restartWidth) * .5f, (Screen.height - restartHeight) * .5f,
                    restartWidth, restartHeight);
                GUI.Box(restartRect, "Possession Ended");
                GUI.Label(new Rect(restartRect.x + 24f, restartRect.y + 38f, restartWidth - 48f, 24f),
                    $"Reason: {possessionEndReason}");
                GUI.Label(new Rect(restartRect.x + 24f, restartRect.y + 64f, restartWidth - 48f, 24f),
                    "Prototype restart — final menu/failure rules are not implemented.");
                if (GUI.Button(new Rect(restartRect.x + 90f, restartRect.y + 98f, 180f, 34f), "Restart (R)"))
                    RestartPossession();
            }
        }
    }
}
