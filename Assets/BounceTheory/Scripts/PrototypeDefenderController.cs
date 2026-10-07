using UnityEngine;

namespace BounceTheory
{
    public enum DefenderState
    {
        Centered,
        LeaningLeft,
        LeaningRight,
        Recovering,
        Reaching,
        Overcommitted,
        Beaten
    }

    public enum StealOpportunity
    {
        Protected,
        Contested,
        Vulnerable
    }

    public enum DefenderReachOutcome
    {
        None,
        Missed,
        Stolen
    }

    /// <summary>
    /// Prototype defender reactions driven by authoritative dribble/gameplay state.
    /// The defender root remains stationary; only the configured visual pivot moves.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeDefenderController : MonoBehaviour
    {
        [Header("Gameplay Source")]
        [SerializeField] private PoundDribbleController dribbleController;

        [Header("Visual Root")]
        [SerializeField] private Transform visualRoot;
        [SerializeField, Min(0f)] private float leanOffset = 0.34f;
        [SerializeField, Range(0f, 30f)] private float leanTiltDegrees = 12f;
        [SerializeField, Min(0.1f)] private float leanResponseSpeed = 12f;

        [Header("Recovery Timing")]
        [SerializeField, Min(0.05f)] private float secureRecoverySeconds = 0.62f;
        [SerializeField, Min(0.05f)] private float recoveringRecoverySeconds = 0.38f;
        [SerializeField, Min(0.05f)] private float exposedRecoverySeconds = 0.20f;
        [SerializeField, Min(0f)] private float hesitationRecoveryExtensionSeconds = 0.20f;
        [SerializeField, Min(0.1f)] private float maximumRecoverySeconds = 1.25f;

        [Header("Reach / Overcommit")]
        [SerializeField, Min(0f)] private float reachLateralOffset = 0.24f;
        [SerializeField, Min(0f)] private float reachForwardOffset = 0.34f;
        [SerializeField, Range(0f, 35f)] private float reachTiltDegrees = 16f;
        [SerializeField, Min(0.05f)] private float overcommitSeconds = 0.58f;
        [SerializeField, Min(0f)] private float overcommitOffset = 0.52f;
        [SerializeField, Range(0f, 45f)] private float overcommitTiltDegrees = 23f;

        [Header("Beaten Window")]
        [SerializeField, Min(0.1f)] private float beatenSeconds = 1.05f;
        [SerializeField, Min(0f)] private float beatenOffset = 0.62f;
        [SerializeField, Range(0f, 50f)] private float beatenTiltDegrees = 28f;

        [Header("Temporary Debug")]
        [SerializeField] private bool showDebugOverlay = true;

        private DefenderState currentState = DefenderState.Centered;
        private BallHand committedSide = BallHand.Left;
        private bool hasCommitment;
        private BallHand reactionSide = BallHand.Left;
        private Vector3 neutralLocalPosition;
        private Quaternion neutralLocalRotation = Quaternion.identity;
        private Vector3 recoveryStartLocalPosition;
        private Quaternion recoveryStartLocalRotation = Quaternion.identity;
        private float recoveryDuration;
        private float recoveryElapsed;
        private float stateElapsed;
        private StealOpportunity currentStealOpportunity = StealOpportunity.Protected;
        private DefenderReachOutcome lastReachOutcome = DefenderReachOutcome.None;
        private int reachAttemptCount;
        private int stealCount;
        private int overcommitCount;
        private int beatenCount;
        private bool subscribed;

        public DefenderState CurrentState => currentState;
        public float RecoveryDuration => recoveryDuration;
        public float RecoveryElapsed => recoveryElapsed;
        public float RecoveryRemaining => Mathf.Max(0f, recoveryDuration - recoveryElapsed);
        public float SecureRecoverySeconds => secureRecoverySeconds;
        public float RecoveringRecoverySeconds => recoveringRecoverySeconds;
        public float ExposedRecoverySeconds => exposedRecoverySeconds;
        public float HesitationRecoveryExtensionSeconds => hesitationRecoveryExtensionSeconds;
        public float OvercommitSeconds => overcommitSeconds;
        public float BeatenSeconds => beatenSeconds;
        public float StateElapsed => stateElapsed;
        public float StateRemaining => currentState == DefenderState.Overcommitted
            ? Mathf.Max(0f, overcommitSeconds - stateElapsed)
            : currentState == DefenderState.Beaten
                ? Mathf.Max(0f, beatenSeconds - stateElapsed)
                : 0f;
        public StealOpportunity CurrentStealOpportunity => currentStealOpportunity;
        public DefenderReachOutcome LastReachOutcome => lastReachOutcome;
        public int ReachAttemptCount => reachAttemptCount;
        public int StealCount => stealCount;
        public int OvercommitCount => overcommitCount;
        public int BeatenCount => beatenCount;
        public Transform VisualRoot => visualRoot;

        private void Awake()
        {
            CacheNeutralPose();
            ResetDefenderState();
        }

        private void OnEnable()
        {
            CacheNeutralPose();
            Subscribe();
        }

        private void OnDisable() => Unsubscribe();

        private void OnValidate()
        {
            leanOffset = Mathf.Max(0f, leanOffset);
            leanTiltDegrees = Mathf.Clamp(leanTiltDegrees, 0f, 30f);
            leanResponseSpeed = Mathf.Max(.1f, leanResponseSpeed);
            secureRecoverySeconds = Mathf.Max(.05f, secureRecoverySeconds);
            recoveringRecoverySeconds = Mathf.Max(.05f, recoveringRecoverySeconds);
            exposedRecoverySeconds = Mathf.Max(.05f, exposedRecoverySeconds);
            hesitationRecoveryExtensionSeconds = Mathf.Max(0f, hesitationRecoveryExtensionSeconds);
            maximumRecoverySeconds = Mathf.Max(.1f, maximumRecoverySeconds);
            reachLateralOffset = Mathf.Max(0f, reachLateralOffset);
            reachForwardOffset = Mathf.Max(0f, reachForwardOffset);
            reachTiltDegrees = Mathf.Clamp(reachTiltDegrees, 0f, 35f);
            overcommitSeconds = Mathf.Max(.05f, overcommitSeconds);
            overcommitOffset = Mathf.Max(0f, overcommitOffset);
            overcommitTiltDegrees = Mathf.Clamp(overcommitTiltDegrees, 0f, 45f);
            beatenSeconds = Mathf.Max(.1f, beatenSeconds);
            beatenOffset = Mathf.Max(0f, beatenOffset);
            beatenTiltDegrees = Mathf.Clamp(beatenTiltDegrees, 0f, 50f);
        }

        private void Update() => Tick(Time.deltaTime);

        public void Configure(PoundDribbleController controller, Transform reactionVisualRoot)
        {
            Unsubscribe();
            dribbleController = controller;
            visualRoot = reactionVisualRoot;
            CacheNeutralPose();
            ResetDefenderState();
            Subscribe();
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || visualRoot == null) return;

            if (currentState == DefenderState.Recovering)
            {
                recoveryElapsed += deltaTime;
                if (recoveryElapsed >= recoveryDuration)
                {
                    SetCentered();
                    return;
                }

                float t = Mathf.SmoothStep(0f, 1f,
                    recoveryDuration <= .0001f ? 1f : recoveryElapsed / recoveryDuration);
                visualRoot.localPosition = Vector3.Lerp(recoveryStartLocalPosition, neutralLocalPosition, t);
                visualRoot.localRotation = Quaternion.Slerp(recoveryStartLocalRotation, neutralLocalRotation, t);
                return;
            }

            if (currentState == DefenderState.Overcommitted)
            {
                stateElapsed += deltaTime;
                if (stateElapsed >= overcommitSeconds)
                {
                    SetCentered();
                    return;
                }
            }
            else if (currentState == DefenderState.Beaten)
            {
                stateElapsed += deltaTime;
                if (stateElapsed >= beatenSeconds)
                {
                    SetCentered();
                    return;
                }
            }

            ApplyReadablePose(deltaTime);
        }

        public void ResetDefenderState()
        {
            currentState = DefenderState.Centered;
            hasCommitment = false;
            committedSide = BallHand.Left;
            reactionSide = BallHand.Left;
            recoveryDuration = 0f;
            recoveryElapsed = 0f;
            stateElapsed = 0f;
            currentStealOpportunity = StealOpportunity.Protected;
            lastReachOutcome = DefenderReachOutcome.None;
            if (visualRoot != null)
            {
                visualRoot.localPosition = neutralLocalPosition;
                visualRoot.localRotation = neutralLocalRotation;
            }
        }

        public float RecoveryDurationFor(BallControlQuality quality)
        {
            switch (quality)
            {
                case BallControlQuality.Exposed:
                    return exposedRecoverySeconds;
                case BallControlQuality.Recovering:
                    return recoveringRecoverySeconds;
                default:
                    return secureRecoverySeconds;
            }
        }

        public static StealOpportunity StealOpportunityFor(BallControlQuality quality)
        {
            switch (quality)
            {
                case BallControlQuality.Exposed:
                    return StealOpportunity.Vulnerable;
                case BallControlQuality.Recovering:
                    return StealOpportunity.Contested;
                default:
                    return StealOpportunity.Protected;
            }
        }

        private void HandleDribbleStarted(BallHand sourceHand)
        {
            if (dribbleController == null ||
                dribbleController.CurrentPossessionState != PossessionState.Active)
                return;

            if (currentState == DefenderState.Beaten)
                return;

            if (currentState == DefenderState.Overcommitted)
            {
                if (IsSuccessfulOvercommitCounter())
                    BeginBeaten();
                return;
            }

            if (currentState == DefenderState.Recovering)
            {
                if (dribbleController.ActiveAction == DribbleAction.Hesitation)
                    ExtendRecoveryForHesitation();
                return;
            }

            StealOpportunity opportunity = StealOpportunityFor(dribbleController.ActiveControlQuality);
            if (opportunity != StealOpportunity.Protected)
            {
                BeginReach(sourceHand, opportunity);
                return;
            }

            committedSide = sourceHand;
            reactionSide = sourceHand;
            hasCommitment = true;
            currentState = sourceHand == BallHand.Left
                ? DefenderState.LeaningLeft
                : DefenderState.LeaningRight;
            currentStealOpportunity = StealOpportunity.Protected;
            lastReachOutcome = DefenderReachOutcome.None;
            stateElapsed = 0f;
        }

        private void HandleFloorContact(BallHand resolvedHand)
        {
            if (dribbleController == null) return;

            if (currentState == DefenderState.Reaching)
            {
                ResolveReachAtFloorContact();
                return;
            }

            if (!hasCommitment) return;

            bool transfersHand = dribbleController.ActiveAction == DribbleAction.Crossover ||
                                 dribbleController.ActiveAction == DribbleAction.BehindTheBack;
            if (!transfersHand || resolvedHand == committedSide) return;

            BeginRecovery(dribbleController.ActiveControlQuality);
        }

        private void HandleBallReturned(BallHand _)
        {
            if (currentState == DefenderState.LeaningLeft ||
                currentState == DefenderState.LeaningRight)
                SetCentered();
        }

        private void BeginReach(BallHand sourceHand, StealOpportunity opportunity)
        {
            currentState = DefenderState.Reaching;
            currentStealOpportunity = opportunity;
            lastReachOutcome = DefenderReachOutcome.None;
            reactionSide = sourceHand;
            hasCommitment = false;
            stateElapsed = 0f;
            reachAttemptCount++;
        }

        private void ResolveReachAtFloorContact()
        {
            bool protectedByQueuedResponse = dribbleController != null && dribbleController.HasPendingInput;
            if (currentStealOpportunity == StealOpportunity.Vulnerable ||
                (currentStealOpportunity == StealOpportunity.Contested && !protectedByQueuedResponse))
            {
                lastReachOutcome = DefenderReachOutcome.Stolen;
                stealCount++;
                hasCommitment = false;
                stateElapsed = 0f;
                dribbleController?.EndPossession(PossessionEndReason.DefenderSteal);
                return;
            }

            if (currentStealOpportunity == StealOpportunity.Contested && protectedByQueuedResponse)
            {
                lastReachOutcome = DefenderReachOutcome.Missed;
                BeginOvercommit();
                return;
            }

            SetCentered();
        }

        private void BeginRecovery(BallControlQuality offensiveQuality)
        {
            currentState = DefenderState.Recovering;
            currentStealOpportunity = StealOpportunity.Protected;
            lastReachOutcome = DefenderReachOutcome.None;
            hasCommitment = false;
            recoveryElapsed = 0f;
            recoveryDuration = Mathf.Min(maximumRecoverySeconds, RecoveryDurationFor(offensiveQuality));
            if (visualRoot != null)
            {
                recoveryStartLocalPosition = visualRoot.localPosition;
                recoveryStartLocalRotation = visualRoot.localRotation;
            }
        }

        private void ExtendRecoveryForHesitation()
        {
            if (currentState != DefenderState.Recovering) return;
            recoveryDuration = Mathf.Min(maximumRecoverySeconds,
                recoveryDuration + hesitationRecoveryExtensionSeconds);
        }

        private void BeginOvercommit()
        {
            currentState = DefenderState.Overcommitted;
            currentStealOpportunity = StealOpportunity.Protected;
            hasCommitment = false;
            stateElapsed = 0f;
            overcommitCount++;
        }

        private bool IsSuccessfulOvercommitCounter()
        {
            if (dribbleController == null ||
                dribbleController.ActiveControlQuality != BallControlQuality.Secure)
                return false;

            return dribbleController.ActiveAction == DribbleAction.Crossover ||
                   dribbleController.ActiveAction == DribbleAction.Hesitation ||
                   dribbleController.ActiveAction == DribbleAction.BehindTheBack;
        }

        private void BeginBeaten()
        {
            currentState = DefenderState.Beaten;
            currentStealOpportunity = StealOpportunity.Protected;
            hasCommitment = false;
            stateElapsed = 0f;
            beatenCount++;
        }

        private void SetCentered()
        {
            currentState = DefenderState.Centered;
            hasCommitment = false;
            recoveryDuration = 0f;
            recoveryElapsed = 0f;
            stateElapsed = 0f;
            currentStealOpportunity = StealOpportunity.Protected;
            if (visualRoot != null)
            {
                visualRoot.localPosition = neutralLocalPosition;
                visualRoot.localRotation = neutralLocalRotation;
            }
        }

        private void ApplyReadablePose(float deltaTime)
        {
            Vector3 targetPosition = neutralLocalPosition;
            Quaternion targetRotation = neutralLocalRotation;
            float sideSign = reactionSide == BallHand.Left ? -1f : 1f;

            switch (currentState)
            {
                case DefenderState.LeaningLeft:
                    targetPosition += Vector3.left * leanOffset;
                    targetRotation = neutralLocalRotation * Quaternion.Euler(0f, 0f, leanTiltDegrees);
                    break;
                case DefenderState.LeaningRight:
                    targetPosition += Vector3.right * leanOffset;
                    targetRotation = neutralLocalRotation * Quaternion.Euler(0f, 0f, -leanTiltDegrees);
                    break;
                case DefenderState.Reaching:
                    targetPosition += Vector3.right * sideSign * reachLateralOffset + Vector3.back * reachForwardOffset;
                    targetRotation = neutralLocalRotation * Quaternion.Euler(reachTiltDegrees * .35f, 0f,
                        -sideSign * reachTiltDegrees);
                    break;
                case DefenderState.Overcommitted:
                    targetPosition += Vector3.right * sideSign * overcommitOffset + Vector3.back * reachForwardOffset;
                    targetRotation = neutralLocalRotation * Quaternion.Euler(reachTiltDegrees * .5f, 0f,
                        -sideSign * overcommitTiltDegrees);
                    break;
                case DefenderState.Beaten:
                    targetPosition += Vector3.right * sideSign * beatenOffset;
                    targetRotation = neutralLocalRotation * Quaternion.Euler(0f, 0f,
                        -sideSign * beatenTiltDegrees);
                    break;
            }

            float positionStep = Mathf.Max(.01f, leanOffset) * leanResponseSpeed * deltaTime;
            visualRoot.localPosition = Vector3.MoveTowards(visualRoot.localPosition, targetPosition, positionStep);
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetRotation,
                Mathf.Clamp01(leanResponseSpeed * deltaTime));
        }

        private void CacheNeutralPose()
        {
            if (visualRoot == null) return;
            neutralLocalPosition = visualRoot.localPosition;
            neutralLocalRotation = visualRoot.localRotation;
        }

        private void Subscribe()
        {
            if (subscribed || dribbleController == null) return;
            dribbleController.DribbleStarted += HandleDribbleStarted;
            dribbleController.FloorContactReached += HandleFloorContact;
            dribbleController.BallReturned += HandleBallReturned;
            dribbleController.PossessionRestarted += ResetDefenderState;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || dribbleController == null)
            {
                subscribed = false;
                return;
            }

            dribbleController.DribbleStarted -= HandleDribbleStarted;
            dribbleController.FloorContactReached -= HandleFloorContact;
            dribbleController.BallReturned -= HandleBallReturned;
            dribbleController.PossessionRestarted -= ResetDefenderState;
            subscribed = false;
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;
            GUI.Box(new Rect(Screen.width - 300f, 18f, 280f, 122f), "Prototype Defender");
            GUI.Label(new Rect(Screen.width - 285f, 45f, 250f, 22f), $"State: {currentState}");
            GUI.Label(new Rect(Screen.width - 285f, 67f, 250f, 22f),
                $"Steal: {currentStealOpportunity} / {lastReachOutcome}");
            GUI.Label(new Rect(Screen.width - 285f, 89f, 250f, 22f),
                currentState == DefenderState.Recovering
                    ? $"Recovery: {RecoveryRemaining:0.00}s"
                    : currentState == DefenderState.Overcommitted || currentState == DefenderState.Beaten
                        ? $"Window: {StateRemaining:0.00}s"
                        : "Window: —");
            GUI.Label(new Rect(Screen.width - 285f, 111f, 250f, 22f),
                $"Reach {reachAttemptCount}  Steals {stealCount}  Over {overcommitCount}  Beaten {beatenCount}");
        }
    }
}
