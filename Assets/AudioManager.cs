using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("오디오 믹서 연결")]
    public AudioMixerGroup bgmMixerGroup;
    public AudioMixerGroup sfxMixerGroup;

    [Header("BGM 전용 소스")]
    [Tooltip("배경음악은 동시에 1개만 재생되므로 하나의 소스만 씁니다.")]
    public AudioSource bgmSource;

    [Header("SFX 풀링 (다중 효과음)")]
    [Tooltip("효과음은 동시에 여러 개가 재생되어야 하므로 소스를 여러 개 만들어 둡니다.")]
    public int sfxSourceCount = 10;
    private List<AudioSource> sfxSources = new List<AudioSource>();

    private float lastHitSoundTime;
    [Header("페이드 설정")]
    public float fadeDuration = 1.0f; // BGM이 전환되는 데 걸리는 시간 (1초)
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSFXPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSFXPool()
    {
        // AudioManager 하위에 빈 오브젝트를 만들고 AudioSource를 여러 개 붙여둡니다.
        GameObject sfxHolder = new GameObject("SFX_Pool");
        sfxHolder.transform.SetParent(transform);

        for (int i = 0; i < sfxSourceCount; i++)
        {
            AudioSource newSource = sfxHolder.AddComponent<AudioSource>();
            newSource.outputAudioMixerGroup = sfxMixerGroup; // 믹서를 SFX로 할당!
            newSource.playOnAwake = false;
            sfxSources.Add(newSource);
        }
    }

    public void PlayBGM(AudioClip bgmClip)
    {
        if (bgmSource.clip == bgmClip) return; // 이미 같은 노래가 나오고 있으면 무시

        bgmSource.outputAudioMixerGroup = bgmMixerGroup; // 믹서를 BGM으로 할당!
        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.Play();
    }


    public void PlaySFX(AudioClip sfxClip, float pitchRandomness = 0.1f)
    {
        if (sfxClip == null) return;

        AudioSource availableSource = sfxSources.Find(source => !source.isPlaying);

        if (availableSource != null)
        {
            // 타격음이 약간씩 다르게 들리도록 피치(음높이)를 살짝 섞어줍니다!
            availableSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
            
            // PlayOneShot을 쓰면 한 소스에서 소리가 겹쳐서 나게 할 수도 있습니다.
            availableSource.PlayOneShot(sfxClip);
        }
        else
        {
            Debug.LogWarning("SFX 풀이 꽉 찼습니다! sfxSourceCount를 늘려주세요.");
        }
    }
    public void PlayHitSound(AudioClip hitClip)
    {
        // 마지막으로 타격음이 난 지 0.05초가 안 지났다면 씹어버립니다!
        if (Time.time - lastHitSoundTime < 0.05f) return; 

        PlaySFX(hitClip);
        lastHitSoundTime = Time.time;
    }
    public void PlayBGMWithFade(AudioClip newClip)
    {
        if (bgmSource.clip == newClip) return;

        // 이미 페이드 효과가 진행 중이라면 중지
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(newClip));
    }

    private System.Collections.IEnumerator FadeRoutine(AudioClip newClip)
    {
        float startVolume = bgmSource.volume;
        
        if (bgmSource.isPlaying)
        {
            while (bgmSource.volume > 0)
            {
                bgmSource.volume -= startVolume * Time.deltaTime / fadeDuration;
                yield return null;
            }
        }

        bgmSource.clip = newClip;
        bgmSource.Play();

        while (bgmSource.volume < startVolume)
        {
            bgmSource.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        bgmSource.volume = startVolume;
        fadeCoroutine = null;
    }
}