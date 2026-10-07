using UnityEngine;

namespace BounceTheory
{
    public enum DefenderState
    {
        Centered,
        LeaningLeft,
        LeaningRight,
        Recovering
    }

    /// <summary>
    /// Readable prototype defender reactions driven by authoritative dribble/gameplay state.
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

        [Header("Temporary Debug")]
        [SerializeField] private bool showDebugOverlay = true;

        private DefenderState currentState = DefenderState.Centered;
        private BallHand committedSide = BallHand.Left;
        private bool hasCommitment;
        private Vector3 neutralLocalPosition;
        private Quaternion neutralLocalRotation = Quaternion.identity;
        private Vector3 recoveryStartLocalPosition;
        private Quaternion recoveryStartLocalRotation = Quaternion.identity;
        private float recoveryDuration;
        private float recoveryElapsed;
        private bool subscribed;

        public DefenderState CurrentState => currentState;
        public float RecoveryDuration => recoveryDuration;
        public float RecoveryElapsed => recoveryElapsed;
        public float RecoveryRemaining => Mathf.Max(0f, recoveryDuration - recoveryElapsed);
        public float SecureRecoverySeconds => secureRecoverySeconds;
        public float RecoveringRecoverySeconds => recoveringRecoverySeconds;
        public float ExposedRecoverySeconds => exposedRecoverySeconds;
        public float HesitationRecoveryExtensionSeconds => hesitationRecoveryExtensionSeconds;
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
                }
                else
                {
                    float t = Mathf.SmoothStep(0f, 1f,
                        recoveryDuration <= .0001f ? 1f : recoveryElapsed / recoveryDuration);
                    visualRoot.localPosition = Vector3.Lerp(recoveryStartLocalPosition, neutralLocalPosition, t);
                    visualRoot.localRotation = Quaternion.Slerp(recoveryStartLocalRotation, neutralLocalRotation, t);
                }
                return;
            }

            Vector3 targetPosition = neutralLocalPosition;
            Quaternion targetRotation = neutralLocalRotation;
            if (currentState == DefenderState.LeaningLeft)
            {
                targetPosition += Vector3.left * leanOffset;
                targetRotation = neutralLocalRotation * Quaternion.Euler(0f, 0f, leanTiltDegrees);
            }
            else if (currentState == DefenderState.LeaningRight)
            {
                targetPosition += Vector3.right * leanOffset;
                targetRotation = neutralLocalRotation * Quaternion.Euler(0f, 0f, -leanTiltDegrees);
            }

            float positionStep = leanOffset * leanResponseSpeed * deltaTime;
            visualRoot.localPosition = Vector3.MoveTowards(visualRoot.localPosition, targetPosition, positionStep);
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetRotation,
                Mathf.Clamp01(leanResponseSpeed * deltaTime));
        }

        public void ResetDefenderState()
        {
            currentState = DefenderState.Centered;
            hasCommitment = false;
            recoveryDuration = 0f;
            recoveryElapsed = 0f;
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

        private void HandleDribbleStarted(BallHand sourceHand)
        {
            if (dribbleController == null ||
                dribbleController.CurrentPossessionState != PossessionState.Active)
                return;

            if (currentState == DefenderState.Recovering)
                return;

            committedSide = sourceHand;
            hasCommitment = true;
            currentState = sourceHand == BallHand.Left
                ? DefenderState.LeaningLeft
                : DefenderState.LeaningRight;
        }

        private void HandleDribbleActionAccepted(DribbleAction action)
        {
            if (dribbleController == null ||
                dribbleController.CurrentPossessionState != PossessionState.Active)
                return;

            if (currentState == DefenderState.Recovering && action == DribbleAction.Hesitation)
                ExtendRecoveryForHesitation();
        }

        private void HandleFloorContact(BallHand resolvedHand)
        {
            if (dribbleController == null || !hasCommitment) return;

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

        private void BeginRecovery(BallControlQuality offensiveQuality)
        {
            currentState = DefenderState.Recovering;
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

        private void SetCentered()
        {
            currentState = DefenderState.Centered;
            hasCommitment = false;
            recoveryDuration = 0f;
            recoveryElapsed = 0f;
            if (visualRoot != null)
            {
                visualRoot.localPosition = neutralLocalPosition;
                visualRoot.localRotation = neutralLocalRotation;
            }
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
            dribbleController.DribbleActionAccepted += HandleDribbleActionAccepted;
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
            dribbleController.DribbleActionAccepted -= HandleDribbleActionAccepted;
            dribbleController.FloorContactReached -= HandleFloorContact;
            dribbleController.BallReturned -= HandleBallReturned;
            dribbleController.PossessionRestarted -= ResetDefenderState;
            subscribed = false;
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;
            GUI.Box(new Rect(Screen.width - 275f, 18f, 255f, 78f), "Prototype Defender");
            GUI.Label(new Rect(Screen.width - 260f, 45f, 225f, 22f), $"State: {currentState}");
            GUI.Label(new Rect(Screen.width - 260f, 67f, 225f, 22f),
                currentState == DefenderState.Recovering
                    ? $"Recovery: {RecoveryRemaining:0.00}s"
                    : "Recovery: —");
        }
    }
}
