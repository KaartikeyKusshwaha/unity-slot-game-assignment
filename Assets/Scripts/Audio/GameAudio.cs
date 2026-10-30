using System.Collections;
using UnityEngine;

namespace SlotGame
{
    /// <summary>Bounded effects pool plus a dedicated, fading reel loop. No speech clips.</summary>
    public class GameAudio : MonoBehaviour
    {
        [Header("Audio sources")]
        public AudioSource source;
        public AudioSource loopSource;

        [Header("Menu and lever sounds")]
        public AudioClip click;
        public AudioClip entry;
        public AudioClip lever;
        public AudioClip latch;

        [Header("Reel sounds")]
        public AudioClip spin;
        public AudioClip stop;
        public AudioClip tick;

        [Header("Reward sounds")]
        public AudioClip win;
        public AudioClip collect;
        public AudioClip count;
        public int ReelTickCount
        {
            get; private set;
        }
        public int RevealCount
        {
            get; private set;
        }
        public int CountTickCount
        {
            get; private set;
        }
        public bool ReelLoopPlaying => loopSource.isPlaying;
        private AudioSource[] tickSources;
        private readonly float[] lastTick = { -1, -1, -1 };
        private int remainingReels;
        private bool rolling;
        private float motorStart;
        private float nextCount;
        private AudioSource[] pool;
        private readonly float[] gains = { 1, 1, 1, 1 };
        private int nextVoice;
        private int lastClickFrame = -1;
        private float master = 1;
        private bool muted;
        private Coroutine loopFade;

        private void Awake()
        {
            tickSources = new AudioSource[3];
            for (int i = 0; i < 3; i++)
            {
                tickSources[i] = gameObject.AddComponent<AudioSource>();
                tickSources[i].playOnAwake = false;
            }
            pool = new AudioSource[4];
            pool[0] = source;
            for (int i = 1; i < pool.Length; i++)
            {
                pool[i] = gameObject.AddComponent<AudioSource>();
                pool[i].playOnAwake = false;
            }
        }
        public void Click()
        {
            if (lastClickFrame == Time.frameCount)
                return;
            lastClickFrame = Time.frameCount;
            Play(click, .65f);
        }
        public void Play(AudioClip clip, float volume = 1, float pitch = 1)
        {
            if (clip == null)
                return;
            int selected = nextVoice++ % pool.Length;
            for (int i = 0; i < pool.Length; i++)
                if (!pool[i].isPlaying)
                {
                    selected = i;
                    break;
                }
            var voice = pool[selected];
            voice.Stop();
            gains[selected] = volume;
            voice.clip = clip;
            voice.pitch = pitch;
            voice.volume = master * volume;
            voice.mute = muted;
            voice.Play();
        }
        public void Reveal(int index)
        {
            RevealCount++;
            remainingReels = 2 - index;
            tickSources[index].Stop();
            Play(stop, .85f, 1 + index * .12f);
        }
        public void ReelTick(int index)
        {
            if (tick == null || Time.unscaledTime - lastTick[index] < .045f)
                return;
            ReelTickCount++;
            lastTick[index] = Time.unscaledTime;
            var voice = tickSources[index];
            voice.clip = tick;
            voice.volume = master * .18f;
            voice.pitch = .94f + index * .07f;
            voice.mute = muted;
            voice.Play();
        }
        public void CountTick(float progress)
        {
            if (Time.unscaledTime < nextCount)
                return;
            CountTickCount++;
            nextCount = Time.unscaledTime + .09f;
            Play(count, .22f, 1 + progress * .35f);
        }
        private void Update()
        {
            if (!rolling)
                return;
            float acceleration = Mathf.Clamp01((Time.unscaledTime - motorStart) / .35f);
            float remainingReelBlend = remainingReels / 3f;
            loopSource.pitch = Mathf.Lerp(.82f, 1.12f, acceleration)
                * Mathf.Lerp(.85f, 1, remainingReelBlend);
            loopSource.volume = master * .3f
                * Mathf.Lerp(.18f, 1, remainingReelBlend);
        }
        public void StartReels()
        {
            if (loopFade != null)
                StopCoroutine(loopFade);
            ReelTickCount = 0;
            RevealCount = 0;
            CountTickCount = 0;
            remainingReels = 3;
            rolling = true;
            motorStart = Time.unscaledTime;
            Play(spin, .65f);
            loopSource.volume = master * .3f;
            loopSource.Play();
        }
        public void StopReels()
        {
            rolling = false;
            foreach (var voice in tickSources)
                voice.Stop();
            loopFade = StartCoroutine(FadeLoop());
        }
        private IEnumerator FadeLoop()
        {
            float from = loopSource.volume;
            for (float t = 0; t < .15f; t += Time.unscaledDeltaTime)
            {
                loopSource.volume = Mathf.Lerp(from, 0, t / .15f);
                yield return null;
            }
            loopSource.Stop();
            loopSource.volume = master * .12f;
            loopFade = null;
        }
        public void ApplySettings(SaveData data)
        {
            master = data.volume;
            muted = data.muted;
            if (pool != null)
                for (int i = 0; i < pool.Length; i++)
                {
                    pool[i].mute = muted;
                    pool[i].volume = master * gains[i];
                }
            if (tickSources != null)
                foreach (var voice in tickSources)
                    voice.mute = muted;
            loopSource.mute = muted;
            loopSource.volume = master * .12f;
        }
    }
}
