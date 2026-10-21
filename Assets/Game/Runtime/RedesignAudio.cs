using UnityEngine;

namespace RocketRaja
{
    /// <summary>Original synthesized music and cues, with two Editor-authored audio sources.</summary>
    public sealed class RedesignAudio : MonoBehaviour
    {
        public RedesignRuntimeDriver driver;
        public AudioSource music, effects;
        public bool explicitFeedback;
        public int CueCount { get; private set; }
        public string LastCue { get; private set; }
        AudioClip tune, coin, reward, purchase, skill, reveal;
        string lastRun;
        RocketRaja.Core.Natural taps, lifetime;
        bool bound, foreground = true, pausedMusic;
        void Start()
        {
            tune = Make("Evening lights", new[] { 261.63f, 329.63f, 392f, 440f, 392f, 329.63f, 293.66f, 329.63f }, .65f);
            coin = Make("Coin", new[] { 1046.5f }, .09f); reward = Make("New shelf", new[] { 523.25f, 659.25f, 783.99f }, .12f);
            music.clip = tune;
            purchase = Make("Purchase", new[] { 659.25f, 987.77f }, .09f);
            skill = Make("Skill", new[] { 392f, 587.33f, 783.99f }, .09f);
            reveal = Make("Talent", new[] { 659.25f, 783.99f, 1046.5f }, .14f);
        }
        void Update()
        {
            var session = driver.Session;
            if (session == null || !driver.Initialized) { Pause(); return; }
            var s = session.Engine.State;
            music.mute = !s.music; effects.mute = !s.sound;
            if (!foreground || session.Suspended || !session.CanMutate || !s.music) Pause();
            else if (pausedMusic) { music.UnPause(); pausedMusic = false; if(!music.isPlaying)music.Play(); }
            else if (!music.isPlaying) music.Play();
            if (!explicitFeedback && bound && lastRun == s.runId && taps.CompareTo(s.manualTaps) < 0) Feedback("tap");
            if (!explicitFeedback && bound && (lastRun != s.runId || lifetime.CompareTo(s.completedPrestiges) < 0)) Feedback("prestige");
            taps = s.manualTaps; lifetime = s.completedPrestiges; lastRun = s.runId; bound = true;
        }
        public void Feedback(string cue)
        {
            if (!foreground || !driver.Initialized || !driver.Session.CanMutate || !driver.Session.Engine.State.sound) return;
            var clip = cue == "tap" ? coin : cue == "purchase" ? purchase : cue == "skill" ? skill : cue == "reveal" ? reveal : cue == "prestige" ? reward : null;
            if (!clip) return;
            effects.mute = false; effects.PlayOneShot(clip); CueCount++; LastCue = cue;
        }
        void Pause() { if (music && music.isPlaying) { music.Pause(); pausedMusic = true; } }
        void OnApplicationPause(bool paused) { foreground = !paused; if (paused) { Pause(); effects.Stop(); } }
        void OnDisable() { Pause(); if(effects)effects.Stop(); }
        void OnDestroy() { foreach(var clip in new[]{tune,coin,reward,purchase,skill,reveal})if(clip)Destroy(clip); }
        static AudioClip Make(string name, float[] notes, float duration)
        {
            const int rate = 22050; var samples = new float[Mathf.CeilToInt(rate * duration * notes.Length)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)rate, local = t % duration; int note = Mathf.Min(notes.Length - 1, (int)(t / duration));
                float envelope = Mathf.Min(1, local * 180) * Mathf.Exp(-local * 6 / duration) * Mathf.Clamp01((duration - local) * 50);
                samples[i] = (Mathf.Sin(2 * Mathf.PI * notes[note] * t) + .25f * Mathf.Sin(4 * Mathf.PI * notes[note] * t)) * envelope * .45f;
            }
            var clip = AudioClip.Create(name, samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
    }
}
