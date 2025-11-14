using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressManager : Singleton<AddressManager>
{
    private Dictionary<object, object> loadedAssetDictionary = new Dictionary<object, object>();

    /// <summary>
    /// 어드레서블 다운로드 메서드.
    /// </summary>
    public void DownloadDependenciesAsync<T>(object key, Action<T> onLoaded = null) where T : UnityEngine.Object
    {
        Addressables.GetDownloadSizeAsync(key).Completed += (opSize) =>
        {
            if (opSize.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[Addressables] 다운로드 크기 확인 실패: {key}");
                return;
            }

            if (opSize.Result > 0)
            {
                Addressables.DownloadDependenciesAsync(key, true).Completed += (opDownload) =>
                {
                    if (opDownload.Status == AsyncOperationStatus.Succeeded)
                    {
                        LoadAssetAsync<T>(key, onLoaded);
                    }
                    else
                    {
                        Debug.LogError($"[Addressables] 다운로드 실패: {key}");
                    }
                };
            }
            else
            {
                LoadAssetAsync<T>(key, onLoaded);
            }
        };
    }

    /// <summary>
    /// 어드레서블 로드 메서드
    /// </summary>
    public void LoadAssetAsync<T>(object key, Action<T> onLoaded = null) where T : UnityEngine.Object
    {
        if (loadedAssetDictionary.TryGetValue(key, out var cachedAsset))
        {
            if (cachedAsset is T asset)
            {
                onLoaded?.Invoke(asset);
                return;
            }
            else
            {
                Debug.LogWarning($"[Addressables] 캐시된 에셋의 타입이 다릅니다. key: {key}, expected: {typeof(T)}");
            }
        }

        Addressables.LoadAssetAsync<T>(key).Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                T loadedAsset = op.Result;
                loadedAssetDictionary[key] = loadedAsset;
                onLoaded?.Invoke(loadedAsset);
            }
            else
            {
                Debug.LogError($"[Addressables] 로드 실패: {key}");
                Debug.LogError($"[Addressables] 다운로드 시도: {key}");
                DownloadDependenciesAsync<T>(key, onLoaded);
            }
        };
    }

    /// <summary>
    /// 대기용 로드 메서드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T> LoadAssetAsyncTask<T>(string key)
    {
        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;
        return handle.Result;
    }
}
