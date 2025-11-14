using ExitGames.Client.Photon;
using KeyValues;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurnManager : SingletonServer<TurnManager>
{
    protected override bool dontDestroy => false;
    private List<TeamType> TurnList;
    Dictionary<string, string[]> roomPlayer;

    private void Start()
    {
        TurnList = new List<TeamType>
        { TeamType.Black, TeamType.White };
        roomPlayer = TeamManager.Instance.GetRoomTeams();

        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable ht = new Hashtable();
            ht[Team.PLAYER_KEY] = TurnList[0];
            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }
    }

    public void NextTurn()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        TeamType currentIndex = (TeamType)PhotonNetwork.CurrentRoom.CustomProperties[Team.PLAYER_KEY];
        TeamType nextIndex = currentIndex == TeamType.Black ? TeamType.White : TeamType.Black;

        Hashtable ht = new Hashtable();
        ht[Team.PLAYER_KEY] = nextIndex;
        PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(Team.PLAYER_KEY))
        {
            TeamType currentTurn = (TeamType)PhotonNetwork.CurrentRoom.CustomProperties[Team.PLAYER_KEY];
            Debug.Log($"현재 턴은 {currentTurn}");
        }
    }

    public TeamType GetCurrentTurnTeam()
    {
        return (TeamType)PhotonNetwork.CurrentRoom.CustomProperties[Team.PLAYER_KEY];
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName}이(가) 나감");

        // 방장만 팀 정보 수정 (중복 방지)
        if (PhotonNetwork.IsMasterClient)
        {
            TeamManager.Instance.RemovePlayerFromTeam(otherPlayer);
        }

        SceneLoadManager.Instance.SceneMove("PSJLobbyScene");
    }
}
