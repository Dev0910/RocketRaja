using UnityEngine;

namespace RocketRaja
{
    // Small original synthesized cues; no external music samples or audio services.
    public sealed class ShopAudio : MonoBehaviour
    {
        GameController game; AudioSource music, effects; AudioClip tap, reward;
        public void Initialize(GameController controller)
        {
            game = controller;
            music = gameObject.AddComponent<AudioSource>(); effects = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = effects.playOnAwake = false;
            music.loop = true; music.volume = .12f; effects.volume = .25f;
            music.clip = Tune("Evening lights", new[] { 261.63f, 329.63f, 392f, 440f, 392f, 329.63f, 293.66f, 329.63f }, .65f);
            tap = Tune("Coin", new[] { 1046.5f }, .09f);
            reward = Tune("New shelf", new[] { 523.25f, 659.25f, 783.99f }, .12f);
            music.Play();
        }
        void Update() { if (game != null) { music.mute = !game.Engine.State.music; effects.mute = !game.Engine.State.sound; } }
        public void Tap() { if (game.Engine.State.sound) effects.PlayOneShot(tap); }
        public void Celebrate() { if (game.Engine.State.sound) effects.PlayOneShot(reward); }
        static AudioClip Tune(string name, float[] notes, float duration)
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
