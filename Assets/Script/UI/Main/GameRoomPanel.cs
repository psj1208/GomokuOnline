using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using KeyValues;

public enum TeamType
{
    None,
    Black,
    White
}

public class GameRoomPanel : MonoBehaviourPunCallbacks
{
    [Header("Parent")]
    [SerializeField] private PSJMainPanel mainPanel;
    [Space]
    [Header("Room Name Text")]
    [SerializeField] private TextMeshProUGUI roomNameText;
    [Space]
    [Header("Players")]
    [SerializeField] private List<TeamInfo> teamList;
    [SerializeField] private GameObject playerPrefab;
    [Space]
    [Header("Buttons")]
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;
    Dictionary<string, string[]> roomTeams;

    private void Start()
    {
        leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        startButton.onClick.AddListener(OnStartButtonClicked);
        readyButton.onClick.AddListener(OnReadyButtonClicked);
    }

    public void InitUI(PSJMainPanel panel)
    {
        mainPanel = panel;
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        RefreshPlayerList();
        startButton.gameObject.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);
    }

    private void RefreshPlayerList()
    {
        DestroyAllTrans();

        roomTeams = TeamManager.Instance.GetRoomTeams();

        foreach (var playerName in roomTeams[Team.BLACK_TEAM_KEY])
        {
            if (string.IsNullOrEmpty(playerName)) continue;

            NameTag obj = Instantiate(playerPrefab, teamList[0].Parent).GetComponent<NameTag>();
            obj.SetName(playerName);
        }

        // White 팀
        foreach (var playerName in roomTeams[Team.WHITE_TEAM_KEY])
        {
            if (string.IsNullOrEmpty(playerName)) continue;

            NameTag obj = Instantiate(playerPrefab, teamList[1].Parent).GetComponent<NameTag>();
            obj.SetName(playerName);
        }

        // 팀별 준비 상태 표시
        bool blackReady = TeamManager.Instance.GetTeamReadyState(TeamType.Black);
        bool whiteReady = TeamManager.Instance.GetTeamReadyState(TeamType.White);

        teamList[0].ReadyAct(blackReady);
        teamList[1].ReadyAct(whiteReady);
        Debug.Log($"검은 팀 준비 상태:{blackReady}, 하얀 팀 준비 상태:{whiteReady}");
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(Team.BLACK_TEAM_KEY)
        || propertiesThatChanged.ContainsKey(Team.WHITE_TEAM_KEY)
        || propertiesThatChanged.ContainsKey(Team.READY_KEY))
        {
            RefreshPlayerList();
        }
    }

    public void DestroyAllTrans()
    {
        foreach (var team in teamList)
        {
            int childCount = team.Parent.childCount;
            for (int i = childCount - 1; i >= 0; i--) // 역순으로 삭제
            {
                Destroy(team.Parent.GetChild(i).gameObject);
            }
        }
    }

    #region 버튼 클릭 이벤트
    private void OnLeaveButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    private void OnStartButtonClicked()
    {
        //방장 제한
        if (!PhotonNetwork.IsMasterClient)
            return;

        //인원 확인
        var teams = TeamManager.Instance.GetRoomTeams();
        var blackMembers = teams[Team.BLACK_TEAM_KEY];
        var whiteMembers = teams[Team.WHITE_TEAM_KEY];

        if (blackMembers.Length == 0 || whiteMembers.Length == 0)
        {
            Debug.LogWarning("양쪽 팀의 인원 부족");
            return;
        }

        //준비 상태 확인
        bool blackReady = TeamManager.Instance.GetTeamReadyState(TeamType.Black);
        bool whiteReady = TeamManager.Instance.GetTeamReadyState(TeamType.White);

        if (!blackReady || !whiteReady)
        {
            Debug.LogWarning("준비 상태 미흡");
            return;
        }

        //조건 통과
        TeamManager.Instance.ToggleTeamReady(TeamType.Black);
        TeamManager.Instance.ToggleTeamReady(TeamType.White);
        SceneLoadManager.Instance.SceneMove("GameScene");
    }

    private void OnReadyButtonClicked()
    {
        TeamType myTeam = TeamManager.Instance.GetTeam(PhotonNetwork.LocalPlayer);

        if (myTeam == TeamType.None)
        {
            Debug.LogWarning("팀이 지정되지 않았습니다.");
            return;
        }

        TeamManager.Instance.ToggleTeamReady(myTeam);
    }
    #endregion

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        AudioManager.Instance.Audio2DPlay("JoinSound", 1, false, EAudioType.SFX);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName}이(가) 나감");

        // 방장만 팀 정보 수정 (중복 방지)
        if (PhotonNetwork.IsMasterClient)
        {
            TeamManager.Instance.RemovePlayerFromTeam(otherPlayer);
        }

        AudioManager.Instance.Audio2DPlay("LeaveSound", 1, false, EAudioType.SFX);
        // UI 갱신
        RefreshPlayerList();
    }

    public override void OnLeftRoom()
    {
        //로비 패널로 복귀하도록.
        DestroyAllTrans();
        AudioManager.Instance.Audio2DPlay("LeaveSound", 1, false, EAudioType.SFX);
        PhotonNetwork.JoinLobby();
        mainPanel.ControlPanel(1);
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        startButton.gameObject.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);
    }
}
