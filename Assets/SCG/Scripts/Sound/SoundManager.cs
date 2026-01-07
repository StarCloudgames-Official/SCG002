using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using StarCloudgamesLibrary;

public class SoundManager : Singleton<SoundManager>
{
    private const string DefaultSoundBoxKey = "SoundBox";
    private const int InitialSfxPoolSize = 4;
    private const int MaxSfxPoolSize = 32;
    private const int MaxSfxInstancesPerSound = 10;

    private SoundBox soundBox;

    private AudioSource bgmSource;

    private ObjectPool<AudioSource> sfxPool;
    private Transform sfxRoot;
    private readonly Dictionary<SoundId, int> activeSfxCounts = new();

    private bool initialized;
    private bool soundBoxLoaded;

    private bool enableSfx = true;
    private bool enableBgm = true;

    #region Initialize

    public override async UniTask Initialize()
    {
        var handle = Addressables.LoadAssetAsync<SoundBox>(DefaultSoundBoxKey);
        var box = await handle.Task;

        if (box == null)
        {
            Debug.LogError($"[SoundManager] SoundBox not found at Addressables key: {DefaultSoundBoxKey}");
            return;
        }

        Instance.Setup(box);
    }

    private void Setup(SoundBox box)
    {
        if (initialized)
        {
            soundBox = box;
            soundBoxLoaded = true;
            return;
        }

        initialized = true;
        soundBox = box;
        soundBoxLoaded = true;

        CreateBgmSource();
        CreateSfxPool();
    }

    #endregion

    #region AudioSources

    private void CreateBgmSource()
    {
        var go = new GameObject("BGM Source");
        go.transform.SetParent(CachedTransform);

        bgmSource = go.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
    }

    private void CreateSfxPool()
    {
        sfxRoot = new GameObject("SFX Root").transform;
        sfxRoot.SetParent(CachedTransform);

        sfxPool = new ObjectPool<AudioSource>(
            CreatePooledSfxSource,
            OnGetSfxSource,
            OnReleaseSfxSource,
            OnDestroySfxSource,
            collectionCheck: false,
            defaultCapacity: InitialSfxPoolSize,
            maxSize: MaxSfxPoolSize
        );

        for (var i = 0; i < InitialSfxPoolSize; i++)
        {
            var src = sfxPool.Get();
            sfxPool.Release(src);
        }
    }

    private AudioSource CreatePooledSfxSource()
    {
        var go = new GameObject("SFX Source");
        go.transform.SetParent(sfxRoot);

        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0f;

        return src;
    }

    private void OnGetSfxSource(AudioSource src)
    {
        src.gameObject.SetActive(true);
    }

    private void OnReleaseSfxSource(AudioSource src)
    {
        src.Stop();
        src.clip = null;
        src.gameObject.SetActive(false);
        src.transform.position = Vector3.zero;
        src.spatialBlend = 0f;
    }

    private void OnDestroySfxSource(AudioSource src)
    {
        if (src != null)
        {
            Destroy(src.gameObject);
        }
    }

    #endregion

    #region Settings

    public static void SetSfxEnabled(bool enabled)
    {
        if (Instance == null) return;
        Instance.enableSfx = enabled;
    }

    public static void SetBgmEnabled(bool enabled)
    {
        if (Instance == null) return;
        Instance.enableBgm = enabled;

        if (!enabled && Instance.bgmSource != null)
        {
            Instance.bgmSource.Stop();
        }
    }

    public static bool IsSfxEnabled => Instance != null && Instance.enableSfx;
    public static bool IsBgmEnabled => Instance != null && Instance.enableBgm;

    #endregion

    #region SFX

    public static void PlaySFX(SoundId id)
    {
        if (Instance == null) return;
        Instance.PlaySfxInternal(id, null, false);
    }

    public static void PlaySFXAt(SoundId id, Vector3 position, bool spatial = true)
    {
        if (Instance == null) return;
        Instance.PlaySfxInternal(id, position, spatial);
    }

    #endregion

    #region BGM

    public static void PlayBGM(SoundId id, bool loop = true)
    {
        if (Instance == null) return;
        Instance.PlayBgmInternal(id, loop);
    }

    public static void StopBGM()
    {
        if (Instance == null) return;
        Instance.StopBgmInternal();
    }

    #endregion

    #region SFX Logic

    private void PlaySfxInternal(SoundId id, Vector3? position, bool spatial)
    {
        if (!initialized || !soundBoxLoaded)
        {
            Debug.LogWarning("[SoundManager] 아직 초기화되지 않았습니다. InitializeAsync() 먼저 호출하세요.");
            return;
        }

        if (!enableSfx) return;

        var clip = soundBox.GetClip(id);
        if (clip == null)
        {
            Debug.LogWarning($"[SoundManager] Clip not found for SoundId: {id}");
            return;
        }

        if (!TryReserveSfxSlot(id)) return;

        var src = sfxPool.Get();

        src.clip = clip;

        if (position.HasValue)
        {
            src.transform.position = position.Value;
            src.spatialBlend = spatial ? 1f : 0f;
        }
        else
        {
            src.transform.position = Vector3.zero;
            src.spatialBlend = 0f;
        }

        src.Play();
        StartCoroutine(Co_ReleaseSfxAfterPlay(id, src, clip.length));
    }

    private bool TryReserveSfxSlot(SoundId id)
    {
        if (!activeSfxCounts.TryGetValue(id, out var count))
        {
            activeSfxCounts[id] = 1;
            return true;
        }

        if (count >= MaxSfxInstancesPerSound) return false;

        activeSfxCounts[id] = count + 1;
        return true;
    }

    private void ReleaseSfxSlot(SoundId id)
    {
        if (!activeSfxCounts.TryGetValue(id, out var count)) return;

        count--;
        if (count <= 0)
        {
            activeSfxCounts.Remove(id);
        }
        else
        {
            activeSfxCounts[id] = count;
        }
    }

    private IEnumerator Co_ReleaseSfxAfterPlay(SoundId id, AudioSource src, float duration)
    {
        yield return new WaitForSeconds(duration);
        try
        {
            sfxPool?.Release(src);
        }
        finally
        {
            ReleaseSfxSlot(id);
        }
    }

    #endregion

    #region BGM Logic

    private void PlayBgmInternal(SoundId id, bool loop)
    {
        if (!initialized || !soundBoxLoaded)
        {
            Debug.LogWarning("[SoundManager] 아직 초기화되지 않았습니다. InitializeAsync() 먼저 호출하세요.");
            return;
        }

        if (!enableBgm) return;
        if (bgmSource == null) return;

        var clip = soundBox.GetClip(id);
        if (clip == null)
        {
            Debug.LogWarning($"[SoundManager] BGM clip not found for SoundId: {id}");
            return;
        }

        bgmSource.loop = loop;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    private void StopBgmInternal()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    #endregion
}
