using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PhotonViewUtil
{
    public static PhotonView ResetPhotonView<T>(GameObject target) where T : MonoBehaviourPunCallbacks
    {
        //기존 포톤뷰를 삭제.
        PhotonView oldView = target.GetComponent<PhotonView>();
        if (oldView != null)
        {
            Object.DestroyImmediate(oldView);
        }

        //새 포톤뷰를 추가.
        PhotonView newView = target.AddComponent<PhotonView>();

        if(PhotonNetwork.IsConnectedAndReady)
        {
            string key = typeof(T).ToString();
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key))
                {
                    int assignedID = (int)PhotonNetwork.CurrentRoom.CustomProperties[key];
                    newView.ViewID = assignedID;

                    Debug.Log($"[PhotonView] Master : {target.name} ViewID 동기화 완료: {assignedID}");
                }
                else
                {
                    PhotonNetwork.AllocateViewID(newView);

                    Hashtable ht = new Hashtable();
                    ht[key] = newView.ViewID;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(ht);

                    Debug.Log($"[PhotonView] Master : {target.name} 새 viewID 생성 및 동기화 : {newView.ViewID}");
                }
            }

            else
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key))
                {
                    int assignedID = (int)PhotonNetwork.CurrentRoom.CustomProperties[key];
                    newView.ViewID = assignedID;

                    Debug.Log($"[PhotonView] Client : {target.name} ViewID 동기화 완료: {assignedID}");
                }
                else
                {
                    Debug.LogWarning($"[PhotonView] Client : {key} 정보가 아직 동기화되지 않음. 나중에 OnRoomPropertiesUpdate에서 처리 필요");
                }
            }
        }
        return newView;
    }
}
