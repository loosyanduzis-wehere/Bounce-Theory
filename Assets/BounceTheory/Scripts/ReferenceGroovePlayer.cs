using UnityEngine;

namespace BounceTheory
{
    /// <summary>Schedules a quiet, non-musical reference pulse from the shared DSP rhythm grid.</summary>
    public sealed class ReferenceGroovePlayer : MonoBehaviour
    {
        [SerializeField] private RhythmClock rhythmClock;
        [SerializeField] private AudioSource firstVoice;
        [SerializeField] private AudioSource secondVoice;
        [SerializeField] private AudioClip referencePulse;
        [SerializeField, Range(0f, 1f)] private float volume = 0.16f;
        [SerializeField] private bool muted;
        [SerializeField, Range(0.05f, 0.5f)] private float scheduleAheadSeconds = 0.15f;

        private long nextBeatToSchedule;
        private bool useFirstVoice;
        private double observedClockStart;
        private float observedBpm;

        public RhythmClock RhythmClock => rhythmClock;
        public AudioClip ReferencePulse => referencePulse;
        public float Volume => volume;
        public bool Muted => muted;
        public float ScheduleAheadSeconds => scheduleAheadSeconds;

        private void OnEnable() => Synchronize();

        private void Update()
        {
            if (rhythmClock == null || referencePulse == null || firstVoice == null || secondVoice == null) return;
            if (observedClockStart != rhythmClock.StartDspTime || !Mathf.Approximately(observedBpm, rhythmClock.Bpm)) Synchronize();

            double scheduleLimit = AudioSettings.dspTime + scheduleAheadSeconds;
            while (rhythmClock.DspTimeForBeat(nextBeatToSchedule) <= scheduleLimit)
            {
                double dspTime = rhythmClock.DspTimeForBeat(nextBeatToSchedule);
                if (dspTime > AudioSettings.dspTime + 0.005) SchedulePulse(dspTime);
                nextBeatToSchedule++;
            }
        }

        public void Configure(RhythmClock clock, AudioSource first, AudioSource second, AudioClip pulse)
        {
            rhythmClock = clock;
            firstVoice = first;
            secondVoice = second;
            referencePulse = pulse;
            ConfigureVoice(firstVoice);
            ConfigureVoice(secondVoice);
            Synchronize();
        }

        public void SetMuted(bool value)
        {
            muted = value;
            ApplyVolume();
        }

        public void SetVolume(float value)
        {
            volume = Mathf.Clamp01(value);
            ApplyVolume();
        }

        public void Synchronize()
        {
            if (firstVoice != null) firstVoice.Stop();
            if (secondVoice != null) secondVoice.Stop();
            if (rhythmClock == null) return;
            observedClockStart = rhythmClock.StartDspTime;
            observedBpm = rhythmClock.Bpm;
            nextBeatToSchedule = (long)System.Math.Ceiling(rhythmClock.CurrentBeatPosition);
            useFirstVoice = true;
            ApplyVolume();
        }

        private void SchedulePulse(double dspTime)
        {
            AudioSource voice = useFirstVoice ? firstVoice : secondVoice;
            useFirstVoice = !useFirstVoice;
            voice.clip = referencePulse;
            voice.PlayScheduled(dspTime);
        }

        private void ApplyVolume()
        {
            float applied = muted ? 0f : volume;
            if (firstVoice != null) firstVoice.volume = applied;
            if (secondVoice != null) secondVoice.volume = applied;
        }

        private static void ConfigureVoice(AudioSource voice)
        {
            if (voice == null) return;
            voice.playOnAwake = false;
            voice.loop = false;
            voice.spatialBlend = 0f;
            voice.dopplerLevel = 0f;
        }
    }
}
