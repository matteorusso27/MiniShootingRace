
using UnityEngine;
using static GameSelectors;
public class AudioSystem : Singleton<AudioSystem>
{
    [SerializeField] private AudioSource GameMusic;
    [SerializeField] private AudioSource VFXSource;

    [SerializeField] private AudioClip BasketClip;
    [SerializeField] private AudioClip BoardClip;
    [SerializeField] private AudioClip FireClip;

    public void Setup()
    {
        GameMusic.volume = 0.02f;
        VFXSource.volume = 0.1f;
        PlayGameMusic();
    }
    public void PlayGameMusic() => GameMusic.Play();
    public void PlayBasketBallSound() => VFXSource.PlayOneShot(BasketClip);
    public void PlayBoardSound() => VFXSource.PlayOneShot(BoardClip);
    public void PlayFireSound()
    {
        var audioSource = GameM.Data.PlayerBall.GetComponent<AudioSource>();
        if (audioSource.isPlaying) return;
        if (audioSource.clip == null) audioSource.clip = FireClip;
        audioSource.volume = 0.1f;
        audioSource.Play();
    }
    public void StopFireSound() => GameM.Data.PlayerBall.GetComponent<AudioSource>().Stop();
}
