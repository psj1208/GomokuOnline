using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public enum EAudioType
{
    Master,
    BGM,
    SFX
}

public class AudioManager : SingletonServer<AudioManager>
{
    private bool isInitialized = false;
    //오디오 타입 설정
    [SerializeField] AudioMixer audioMixer;
    //메인 BGM은 단 하나만 재생되도록 해야 하므로 변수를 따로 설정. 메서드 또한 구분되어있다.
    [SerializeField] SoundSource curBgm;

    //오브젝트 풀링용(사용되는 빈도가 높아 풀링이 필요하다 생각하여 먼저 작업)
    ObjectPool<SoundSource> audioPool;
    [SerializeField] SoundSource audioSourcePrefab;

    //인스펙터 상에서 깔끔하게 보이도록 정리용으로 만든 변수
    [SerializeField] Transform tempRoot;
    [SerializeField] Transform bgmRoot;
    //현재 활성화되어있는 사운드(사운드 일괄 제거용)
    private List<SoundSource> activeSources;
    private Dictionary<string, AudioClip> loadedAudio = new Dictionary<string, AudioClip>();

    protected override async void Awake()
    {
        base.Awake();
        activeSources = new List<SoundSource>();
        var mixerLoad = AddressManager.Instance.LoadAssetAsyncTask<AudioMixer>("AudioMixer");
        audioMixer = await mixerLoad;

        var sourceLoad = AddressManager.Instance.LoadAssetAsyncTask<GameObject>("SoundSource");
        audioSourcePrefab = (await sourceLoad).GetComponent<SoundSource>();

        // 풀 초기화
        audioPool = new ObjectPool<SoundSource>(
            CreatePooledItem,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 50
        );

        //일시적인지 BGM인지 구분해서 맞는 부모에 할당.
        tempRoot = new GameObject("Temp").transform;
        tempRoot.SetParent(transform);

        bgmRoot = new GameObject("BGM").transform;
        bgmRoot.SetParent(transform);

        SceneManager.sceneLoaded += OnSceneLoaded;
        isInitialized = true;
        Debug.Log("AudioManager initialized");
    }

    public async Task WaitForInitialize()
    {
        while (!isInitialized)
            await Task.Yield();
    }

    #region 풀링
    SoundSource CreatePooledItem()
    {
        SoundSource soundSource = Instantiate(audioSourcePrefab).GetComponent<SoundSource>();
        soundSource.Init(audioPool);
        return soundSource;
    }

    void OnTakeFromPool(SoundSource obj)
    {
        obj.gameObject.SetActive(true);
        activeSources.Add(obj);
    }

    void OnReturnedToPool(SoundSource obj)
    {
        obj.gameObject.SetActive(false);
        activeSources.Remove(obj);
    }

    void OnDestroyPoolObject(SoundSource obj)
    {
        Destroy(obj.gameObject);
    }

    #endregion

    #region 사운드 재생&정지
    public async void Audio2DPlay(string clipKey, float volume = 1f, bool isLoop = false, EAudioType type = EAudioType.Master)
    {
        await AudioManager.Instance.WaitForInitialize();

        if (clipKey == null)
        {
            Debug.LogWarning("재생할 오디오 클립이 없습니다.");
            return;
        }

        AudioClip clip;

        if (loadedAudio.TryGetValue(clipKey, out clip))
        {
            Debug.Log($"[AudioManager] Dictionary Found! Key : {clipKey}");

            SoundSource obj = audioPool.Get();
            obj.transform.SetParent(tempRoot);
            AudioSource source = obj.GetComponent<AudioSource>();
            source.loop = isLoop;
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.spatialBlend = 0f;
            SetOutputGroup(source, type);
            source.Play();

            // clip의 길이만큼 지난 후 오브젝트 파괴
            if (isLoop == false)
            {
                obj.GetComponent<SoundSource>().Play(clip.length);
            }
            return;
        }

        AddressManager.Instance.LoadAssetAsync<AudioClip>(clipKey, (audio) =>
        {
            clip = audio;

            if (clip == null)
            {
                Debug.Log($"해당 Audio는 존재하지 않음! key : {clipKey}");
                return;
            }

            loadedAudio[clipKey] = clip;

            SoundSource obj = audioPool.Get();
            obj.transform.SetParent(tempRoot);
            AudioSource source = obj.GetComponent<AudioSource>();
            source.loop = isLoop;
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.spatialBlend = 0f;
            SetOutputGroup(source, type);
            source.Play();

            // clip의 길이만큼 지난 후 오브젝트 파괴
            if (isLoop == false)
            {
                obj.GetComponent<SoundSource>().Play(clip.length);
            }
        });
    }

    public async void Audio3DPlay(string clipKey, Vector3 pos, float volume = 1f, bool isLoop = false, EAudioType type = EAudioType.Master)
    {
        await AudioManager.Instance.WaitForInitialize();

        if (clipKey == null)
        {
            Debug.LogWarning("재생할 오디오 클립이 없습니다.");
            return;
        }

        AudioClip clip;

        if (loadedAudio.TryGetValue(clipKey, out clip))
        {
            Debug.Log($"[AudioManager] Dictionary Found! Key : {clipKey}");

            SoundSource obj = audioPool.Get();
            obj.transform.position = pos;
            obj.transform.SetParent(tempRoot);

            AudioSource source = obj.GetComponent<AudioSource>();
            source.clip = clip;
            source.loop = isLoop;
            source.volume = Mathf.Clamp01(volume);
            SetOutputGroup(source, type);
            source.spatialBlend = 1f;
            source.Play();

            if (!isLoop)
                obj.Play(clip.length);
        }

        AddressManager.Instance.LoadAssetAsync<AudioClip>(clipKey, (audio) =>
        {
            clip = audio;

            if (clip == null)
            {
                Debug.Log($"해당 Audio는 존재하지 않음! key : {clipKey}");
                return;
            }

            loadedAudio[clipKey] = clip;

            SoundSource obj = audioPool.Get();
            obj.transform.position = pos;
            obj.transform.SetParent(tempRoot);

            AudioSource source = obj.GetComponent<AudioSource>();
            source.clip = clip;
            source.loop = isLoop;
            source.volume = Mathf.Clamp01(volume);
            SetOutputGroup(source, type);
            source.spatialBlend = 1f;
            source.Play();

            if (!isLoop)
                obj.Play(clip.length);
        });
    }

    public async void AudioBGMPlay(string clipKey, bool isLoop = true, float volume = 1f, EAudioType type = EAudioType.Master)
    {
        await AudioManager.Instance.WaitForInitialize();

        if (clipKey == null)
        {
            Debug.LogWarning("재생할 BGM 클립이 없습니다.");
            return;
        }

        AudioClip clip;

        if (loadedAudio.TryGetValue(clipKey, out clip))
        {
            Debug.Log($"[AudioManager] Dictionary Found! Key : {clipKey}");

            SoundSource obj = audioPool.Get();
            obj.transform.SetParent(bgmRoot);

            AudioSource source = obj.GetComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 0f;
            source.volume = volume;
            source.loop = isLoop;
            SetOutputGroup(source, type);
            source.Play();

            if (isLoop == true)
                curBgm = obj;
            else
                obj.Play(clip.length);
        }

        AddressManager.Instance.LoadAssetAsync<AudioClip>(clipKey, (audio) =>
        {
            clip = audio;

            if (clip == null)
            {
                Debug.Log($"해당 Audio는 존재하지 않음! key : {clipKey}");
                return;
            }

            if (curBgm != null)
            {
                audioPool.Release(curBgm);
                curBgm = null;
            }

            loadedAudio[clipKey] = clip;

            SoundSource obj = audioPool.Get();
            obj.transform.SetParent(bgmRoot);

            AudioSource source = obj.GetComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 0f;
            source.volume = volume;
            source.loop = isLoop;
            SetOutputGroup(source, type);
            source.Play();

            if (isLoop == true)
                curBgm = obj;
            else
                obj.Play(clip.length);
        });
    }
    #endregion

    #region 포톤 서버 전용 오디오 재생 메서드 그룹
    [PunRPC]
    void RPC_PlaySound(string clipKey, float volume = 1f, bool isLoop = false, EAudioType type = EAudioType.Master)
    {
        Audio2DPlay(clipKey, volume, isLoop, type);
    }

    public void PlayNetwork2DSound(string clipKey, float volume = 1f, bool isLoop = false, EAudioType type = EAudioType.Master)
    {
        photonView.RPC(nameof(RPC_PlaySound), RpcTarget.All, clipKey, volume, isLoop, type);
    }

    #endregion
    public void StopBGM()
    {
        if (curBgm != null)
        {
            audioPool.Release(curBgm);
            curBgm = null;
        }
    }

    public void StopAllSounds()
    {
        if (activeSources == null) return;

        foreach (var source in activeSources.ToArray())
        {
            audioPool.Release(source);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllSounds();
        StopBGM();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #region 오디오 설정, 타입 지정
    //SettingManager같은 걸로 따로 정리해둘 껄 그랬음.
    async void LoadAllAudioSetting()
    {
        await AudioManager.Instance.WaitForInitialize();

        foreach (EAudioType type in System.Enum.GetValues(typeof(EAudioType)))
        {
            if (PlayerPrefs.HasKey(type.ToString()))
            {
                float CurrentValue = PlayerPrefs.GetFloat(type.ToString());
                SetLevel(CurrentValue, type);
            }
            else
            {
                SetLevel(0.5f, type);
            }
        }
    }
    //오디오 믹서에 음량을 맞추는 과정
    public async void SetLevel(float value, EAudioType type)
    {
        await AudioManager.Instance.WaitForInitialize();

        float ChangeValue;
        if (value <= 0.001f)
            ChangeValue = -80;
        else
            ChangeValue = Mathf.Log10(value) * 20;
        audioMixer.SetFloat(type.ToString(), ChangeValue);
    }

    //해당 슬라이더에 오디오세팅을 적용(UI에 표현하기 위함, PlayerPrefs 이용)
    public async void LoadAudioSetting(EAudioType type, Slider slider = null)
    {
        await AudioManager.Instance.WaitForInitialize();

        if (PlayerPrefs.HasKey(type.ToString()))
        {
            float CurrentValue = PlayerPrefs.GetFloat(type.ToString());
            if (slider != null)
            {
                slider.value = Mathf.Clamp(CurrentValue, slider.minValue, slider.maxValue);
                SetLevel(CurrentValue, type);
            }
        }
    }

    public async void LoadAudioSettingNotPrefs(EAudioType type, Slider slider = null)
    {
        await AudioManager.Instance.WaitForInitialize();

        if (audioMixer.GetFloat(type.ToString(), out float dbValue))
        {
            if (slider != null)
            {
                float value = Mathf.Pow(10, dbValue / 20);

                value = Mathf.Clamp(value, slider.minValue, slider.maxValue);

                slider.value = value;
            }
        }
        else
        {
            Debug.LogWarning($"[AudioManager] 해당 타입은 AudioMixer에 존재하지 않습니다. {type.ToString()}");
        }
    }
    //오디오 세팅 저장
    public async void SaveAudioSetting(float value, EAudioType type)
    {
        await AudioManager.Instance.WaitForInitialize();

        PlayerPrefs.SetFloat(type.ToString(), value);
    }
    //열거체를 오디오 믹서의 타입과 비교해 해당 타입을 소스에 적용함(오디오 재생할 때 이걸로 타입 맞춤)
    private void SetOutputGroup(AudioSource source, EAudioType type)
    {
        AudioMixerGroup[] groups = audioMixer.FindMatchingGroups(type.ToString());
        if (groups != null && groups.Length > 0)
            source.outputAudioMixerGroup = groups[0];
        else
            source.outputAudioMixerGroup = null;
    }
    #endregion
}
