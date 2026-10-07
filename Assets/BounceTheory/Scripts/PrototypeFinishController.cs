using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BounceTheory
{
    public enum FinishType
    {
        None,
        Shot,
        Stepback,
        Drive
    }

    public enum FinishPhase
    {
        Idle,
        Timing,
        Resolved
    }

    public enum FinishWindowTier
    {
        Tight,
        Medium,
        Wide
    }

    public enum FinishOutcome
    {
        None,
        GreenMade,
        EarlyMiss,
        LateMiss
    }

    /// <summary>
    /// Prototype finish timing layer. A finish is committed with Q/E/F and released by pressing
    /// the same key again. The timing target is snapped to the shared rhythm clock; defender
    /// state captured at commit time determines the green-window size.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeFinishController : MonoBehaviour
    {
        [Header("Gameplay Sources")]
        [SerializeField] private PoundDribbleController dribbleController;
        [SerializeField] private RhythmClock rhythmClock;
        [SerializeField] private PrototypeDefenderController defenderController;

        [Header("Finish Timing")]
        [SerializeField, Min(.05f)] private float shotMinimumLeadSeconds = .55f;
        [SerializeField, Min(.05f)] private float stepbackMinimumLeadSeconds = .70f;
        [SerializeField, Min(.05f)] private float driveMinimumLeadSeconds = .50f;

        [Header("Green Half-Windows")]
        [SerializeField, Min(1f)] private float tightHalfWindowMilliseconds = 70f;
        [SerializeField, Min(1f)] private float mediumHalfWindowMilliseconds = 120f;
        [SerializeField, Min(1f)] private float wideHalfWindowMilliseconds = 180f;

        [Header("Temporary Presentation")]
        [SerializeField] private GameObject timingCue;
        [SerializeField, Min(.01f)] private float cueMinimumScale = .16f;
        [SerializeField, Min(.01f)] private float cueMaximumScale = .42f;
        [SerializeField] private bool showDebugOverlay = true;

        private FinishPhase phase = FinishPhase.Idle;
        private FinishType activeFinish = FinishType.None;
        private FinishWindowTier activeWindowTier = FinishWindowTier.Tight;
        private FinishOutcome lastOutcome = FinishOutcome.None;
        private DefenderState defenderStateAtCommit = DefenderState.Centered;
        private double commitDsp;
        private double targetDsp;
        private double releaseDsp;
        private double releaseErrorMilliseconds;
        private float greenHalfWindowSeconds;
        private int attemptCount;
        private int madeCount;
        private int missCount;
        private bool subscribed;

        public FinishPhase Phase => phase;
        public FinishType ActiveFinish => activeFinish;
        public FinishWindowTier ActiveWindowTier => activeWindowTier;
        public FinishOutcome LastOutcome => lastOutcome;
        public DefenderState DefenderStateAtCommit => defenderStateAtCommit;
        public double CommitDsp => commitDsp;
        public double TargetDsp => targetDsp;
        public double ReleaseDsp => releaseDsp;
        public double ReleaseErrorMilliseconds => releaseErrorMilliseconds;
        public float GreenHalfWindowSeconds => greenHalfWindowSeconds;
        public float TightHalfWindowMilliseconds => tightHalfWindowMilliseconds;
        public float MediumHalfWindowMilliseconds => mediumHalfWindowMilliseconds;
        public float WideHalfWindowMilliseconds => wideHalfWindowMilliseconds;
        public int AttemptCount => attemptCount;
        public int MadeCount => madeCount;
        public int MissCount => missCount;
        public bool IsTiming => phase == FinishPhase.Timing;

        private void Awake()
        {
            ResetFinishState();
        }

        private void OnEnable()
        {
            Subscribe();
            SetCue(false);
        }

        private void OnDisable()
        {
            Unsubscribe();
            if (dribbleController != null && phase == FinishPhase.Timing)
                dribbleController.SetGameplayInputSuppressed(false);
        }

        private void OnValidate()
        {
            shotMinimumLeadSeconds = Mathf.Max(.05f, shotMinimumLeadSeconds);
            stepbackMinimumLeadSeconds = Mathf.Max(.05f, stepbackMinimumLeadSeconds);
            driveMinimumLeadSeconds = Mathf.Max(.05f, driveMinimumLeadSeconds);
            tightHalfWindowMilliseconds = Mathf.Max(1f, tightHalfWindowMilliseconds);
            mediumHalfWindowMilliseconds = Mathf.Max(tightHalfWindowMilliseconds, mediumHalfWindowMilliseconds);
            wideHalfWindowMilliseconds = Mathf.Max(mediumHalfWindowMilliseconds, wideHalfWindowMilliseconds);
            cueMinimumScale = Mathf.Max(.01f, cueMinimumScale);
            cueMaximumScale = Mathf.Max(cueMinimumScale, cueMaximumScale);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (phase == FinishPhase.Idle)
                {
                    if (keyboard.qKey.wasPressedThisFrame) TryStartFinish(FinishType.Shot);
                    else if (keyboard.eKey.wasPressedThisFrame) TryStartFinish(FinishType.Stepback);
                    else if (keyboard.fKey.wasPressedThisFrame) TryStartFinish(FinishType.Drive);
                }
                else if (phase == FinishPhase.Timing)
                {
                    bool releasePressed =
                        (activeFinish == FinishType.Shot && keyboard.qKey.wasPressedThisFrame) ||
                        (activeFinish == FinishType.Stepback && keyboard.eKey.wasPressedThisFrame) ||
                        (activeFinish == FinishType.Drive && keyboard.fKey.wasPressedThisFrame);
                    if (releasePressed) ResolveFinish();
                }
            }

            TickAtDspTime(AudioSettings.dspTime);
        }

        public void Configure(PoundDribbleController dribble, RhythmClock clock,
            PrototypeDefenderController defender, GameObject cue)
        {
            Unsubscribe();
            dribbleController = dribble;
            rhythmClock = clock;
            defenderController = defender;
            timingCue = cue;
            ResetFinishState();
            Subscribe();
        }

        public bool TryStartFinish(FinishType finish)
        {
            return TryStartFinishAtDspTime(finish, AudioSettings.dspTime);
        }

        public bool TryStartFinishAtElapsedTime(FinishType finish, double elapsedSeconds)
        {
            double dsp = rhythmClock != null
                ? rhythmClock.StartDspTime + Math.Max(0, elapsedSeconds)
                : Math.Max(0, elapsedSeconds);
            return TryStartFinishAtDspTime(finish, dsp);
        }

        public bool TryStartFinishAtDspTime(FinishType finish, double dsp)
        {
            if (finish == FinishType.None || phase != FinishPhase.Idle || dribbleController == null)
                return false;
            if (dribbleController.CurrentPossessionState != PossessionState.Active ||
                dribbleController.LogicalPhase != BallLogicalPhase.Controlled ||
                dribbleController.HasPendingInput ||
                dribbleController.HasPendingPlainSpace ||
                dribbleController.GameplayInputSuppressed)
                return false;

            activeFinish = finish;
            phase = FinishPhase.Timing;
            lastOutcome = FinishOutcome.None;
            commitDsp = dsp;
            releaseDsp = 0;
            releaseErrorMilliseconds = 0;
            defenderStateAtCommit = defenderController != null
                ? defenderController.CurrentState
                : DefenderState.Centered;
            activeWindowTier = WindowTierForDefenderState(defenderStateAtCommit);
            greenHalfWindowSeconds = HalfWindowMillisecondsFor(activeWindowTier) / 1000f;
            targetDsp = CalculateTargetDsp(dsp, MinimumLeadSecondsFor(finish));
            attemptCount++;
            dribbleController.SetGameplayInputSuppressed(true);
            SetCue(true);
            UpdateCue(dsp);
            return true;
        }

        public bool ResolveFinish()
        {
            return ResolveFinishAtDspTime(AudioSettings.dspTime);
        }

        public bool ResolveFinishAtElapsedTime(double elapsedSeconds)
        {
            double dsp = rhythmClock != null
                ? rhythmClock.StartDspTime + Math.Max(0, elapsedSeconds)
                : Math.Max(0, elapsedSeconds);
            return ResolveFinishAtDspTime(dsp);
        }

        public bool ResolveFinishAtDspTime(double dsp)
        {
            if (phase != FinishPhase.Timing || dribbleController == null)
                return false;

            releaseDsp = dsp;
            double errorSeconds = dsp - targetDsp;
            releaseErrorMilliseconds = errorSeconds * 1000.0;

            if (Math.Abs(errorSeconds) <= greenHalfWindowSeconds)
            {
                lastOutcome = FinishOutcome.GreenMade;
                madeCount++;
                CompleteFinish(PossessionEndReason.FinishMade);
            }
            else
            {
                lastOutcome = errorSeconds < 0 ? FinishOutcome.EarlyMiss : FinishOutcome.LateMiss;
                missCount++;
                CompleteFinish(PossessionEndReason.FinishMissed);
            }

            return true;
        }

        public void TickAtDspTime(double dsp)
        {
            if (phase != FinishPhase.Timing) return;

            UpdateCue(dsp);
            if (dsp > targetDsp + greenHalfWindowSeconds)
                ResolveFinishAtDspTime(dsp);
        }

        public static FinishWindowTier WindowTierForDefenderState(DefenderState state)
        {
            switch (state)
            {
                case DefenderState.Beaten:
                    return FinishWindowTier.Wide;
                case DefenderState.Recovering:
                case DefenderState.Overcommitted:
                    return FinishWindowTier.Medium;
                default:
                    return FinishWindowTier.Tight;
            }
        }

        public float HalfWindowMillisecondsFor(FinishWindowTier tier)
        {
            switch (tier)
            {
                case FinishWindowTier.Wide:
                    return wideHalfWindowMilliseconds;
                case FinishWindowTier.Medium:
                    return mediumHalfWindowMilliseconds;
                default:
                    return tightHalfWindowMilliseconds;
            }
        }

        public void ResetFinishState()
        {
            phase = FinishPhase.Idle;
            activeFinish = FinishType.None;
            activeWindowTier = FinishWindowTier.Tight;
            lastOutcome = FinishOutcome.None;
            defenderStateAtCommit = DefenderState.Centered;
            commitDsp = 0;
            targetDsp = 0;
            releaseDsp = 0;
            releaseErrorMilliseconds = 0;
            greenHalfWindowSeconds = 0f;
            if (dribbleController != null)
                dribbleController.SetGameplayInputSuppressed(false);
            SetCue(false);
        }

        private void CompleteFinish(PossessionEndReason endReason)
        {
            phase = FinishPhase.Resolved;
            dribbleController.SetGameplayInputSuppressed(false);
            SetCue(false);
            dribbleController.EndPossession(endReason);
        }

        private double CalculateTargetDsp(double startDsp, float minimumLeadSeconds)
        {
            if (rhythmClock == null)
                return startDsp + minimumLeadSeconds;

            double startBeat = rhythmClock.BeatPositionAtDspTime(startDsp);
            double minimumTargetBeat = startBeat + minimumLeadSeconds / rhythmClock.SecondsPerBeat;
            double targetBeat = Math.Ceiling(minimumTargetBeat - 1e-9);
            return rhythmClock.DspTimeForBeat(targetBeat);
        }

        private float MinimumLeadSecondsFor(FinishType finish)
        {
            switch (finish)
            {
                case FinishType.Stepback:
                    return stepbackMinimumLeadSeconds;
                case FinishType.Drive:
                    return driveMinimumLeadSeconds;
                default:
                    return shotMinimumLeadSeconds;
            }
        }

        private void UpdateCue(double dsp)
        {
            if (timingCue == null || phase != FinishPhase.Timing) return;
            double totalLead = Math.Max(.001, targetDsp - commitDsp);
            float progress = Mathf.Clamp01((float)((dsp - commitDsp) / totalLead));
            float scale = Mathf.Lerp(cueMinimumScale, cueMaximumScale, progress);
            timingCue.transform.localScale = Vector3.one * scale;
        }

        private void SetCue(bool visible)
        {
            if (timingCue != null && timingCue.activeSelf != visible)
                timingCue.SetActive(visible);
        }

        private void Subscribe()
        {
            if (subscribed || dribbleController == null) return;
            dribbleController.PossessionRestarted += ResetFinishState;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || dribbleController == null)
            {
                subscribed = false;
                return;
            }

            dribbleController.PossessionRestarted -= ResetFinishState;
            subscribed = false;
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;

            const float width = 390f;
            const float height = 128f;
            float x = (Screen.width - width) * .5f;
            GUI.Box(new Rect(x, 18f, width, height), "Prototype Finish");

            if (phase == FinishPhase.Idle)
            {
                GUI.Label(new Rect(x + 14f, 45f, width - 28f, 22f),
                    "Q Shot   E Stepback   F Drive");
                GUI.Label(new Rect(x + 14f, 69f, width - 28f, 44f),
                    "Press a finish key to commit. Press the same key again to release on the green window.");
                return;
            }

            GUI.Label(new Rect(x + 14f, 45f, width - 28f, 22f),
                $"Finish: {activeFinish}   Phase: {phase}   Window: {activeWindowTier}");
            GUI.Label(new Rect(x + 14f, 67f, width - 28f, 22f),
                $"Defender at commit: {defenderStateAtCommit}   ±{greenHalfWindowSeconds * 1000f:0} ms");
            GUI.Label(new Rect(x + 14f, 89f, width - 28f, 22f),
                phase == FinishPhase.Timing
                    ? $"Release with {KeyLabel(activeFinish)} on the target beat."
                    : $"Outcome: {lastOutcome}   Error: {releaseErrorMilliseconds:+0;-0;0} ms");
            GUI.Label(new Rect(x + 14f, 111f, width - 28f, 22f),
                $"Attempts {attemptCount}   Made {madeCount}   Missed {missCount}   R = restart");
        }

        private static string KeyLabel(FinishType finish)
        {
            switch (finish)
            {
                case FinishType.Stepback: return "E";
                case FinishType.Drive: return "F";
                default: return "Q";
            }
        }
    }
}
