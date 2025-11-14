using ExitGames.Client.Photon;
using KeyValues;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeamManager : SingletonServer<TeamManager>
{
    /// <summary>
    /// 해당 플레이어의 팀을 반환(TeamType으로) 없으면 None으로 처리.
    /// </summary>
    /// <returns></returns>
    public TeamType GetTeam(Player player)
    {
        if (PhotonNetwork.CurrentRoom == null) return TeamType.None;

        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        if (props.ContainsKey(Team.BLACK_TEAM_KEY))
        {
            var black = (string[])props[Team.BLACK_TEAM_KEY];
            if (System.Array.Exists(black, name => name == player.NickName))
                return TeamType.Black;
        }

        if (props.ContainsKey(Team.WHITE_TEAM_KEY))
        {
            var white = (string[])props[Team.WHITE_TEAM_KEY];
            if (System.Array.Exists(white, name => name == player.NickName))
                return TeamType.White;
        }

        return TeamType.None;
    }

    /// <summary>
    /// 해당 팀의 n번째 플레이어를 반환
    /// </summary>
    /// <param name="type"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public string GetPlayerNameByTeam(TeamType type, int index = 0)
    {
        if (PhotonNetwork.CurrentRoom == null) return null;

        Dictionary<string, string[]> props = GetRoomTeams();

        if (index < 0 || index >= props[TeamTypeToKey(type)].Count())
            return null;
        return props[TeamTypeToKey(type)][index];
    }

    /// <summary>
    /// 자신의 팀을 반환.
    /// </summary>
    /// <returns></returns>
    public TeamType GetMyTeam()
    {
        return GetTeam(PhotonNetwork.LocalPlayer);
    }

    /// <summary>
    /// 클라이언트가 방장에게 팀 배정을 요청(RPC)
    /// </summary>
    public void RequestAssignTeam(Player player)
    {
        photonView.RPC("RPC_AssignTeam", RpcTarget.MasterClient, player.NickName);
    }

    /// <summary>
    /// 현재 방의 정보를 로컬에서 조회
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string[]> GetRoomTeams()
    {
        Dictionary<string, string[]> result = new Dictionary<string, string[]>();

        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogWarning("현재 방이 존재하지 않습니다.");
            result[Team.BLACK_TEAM_KEY] = new string[0];
            result[Team.WHITE_TEAM_KEY] = new string[0];
            return result;
        }

        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        if (props == null)
        {
            Debug.LogWarning("CustomProperties가 아직 초기화되지 않았습니다.");
            result[Team.BLACK_TEAM_KEY] = new string[0];
            result[Team.WHITE_TEAM_KEY] = new string[0];
            return result;
        }

        if (props.ContainsKey(Team.BLACK_TEAM_KEY))
            result[Team.BLACK_TEAM_KEY] = (string[])props[Team.BLACK_TEAM_KEY];
        else
            result[Team.BLACK_TEAM_KEY] = new string[0];

        if (props.ContainsKey(Team.WHITE_TEAM_KEY))
            result[Team.WHITE_TEAM_KEY] = (string[])props[Team.WHITE_TEAM_KEY];
        else
            result[Team.WHITE_TEAM_KEY] = new string[0];

        return result;
    }

    public void AssignTeam(Player player, TeamType type)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("방장만 배정가능합니다!");
            return;
        }

        string key = type == TeamType.Black ? Team.BLACK_TEAM_KEY : Team.WHITE_TEAM_KEY;
        string otherKey = type == TeamType.Black ? Team.WHITE_TEAM_KEY : Team.BLACK_TEAM_KEY;

        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        // 현재 팀 상태 가져오기
        List<string> selectTeamInfo = props.ContainsKey(key) ? new List<string>((string[])props[key]) : new List<string>();
        List<string> otherTeamInfo = props.ContainsKey(otherKey) ? new List<string>((string[])props[otherKey]) : new List<string>();

        //선택한 팀에 아무도 없다면 할당
        if (selectTeamInfo.Count >= 1)
        {
            //해당 팀에 있는 플레이어와 자리 교대.
            string existPlayer = selectTeamInfo[0];
            selectTeamInfo.Clear();
            selectTeamInfo.Add(player.NickName);

            otherTeamInfo.Clear();
            otherTeamInfo.Add(existPlayer);

            Debug.Log($"{player.NickName} 과 {existPlayer} 님의 자리 교체)");
            return;
        }

        selectTeamInfo.Add(player.NickName);

        Hashtable newProps = new Hashtable
        {
            {key,selectTeamInfo.ToArray() },
            {otherKey,otherTeamInfo.ToArray() }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(newProps);

        Debug.Log($"{player.NickName}이 {type.ToString()}에 배정되었습니다.");
    }

    /// <summary>
    /// 플레이어 방에서 제거.
    /// </summary>
    /// <param name="player"></param>
    public void RemovePlayerFromTeam(Player player)
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        var keyEnum = GetTeam(player);

        if (keyEnum == TeamType.None)
        {
            Debug.Log("해당 Player는 팀에 존재하지 않습니다.");
            return;
        }

        var key = keyEnum.ToString();

        List<string> black = props.ContainsKey(Team.BLACK_TEAM_KEY) ? new List<string>((string[])props[Team.BLACK_TEAM_KEY]) : new List<string>();
        List<string> white = props.ContainsKey(Team.WHITE_TEAM_KEY) ? new List<string>((string[])props[Team.WHITE_TEAM_KEY]) : new List<string>();

        if (black.Contains(player.NickName))
            black.Remove(player.NickName);
        else if (white.Contains(player.NickName))
            white.Remove(player.NickName);

        Dictionary<string, bool> readyStates = props.ContainsKey(Team.READY_KEY) ? (Dictionary<string, bool>)props[Team.READY_KEY] : readyStates = new Dictionary<string, bool>();

        if (readyStates.ContainsKey(key))
        {
            readyStates[key] = false;
        }
        else
        {
            Debug.Log("Key가 존재하지 않습니다[준비상태]");
        }

        Hashtable newProps = new Hashtable
    {
        { Team.BLACK_TEAM_KEY, black.ToArray() },
        { Team.WHITE_TEAM_KEY, white.ToArray() },
        { Team.READY_KEY,readyStates }
    };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newProps);
    }

    /// <summary>
    /// 방장이 실제로 팀 배정
    /// </summary>
    [PunRPC]
    private void RPC_AssignTeam(string playerName)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        List<string> blackTeam = props.ContainsKey(Team.BLACK_TEAM_KEY) ? new List<string>((string[])props[Team.BLACK_TEAM_KEY]) : new List<string>();
        List<string> whiteTeam = props.ContainsKey(Team.WHITE_TEAM_KEY) ? new List<string>((string[])props[Team.WHITE_TEAM_KEY]) : new List<string>();

        // 빈 팀 우선 배정
        if (blackTeam.Count == 0)
            blackTeam.Add(playerName);
        else if (whiteTeam.Count == 0)
            whiteTeam.Add(playerName);
        else
        {
            Debug.LogWarning("모든 팀이 가득 찼습니다!");
            return;
        }

        // CustomProperties 갱신
        Hashtable newProps = new Hashtable
        {
            { Team.BLACK_TEAM_KEY, blackTeam.ToArray() },
            { Team.WHITE_TEAM_KEY, whiteTeam.ToArray() }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(newProps);

        Debug.Log($"{playerName} 팀 배정 완료: Black({blackTeam.Count}) / White({whiteTeam.Count})");
    }

    /// <summary>
    /// 빈 공간 정리
    /// </summary>
    /// <param name="playerName"></param>
    public void CleanupEmptySlots(string playerName)
    {
        var props = PhotonNetwork.CurrentRoom.CustomProperties;
        List<string> black = props.ContainsKey(Team.BLACK_TEAM_KEY) ? new List<string>((string[])props[Team.BLACK_TEAM_KEY]) : new List<string>();
        List<string> white = props.ContainsKey(Team.WHITE_TEAM_KEY) ? new List<string>((string[])props[Team.WHITE_TEAM_KEY]) : new List<string>();

        black.Remove(playerName);
        white.Remove(playerName);

        Hashtable newProps = new Hashtable
        {
            { Team.BLACK_TEAM_KEY, black.ToArray() },
            { Team.WHITE_TEAM_KEY, white.ToArray() }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(newProps);
        Debug.Log($"{playerName} 제거 완료. 남은 인원: 흑 {black.Count}, 백 {white.Count}");
    }

    /// <summary>
    /// 준비 상태 가져오기
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public bool GetTeamReadyState(TeamType team)
    {
        var props = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!props.ContainsKey(Team.READY_KEY))
            return false;

        var readyStates = (Dictionary<string, bool>)props[Team.READY_KEY];
        string key = team.ToString();
        return readyStates.ContainsKey(key) && readyStates[key];
    }

    /// <summary>
    /// 준비 상태의 변경
    /// </summary>
    /// <param name="type"></param>
    public void ToggleTeamReady(TeamType team)
    {
        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        Dictionary<string, bool> readyStates;
        if (props.ContainsKey(Team.READY_KEY))
            readyStates = (Dictionary<string, bool>)props[Team.READY_KEY];
        else
            readyStates = new Dictionary<string, bool>();

        string key = team.ToString();
        readyStates[key] = !GetTeamReadyState(team);

        Hashtable newProps = new Hashtable
        {
            { Team.READY_KEY, readyStates }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(newProps);
    }
    
    /// <summary>
    /// 플레이어 강퇴 동작
    /// </summary>
    /// <param name="player"></param>
    public void KickPlayer(Player player)
    {
        Debug.Log($"Kick {player.NickName}, isMaster={PhotonNetwork.IsMasterClient}");

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (player == null)
            return;

        photonView.RPC(nameof(RPC_KickPlayer), player);
    }

    [PunRPC]
    private void RPC_KickPlayer()
    {
        Debug.Log("당신은 강퇴당했습니다.");

        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// 닉네임으로 플레이어 반환 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Player GetPlayerByNickName(string name)
    {
        return PhotonNetwork.PlayerList.FirstOrDefault(p => p.NickName == name);
    }

    /// <summary>
    /// 팀 타입을 키 형태로.
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public string TeamTypeToKey(TeamType team)
    {
        if (team == TeamType.None)
            return null;
        return team == TeamType.White ? Team.WHITE_TEAM_KEY : Team.BLACK_TEAM_KEY;
    }
}
