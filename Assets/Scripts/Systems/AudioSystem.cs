
using UnityEngine;

public class AudioSystem : Singleton<AudioSystem>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundsSource;

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }
    public void PlaySounds(AudioClip clip) => PlaySound(clip);
    public void PlaySound(AudioClip clip) => soundsSource.PlayOneShot(clip);
}
