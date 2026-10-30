using UnityEngine;
namespace SlotGame { public class GameAudio:MonoBehaviour { public AudioSource source; public AudioClip spin,stop,win; public void Play(AudioClip clip){if(clip)source.PlayOneShot(clip);} public void Mute(bool value){source.mute=value;} } }
