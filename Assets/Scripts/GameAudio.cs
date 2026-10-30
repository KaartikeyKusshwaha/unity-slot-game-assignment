using UnityEngine;

namespace SlotGame
{
    public class GameAudio : MonoBehaviour
    {
        public AudioSource source;
        public AudioClip spin;
        public AudioClip stop;
        public AudioClip win;

        public void Play(AudioClip clip)
        {
            if (clip != null)
                source.PlayOneShot(clip);
        }

        public void Mute(bool value)
        {
            source.mute = value;
        }
    }
}
