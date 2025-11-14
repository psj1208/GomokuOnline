using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : SingletonServer<SceneLoadManager>
{
    public delegate void SceneLoaded();
    public SceneLoaded OnSceneLoaded;

    public void SceneMove(string sceneName)
    {
        photonView.RPC(nameof(RPC_SceneMove), RpcTarget.All, sceneName);
    }

    [PunRPC]
    private void RPC_SceneMove(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        OnSceneLoaded?.Invoke();
    }

    public void PrivateSceneMove(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        OnSceneLoaded?.Invoke();
    }
}
