using System;
using UnityEngine;

namespace BounceTheory
{
    public enum TimingResult { None, Perfect, Good, Early, Late, BrokenRhythm }
    public enum TimingDirection { OnBeat, Early, Late }
    public enum TimingDifficulty { Easy }
    public enum RhythmicIntervalKind { None, HalfBeat, ThreeQuarterBeat, OneBeat, OneAndHalfBeats, TwoBeats }

    public static class RhythmicIntervalCatalog
    {
        public static readonly RhythmicIntervalKind[] Ordered =
        {
            RhythmicIntervalKind.HalfBeat,
            RhythmicIntervalKind.ThreeQuarterBeat,
            RhythmicIntervalKind.OneBeat,
            RhythmicIntervalKind.OneAndHalfBeats,
            RhythmicIntervalKind.TwoBeats
        };

        public static double Beats(RhythmicIntervalKind interval)
        {
            switch (interval)
            {
                case RhythmicIntervalKind.HalfBeat: return 0.5;
                case RhythmicIntervalKind.ThreeQuarterBeat: return 0.75;
                case RhythmicIntervalKind.OneBeat: return 1.0;
                case RhythmicIntervalKind.OneAndHalfBeats: return 1.5;
                case RhythmicIntervalKind.TwoBeats: return 2.0;
                default: return 0.0;
            }
        }

        public static string Label(RhythmicIntervalKind interval)
        {
            return interval == RhythmicIntervalKind.None ? "None" : Beats(interval).ToString("0.##") + " beats";
        }
    }

    [Serializable]
    public struct TimingWindowProfile
    {
        [Min(1f)] public float perfectMilliseconds;
        [Min(1f)] public float goodEarlyMilliseconds;
        [Min(1f)] public float goodLateMilliseconds;
        [Min(1f)] public float maximumEarlyMilliseconds;
        [Min(1f)] public float maximumLateMilliseconds;

        public static TimingWindowProfile EasyDefault => new TimingWindowProfile
        {
            perfectMilliseconds = 18f,
            goodEarlyMilliseconds = 34f,
            goodLateMilliseconds = 34f,
            maximumEarlyMilliseconds = 52f,
            maximumLateMilliseconds = 52f
        };

        public void Clamp()
        {
            perfectMilliseconds = Mathf.Max(1f, perfectMilliseconds);
            goodEarlyMilliseconds = Mathf.Max(perfectMilliseconds, goodEarlyMilliseconds);
            goodLateMilliseconds = Mathf.Max(perfectMilliseconds, goodLateMilliseconds);
            maximumEarlyMilliseconds = Mathf.Max(goodEarlyMilliseconds, maximumEarlyMilliseconds);
            maximumLateMilliseconds = Mathf.Max(goodLateMilliseconds, maximumLateMilliseconds);
        }
    }

    public readonly struct TimingJudgment
    {
        public static TimingJudgment None => new TimingJudgment(TimingResult.None, TimingDirection.OnBeat, 0, 0, 0, 0, 0, RhythmicIntervalKind.None);

        public TimingResult Result { get; }
        public TimingDirection Direction { get; }
        public double ErrorSeconds { get; }
        public double ErrorMilliseconds => ErrorSeconds * 1000.0;
        public long NearestBeatIndex { get; }
        public double BeatPosition { get; }
        public double SampleElapsedSeconds { get; }
        public double TargetElapsedSeconds { get; }
        public double TargetBeatPosition { get; }
        public RhythmicIntervalKind Interval { get; }
        public bool HasRhythmicInterval => Interval != RhythmicIntervalKind.None;
        public bool IsValid => Result != TimingResult.None;

        public TimingJudgment(TimingResult result, TimingDirection direction, double errorSeconds, long nearestBeatIndex, double beatPosition)
            : this(result, direction, errorSeconds, nearestBeatIndex, beatPosition, 0, nearestBeatIndex, RhythmicIntervalKind.None) { }

        public TimingJudgment(TimingResult result, TimingDirection direction, double errorSeconds, long nearestBeatIndex,
            double beatPosition, double sampleElapsedSeconds, double targetBeatPosition, RhythmicIntervalKind interval)
        {
            Result = result;
            Direction = direction;
            ErrorSeconds = errorSeconds;
            NearestBeatIndex = nearestBeatIndex;
            BeatPosition = beatPosition;
            SampleElapsedSeconds = sampleElapsedSeconds;
            TargetBeatPosition = targetBeatPosition;
            TargetElapsedSeconds = targetBeatPosition <= 0 && nearestBeatIndex == 0 ? 0 : sampleElapsedSeconds - errorSeconds;
            Interval = interval;
        }
    }

    public readonly struct ContactTimingPlan
    {
        public TimingJudgment Judgment { get; }
        public RhythmicIntervalKind Interval { get; }
        public double InputElapsedSeconds { get; }
        public double InputDspTimestamp { get; }
        public double TargetContactBeat { get; }
        public double TargetContactElapsedSeconds { get; }
        public double TargetContactDspTimestamp { get; }
        public double ProjectedContactElapsedSeconds { get; }
        public bool HasIntervalContext => Interval != RhythmicIntervalKind.None;

        public ContactTimingPlan(TimingJudgment judgment, RhythmicIntervalKind interval, double inputElapsed,
            double inputDsp, double targetBeat, double targetElapsed, double targetDsp, double projectedContactElapsed)
        {
            Judgment = judgment;
            Interval = interval;
            InputElapsedSeconds = inputElapsed;
            InputDspTimestamp = inputDsp;
            TargetContactBeat = targetBeat;
            TargetContactElapsedSeconds = targetElapsed;
            TargetContactDspTimestamp = targetDsp;
            ProjectedContactElapsedSeconds = projectedContactElapsed;
        }
    }

    /// <summary>
    /// Shared DSP-time rhythm source. Ball-event history is snapped to the global quarter-beat
    /// grid before interval targets are built, preventing small player errors from accumulating.
    /// </summary>
    public sealed class RhythmClock : MonoBehaviour
    {
        [Header("Global Rhythm Grid")]
        [SerializeField, Range(30f, 240f)] private float bpm = 130f;
        [SerializeField, Range(1, 16)] private int subdivisionsPerBeat = 4;

        [Header("Timing Difficulty")]
        [SerializeField] private TimingDifficulty activeDifficulty = TimingDifficulty.Easy;
        [SerializeField] private TimingWindowProfile easyTiming = default;
        [SerializeField, Min(0f)] private float ambiguityGuardMilliseconds = 4f;

        private double startDspTime;
        private bool running;
        private bool hasPreviousBallEvent;
        private double previousBallEventElapsedSeconds;
        private double previousBallEventAlignedBeat;

        public float Bpm => bpm;
        public int SubdivisionsPerBeat => subdivisionsPerBeat;
        public double SecondsPerBeat => 60.0 / bpm;
        public double StartDspTime => startDspTime;
        public double ElapsedSeconds => running ? Math.Max(0.0, AudioSettings.dspTime - startDspTime) : 0.0;
        public double CurrentBeatPosition => ElapsedSeconds / SecondsPerBeat;
        public long CurrentBeatNumber => (long)Math.Floor(CurrentBeatPosition);
        public double CurrentBeatPhase => CurrentBeatPosition - Math.Floor(CurrentBeatPosition);
        public int CurrentSubdivision => Math.Min(subdivisionsPerBeat - 1, (int)Math.Floor(CurrentBeatPhase * subdivisionsPerBeat));
        public TimingDifficulty ActiveDifficulty => activeDifficulty;
        public TimingWindowProfile EasyTiming => easyTiming;
        public float AmbiguityGuardMilliseconds => ambiguityGuardMilliseconds;
        public bool HasPreviousBallEvent => hasPreviousBallEvent;
        public double PreviousBallEventElapsedSeconds => previousBallEventElapsedSeconds;
        public double PreviousBallEventAlignedBeat => previousBallEventAlignedBeat;
        public double EffectiveMaximumIntervalWindowMilliseconds => EffectiveMaximumIntervalWindowMs();

        private void Awake()
        {
            EnsureDefaults();
            RestartClock();
        }

        private void OnEnable()
        {
            if (!running) RestartClock();
        }

        private void OnValidate()
        {
            EnsureDefaults();
            bpm = Mathf.Clamp(bpm, 30f, 240f);
            subdivisionsPerBeat = Mathf.Clamp(subdivisionsPerBeat, 1, 16);
            ambiguityGuardMilliseconds = Mathf.Max(0f, ambiguityGuardMilliseconds);
            if (Application.isPlaying) RestartClock();
        }

        public void SetBpm(float value)
        {
            bpm = Mathf.Clamp(value, 30f, 240f);
            if (Application.isPlaying) RestartClock();
        }

        public void RestartClock()
        {
            startDspTime = AudioSettings.dspTime;
            running = true;
            ResetBallEventHistory();
        }

        public void ResetBallEventHistory()
        {
            hasPreviousBallEvent = false;
            previousBallEventElapsedSeconds = 0;
            previousBallEventAlignedBeat = 0;
        }

        public double DspTimeForBeat(double beatPosition) => startDspTime + beatPosition * SecondsPerBeat;
        public double BeatPositionAtDspTime(double dspTime) => Math.Max(0, dspTime - startDspTime) / SecondsPerBeat;
        public double ElapsedSecondsAtDspTime(double dspTime) => Math.Max(0, dspTime - startDspTime);

        public TimingJudgment JudgeNow() => JudgeAtElapsedTime(ElapsedSeconds);
        public TimingJudgment JudgeNextIntervalNow() => JudgeNextIntervalAtElapsedTime(ElapsedSeconds);

        public ContactTimingPlan PlanNextContactAtDspTime(double inputDspTimestamp, double referenceDescentSeconds,
            double minimumApproachSeconds, bool preferUpcomingHalfBeat)
        {
            return PlanNextContactAtElapsedTime(ElapsedSecondsAtDspTime(inputDspTimestamp), inputDspTimestamp,
                referenceDescentSeconds, minimumApproachSeconds, preferUpcomingHalfBeat);
        }

        public ContactTimingPlan PlanNextContactAtElapsedTime(double inputElapsedSeconds, double inputDspTimestamp,
            double referenceDescentSeconds, double minimumApproachSeconds, bool preferUpcomingHalfBeat,
            double referenceContactBeatOverride = double.NaN)
        {
            inputElapsedSeconds = Math.Max(0, inputElapsedSeconds);
            referenceDescentSeconds = Math.Max(.01, referenceDescentSeconds);
            minimumApproachSeconds = Math.Max(.01, minimumApproachSeconds);
            double projectedContact = inputElapsedSeconds + referenceDescentSeconds;
            RhythmicIntervalKind interval = RhythmicIntervalKind.None;
            double targetBeat;

            bool hasReference = !double.IsNaN(referenceContactBeatOverride) || hasPreviousBallEvent;
            double referenceBeat = !double.IsNaN(referenceContactBeatOverride)
                ? referenceContactBeatOverride
                : previousBallEventAlignedBeat;
            if (!hasReference)
            {
                interval = RhythmicIntervalKind.OneBeat;
                targetBeat = Math.Max(1, Math.Round(projectedContact / SecondsPerBeat, MidpointRounding.AwayFromZero));
            }
            else
            {
                interval = RhythmicIntervalCatalog.Ordered[0];
                targetBeat = referenceBeat + RhythmicIntervalCatalog.Beats(interval);

                double halfRemaining = targetBeat * SecondsPerBeat - inputElapsedSeconds;
                if (!(preferUpcomingHalfBeat && halfRemaining >= minimumApproachSeconds))
                {
                    double bestDistance = Math.Abs(projectedContact - targetBeat * SecondsPerBeat);
                    for (int i = 1; i < RhythmicIntervalCatalog.Ordered.Length; i++)
                    {
                        RhythmicIntervalKind candidate = RhythmicIntervalCatalog.Ordered[i];
                        double candidateBeat = referenceBeat + RhythmicIntervalCatalog.Beats(candidate);
                        double distance = Math.Abs(projectedContact - candidateBeat * SecondsPerBeat);
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            interval = candidate;
                            targetBeat = candidateBeat;
                        }
                    }
                }
            }

            double targetElapsed = targetBeat * SecondsPerBeat;
            double targetDsp = startDspTime + targetElapsed;
            double commandError = projectedContact - targetElapsed;
            TimingJudgment judgment = EvaluateCommand(inputElapsedSeconds, commandError, targetBeat, interval);
            return new ContactTimingPlan(judgment, interval, inputElapsedSeconds, inputDspTimestamp,
                targetBeat, targetElapsed, targetDsp, projectedContact);
        }

        public TimingJudgment JudgeAtElapsedTime(double elapsedSeconds)
        {
            elapsedSeconds = Math.Max(0, elapsedSeconds);
            double beatPosition = elapsedSeconds / SecondsPerBeat;
            long nearestBeat = (long)Math.Round(beatPosition, MidpointRounding.AwayFromZero);
            return Evaluate(elapsedSeconds, beatPosition, nearestBeat, nearestBeat, RhythmicIntervalKind.None, false);
        }

        public TimingJudgment JudgeNextIntervalAtElapsedTime(double elapsedSeconds)
        {
            if (!hasPreviousBallEvent) return JudgeAtElapsedTime(elapsedSeconds);

            elapsedSeconds = Math.Max(0, elapsedSeconds);
            double beatPosition = elapsedSeconds / SecondsPerBeat;
            RhythmicIntervalKind bestInterval = RhythmicIntervalCatalog.Ordered[0];
            double bestTargetBeat = previousBallEventAlignedBeat + RhythmicIntervalCatalog.Beats(bestInterval);
            double bestDistance = Math.Abs(beatPosition - bestTargetBeat);

            for (int i = 1; i < RhythmicIntervalCatalog.Ordered.Length; i++)
            {
                RhythmicIntervalKind candidate = RhythmicIntervalCatalog.Ordered[i];
                double candidateBeat = previousBallEventAlignedBeat + RhythmicIntervalCatalog.Beats(candidate);
                double distance = Math.Abs(beatPosition - candidateBeat);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTargetBeat = candidateBeat;
                    bestInterval = candidate;
                }
            }

            return Evaluate(elapsedSeconds, beatPosition, (long)Math.Round(bestTargetBeat), bestTargetBeat, bestInterval, true);
        }

        public void RegisterBallEventNow() => RegisterBallEventAtElapsedTime(ElapsedSeconds);

        public void RegisterBallEventAtElapsedTime(double elapsedSeconds)
        {
            previousBallEventElapsedSeconds = Math.Max(0, elapsedSeconds);
            double rawBeat = previousBallEventElapsedSeconds / SecondsPerBeat;
            previousBallEventAlignedBeat = Math.Round(rawBeat * subdivisionsPerBeat, MidpointRounding.AwayFromZero) / subdivisionsPerBeat;
            hasPreviousBallEvent = true;
        }

        private TimingJudgment Evaluate(double elapsed, double beatPosition, long nearestBeat, double targetBeat,
            RhythmicIntervalKind interval, bool guardAmbiguity)
        {
            double targetElapsed = targetBeat * SecondsPerBeat;
            double error = elapsed - targetElapsed;
            double errorMs = error * 1000.0;
            double absoluteMs = Math.Abs(errorMs);
            double maxEarly = guardAmbiguity ? Math.Min(easyTiming.maximumEarlyMilliseconds, EffectiveMaximumIntervalWindowMs()) : easyTiming.maximumEarlyMilliseconds;
            double maxLate = guardAmbiguity ? Math.Min(easyTiming.maximumLateMilliseconds, EffectiveMaximumIntervalWindowMs()) : easyTiming.maximumLateMilliseconds;
            TimingDirection direction = absoluteMs < 0.0001 ? TimingDirection.OnBeat : error < 0 ? TimingDirection.Early : TimingDirection.Late;
            TimingResult result;

            if (absoluteMs <= easyTiming.perfectMilliseconds) result = TimingResult.Perfect;
            else if (error < 0 && absoluteMs <= easyTiming.goodEarlyMilliseconds) result = TimingResult.Good;
            else if (error > 0 && absoluteMs <= easyTiming.goodLateMilliseconds) result = TimingResult.Good;
            else if (error < 0 && absoluteMs <= maxEarly) result = TimingResult.Early;
            else if (error > 0 && absoluteMs <= maxLate) result = TimingResult.Late;
            else result = TimingResult.BrokenRhythm;

            return new TimingJudgment(result, direction, error, nearestBeat, beatPosition, elapsed, targetBeat, interval);
        }

        private TimingJudgment EvaluateCommand(double inputElapsed, double commandError, double targetContactBeat,
            RhythmicIntervalKind interval)
        {
            double errorMs = commandError * 1000.0;
            double absoluteMs = Math.Abs(errorMs);
            TimingDirection direction = absoluteMs < .0001 ? TimingDirection.OnBeat : commandError < 0 ? TimingDirection.Early : TimingDirection.Late;
            TimingResult result;
            if (absoluteMs <= easyTiming.perfectMilliseconds) result = TimingResult.Perfect;
            else if (commandError < 0 && absoluteMs <= easyTiming.goodEarlyMilliseconds) result = TimingResult.Good;
            else if (commandError > 0 && absoluteMs <= easyTiming.goodLateMilliseconds) result = TimingResult.Good;
            else if (commandError < 0 && absoluteMs <= easyTiming.maximumEarlyMilliseconds) result = TimingResult.Early;
            else if (commandError > 0 && absoluteMs <= easyTiming.maximumLateMilliseconds) result = TimingResult.Late;
            else result = TimingResult.BrokenRhythm;

            double inputBeat = inputElapsed / SecondsPerBeat;
            return new TimingJudgment(result, direction, commandError, (long)Math.Round(targetContactBeat), inputBeat,
                inputElapsed, targetContactBeat, interval);
        }

        private double EffectiveMaximumIntervalWindowMs()
        {
            const double closestTargetSpacingBeats = 0.25;
            return Math.Max(0, closestTargetSpacingBeats * SecondsPerBeat * 500.0 - ambiguityGuardMilliseconds);
        }

        private void EnsureDefaults()
        {
            if (easyTiming.perfectMilliseconds <= 0) easyTiming = TimingWindowProfile.EasyDefault;
            easyTiming.Clamp();
        }
    }
}
