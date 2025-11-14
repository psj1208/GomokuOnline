using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;

//싱글턴 (멀티 담당 매니저용)
//해당 매니저를 호출 시에 씬에 존재하지 않으면 만들고 반환함.
//IsAlive는 해당 스크립트가 이미 존재하는 지에 대해 반환함. Instance로 바로 접근하면 만들어서 주기 때문.
public abstract class SingletonServer<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
{
    protected virtual bool dontDestroy => true;
    private static T _instance;
    public static bool IsAlive;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                IsAlive = true;
                Create();
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        Create();

        if (_instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            IsAlive = true;

            if (dontDestroy)
                DontDestroyOnLoad(this);
        }
    }

    protected static void Create()
    {
        if (_instance == null)
        {
            T[] objects = FindObjectsOfType<T>();

            if (objects.Length > 0)
            {
                _instance = objects[0];

                for (int i = 1; i < objects.Length; ++i)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(objects[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(objects[i].gameObject);
                    }
                }
            }
            else
            {
                GameObject go = new GameObject(string.Format("{0}", typeof(T).Name));
                _instance = go.AddComponent<T>();
                PhotonViewUtil.ResetPhotonView<T>(go);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            IsAlive = false;
        }
    }

    public override void OnJoinedRoom()
    {
        PhotonViewUtil.ResetPhotonView<T>(gameObject);
    }
}
