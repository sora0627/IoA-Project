using UnityEngine;

namespace Systems
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance { get; private set; }

        [Header("オーディオソース")]
        [SerializeField] private AudioSource bgmSource; //BGM用
        [SerializeField] private AudioSource seSource;  //SE用

        [Header("BGMクリップ")]
        public AudioClip mainBGM;
        public AudioClip titleBGM;

        [Header("SEクリップ")]
        public AudioClip setSE;
        public AudioClip winSE;

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            PlayBGM(mainBGM);
        }

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;

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