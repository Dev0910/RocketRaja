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
        AudioClip tune, coin, reward, purchase, skill, reveal, open, close, click;
        float lastTapAt=-10;
        string lastRun;
        RocketRaja.Core.Natural taps, lifetime;
        bool bound, foreground = true, pausedMusic;
        void Start()
        {
            tune = MakeMusic();
            coin = Make("Coin", new[] { 1046.5f }, .09f); reward = Make("New shelf", new[] { 523.25f, 659.25f, 783.99f }, .12f);
            music.clip = tune;
            purchase = Make("Purchase", new[] { 659.25f, 987.77f }, .09f);
            skill = Make("Skill", new[] { 392f, 587.33f, 783.99f }, .09f);
            reveal = Make("Talent", new[] { 659.25f, 783.99f, 1046.5f }, .14f);
            open=Make("Open",new[]{392f,523.25f},.045f);close=Make("Close",new[]{587.33f,440f},.045f);click=Make("Switch",new[]{660f},.05f);
        }
        void Update()
        {
            var session = driver.Session;
            if (session == null || !driver.Initialized) { Pause(); return; }
            var s = session.Engine.State;
            music.mute = !s.music; effects.mute = !s.sound;
            if(!s.sound&&effects.isPlaying)effects.Stop();
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
            if(cue=="tap"){if(Time.unscaledTime-lastTapAt<.055f)return;lastTapAt=Time.unscaledTime;}
            var clip = cue == "tap" ? coin : cue == "purchase" ? purchase : cue == "skill" ? skill : cue == "reveal" ? reveal : cue == "prestige" || cue=="reward" ? reward : cue=="open" || cue=="navigate" ? open : cue=="close" ? close : cue=="click" ? click : null;
            if (!clip) return;
            effects.mute = false; effects.PlayOneShot(clip); CueCount++; LastCue = cue;
        }
        void Pause() { if (music && music.isPlaying) { music.Pause(); pausedMusic = true; } }
        void OnApplicationPause(bool paused) { foreground = !paused; if (paused) { Pause(); effects.Stop(); } }
        void OnDisable() { Pause(); if(effects)effects.Stop(); }
        void OnDestroy() { foreach(var clip in new[]{tune,coin,reward,purchase,skill,reveal,open,close,click})if(clip)Destroy(clip); }
        static AudioClip MakeMusic()
        {
            const int rate=22050;const float beat=.36f;
            float[] notes={523.25f,0,587.33f,659.25f,783.99f,0,659.25f,587.33f,523.25f,0,440,0,392,440,523.25f,0,659.25f,0,783.99f,880,783.99f,0,659.25f,587.33f,523.25f,440,392,0,440,0,523.25f,0};
            var samples=new float[Mathf.CeilToInt(rate*beat*notes.Length)];
            for(int n=0;n<notes.Length;n++)
            {
                Add(notes[n],n*beat,.38f,.14f);
                if(n%8==0)Add(n<16?130.81f:110,n*beat,.7f,.09f);
            }
            void Add(float frequency,float at,float duration,float volume)
            {
                if(frequency<=0)return;int start=Mathf.RoundToInt(at*rate),count=Mathf.CeilToInt(duration*rate);
                for(int i=0;i<count&&start+i<samples.Length;i++)
                {
                    float t=i/(float)rate,envelope=Mathf.Min(1,t/.008f)*Mathf.Exp(-t*7/duration);
                    samples[start+i]+=Mathf.Sin(2*Mathf.PI*frequency*t)*envelope*volume;
                }
            }
            var clip=AudioClip.Create("Evening lights - reviewed festival loop",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
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
