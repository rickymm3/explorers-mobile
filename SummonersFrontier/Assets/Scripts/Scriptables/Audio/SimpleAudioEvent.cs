using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Audio Events/Simple")]
public class SimpleAudioEvent : AudioEvent {
    public AudioClip[] clips;

    [MinMaxRange(0, 1)]
    public RangedFloat volume = new RangedFloat() { minValue = 1, maxValue = 1 };
    [MinMaxRange(0, 2)]
    public RangedFloat pitch = new RangedFloat() { minValue = 1, maxValue = 1 };

    public override void Play(AudioSource source) {
        if (clips.Length == 0) {
            Debug.LogWarning("No clips assigned in: "+ this.ToString());
            return;
        }

        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = Random.Range(volume.minValue, volume.maxValue);
        source.pitch = Random.Range(pitch.minValue, pitch.maxValue);
        source.Play();
    }
}
