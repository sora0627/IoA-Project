using UnityEngine;

namespace Systems
{
    public class SoundManager :SingletonDontDestroy<SoundManager>
    {
        [Header("オーディオソース")]
        [SerializeField] private AudioSource bgmSource; //BGM用
        [SerializeField] private AudioSource seSource;  //SE用

        [Header("BGMクリップ")]
        public AudioClip mainBGM;
        public AudioClip titleBGM;

        [Header("SEクリップ")]
        public AudioClip setSE;
        public AudioClip winSE;
        public AudioClip loseSE;

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;

            bgmSource.time = 0f;
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
        }

        public void PlaySE(AudioClip clip)
        {
            if (clip == null) return;

            seSource.PlayOneShot(clip);
        }
    }
}