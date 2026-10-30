using System.Collections;
using UnityEngine;

namespace SlotGame
{
    /// <summary>Bounded effects pool plus a dedicated, fading reel loop. No speech clips.</summary>
    public class GameAudio : MonoBehaviour
    {
        public AudioSource source, loopSource;
        public AudioClip click, entry, lever, spin, stop, win, collect;
        private AudioSource[] pool;
        private readonly float[] gains = { 1, 1, 1, 1 };
        private int nextVoice, lastClickFrame = -1;
        private float master = 1;
        private bool muted;
        private Coroutine loopFade;

        private void Awake()
        {
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
            Play(stop, .85f, 1 + index * .12f);
        }
        public void StartReels()
        {
            if (loopFade != null)
                StopCoroutine(loopFade);
            Play(spin, .65f);
            loopSource.volume = master * .12f;
            loopSource.Play();
        }
        public void StopReels()
        {
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
            loopSource.mute = muted;
            loopSource.volume = master * .12f;
        }
    }
}
