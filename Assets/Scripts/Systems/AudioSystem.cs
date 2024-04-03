
using UnityEngine;

public class AudioSystem : Singleton<AudioSystem>
{
    [SerializeField] private AudioSource Hoop;
    [SerializeField] private AudioClip BasketClip;

    public void PlayBasketBallSound() => Hoop.PlayOneShot(BasketClip);
}
