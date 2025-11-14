using Photon.Pun.Demo.PunBasics;
using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    //현재 활성화된 UI를 저장할 리스트.
    [Header("No Need to Allocate")]
    private Dictionary<string, BaseUI> uiList;
    [SerializeField] private Canvas mainCanvas;
    public Canvas MainCanvas { get { return mainCanvas; } }
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    public GraphicRaycaster GraphicRaycaster {  get { return graphicRaycaster; } }

    protected override void Awake()
    {
        base.Awake();
        uiList = new Dictionary<string, BaseUI>();
        GameObject obj = GameObject.Find("MainCanvas");

        SceneManager.sceneLoaded += OnSceneLoaded;
        mainCanvas = obj.GetComponent<Canvas>();
        graphicRaycaster = obj.GetComponent<GraphicRaycaster>();
    }

    //해당 오브젝트를 보여주고 리턴. 또한 활성화 딕셔너리에 추가.
    public void show<T>(Action<T> onShown = null) where T : BaseUI
    {
        if (mainCanvas == null)
            mainCanvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();

        string key = typeof(T).Name;
        AddressManager.Instance.LoadAssetAsync<GameObject>(key, (uiPrefab) =>
        {
            // 로드 완료 후 Instantiate
            T uiInstance = Instantiate(uiPrefab, mainCanvas.transform).GetComponent<T>();

            // 딕셔너리에 저장
            uiList[key] = uiInstance;

            // 콜백
            onShown?.Invoke(uiInstance);
        });
    }

    //활성화된 UI 중 타입에 맞는 UI를 반환.
    public T Get<T>() where T : BaseUI
    {
        string key = typeof(T).Name;
        if (uiList.TryGetValue(key, out BaseUI ui))
        {
            return ui as T;
        }
        Debug.LogWarning($"UI Not Found: {key}");
        return null;
    }
    
    /// <summary>
    /// 활성화된 UI 중 타입에 맞는 UI를 반환하도록 시도한다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryGet<T>(out T result) where T : BaseUI
    {
        string key = typeof(T).Name;
        if (uiList.TryGetValue(key, out BaseUI ui))
        {
            result = ui as T;
            Debug.Log($"[UiManager] : {key} found!");
            return true;
        }
        result = null;
        Debug.LogWarning($"[UiManager] : {key} not found!");
        return false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearList();
    }

    public void ClearList()
    {
        uiList.Clear();
    }

    public void ClearListAndDestroy(BaseUI script = null)
    {
        foreach (var ui in uiList)
        {
            if (ui.Value != null && ui.Value.gameObject != null)
            {
                if (ui.Value != script)
                    Destroy(ui.Value.gameObject);
            }
        }
        ClearList();
    }

    public void RemoveUIInList(string name)
    {
        uiList.Remove(name);
        Debug.Log($"{name} is Delete");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}