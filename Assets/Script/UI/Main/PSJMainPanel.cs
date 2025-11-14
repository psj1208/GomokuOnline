using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PSJMainPanel : MonoBehaviourPunCallbacks
{
    [Header("Head")]
    [SerializeField] List<GameObject> Panels = new List<GameObject>();
    [Header("About LoginLobby")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button loginButton;
    [Space]
    [Header("About RoomLobby")]
    [SerializeField] private GameObject roomListPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button JoinButton;
    [SerializeField] private RoomButton selectedRoomButton;
    public RoomButton SelectedRoomButton { set { SelectedRoomButton = value; } }
    [SerializeField] private string selectedRoomName;
    public string SelectedRoomName { set { selectedRoomName = value; } }
    [Space]
    [Header("About GameRoom")]
    [SerializeField] private GameObject gameRoomPanel;
    [SerializeField] private GameRoomPanel gameRoomScript;

    private Dictionary<string,RoomInfo> cachedRoomList = new Dictionary<string,RoomInfo>();

    // Start is called before the first frame update
    void Start()
    {
        Panels.Add(loginPanel);
        Panels.Add(roomListPanel);
        Panels.Add(gameRoomPanel);
        PhotonNetwork.AutomaticallySyncScene = false;
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        createRoomButton.onClick.AddListener(OnCreateButtonClicked);
        JoinButton.onClick.AddListener(RoomJoin);
        foreach (var obj in Panels)
            obj.SetActive(false);
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InLobby)
                ControlPanel(1);
            else if (PhotonNetwork.InRoom)
                ControlPanel(2);
        }
        else
        {
            ControlPanel(0);
        }
    }
    /// <summary>
    /// 패널 전부 종료.
    /// </summary>
    public void TurnOffAllUI()
    {
        foreach (var obj in Panels)
            obj.SetActive(false);
    }
    /// <summary>
    /// 0.로그인 패널
    /// 1.룸 리스트 패널
    /// 2.게임 룸 패널
    /// </summary>
    public void ControlPanel(int num, bool boolValue = true)
    {
        if (num >= Panels.Count)
            return;
        for (int i = 0; i < Panels.Count; i++)
        {
            if (i == num)
            {
                Panels[i].SetActive(boolValue);
                if (UIManager.Instance.TryGet<TitleImageUI>(out TitleImageUI ui))
                {
                    Destroy(ui.gameObject);
                }

                if (i == 0)
                {
                    UIManager.Instance.show<TitleImageUI>();
                }
                else if (i == 2)
                {
                    gameRoomScript.InitUI(this);
                }
            }
            else
                Panels[i].SetActive(false);
        }
    }

    void OnLoginButtonClicked()
    {
        string nickname = nicknameInput.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            Debug.Log("닉네임이 비어있습니다");
            return;
        }

        PhotonNetwork.NickName = nickname;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log($"로그인 성공 : {PhotonNetwork.NickName}");
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("연결 실패: {cause}");
        ControlPanel(0);
    }

    public override void OnJoinedLobby()
    {
        ControlPanel(1);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomInfo info in roomList)
        {
            if(info.RemovedFromList)
            {
                if(cachedRoomList.ContainsKey(info.Name))
                    cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }

        RefreshRoomList();
    }

    private async void RefreshRoomList()
    {
        foreach(Transform child in contentParent)
            Destroy(child.gameObject);

        int count = 0;
        foreach (var room in cachedRoomList)
        {
            GameObject newButton = Instantiate(roomPrefab, contentParent);
            newButton.GetComponent<RoomButton>().SetUp(room.Value, this);

            count++;

            // 한 프레임마다 10개씩만 생성
            if (count % 10 == 0)
                await Task.Yield();
        }

        Debug.Log($"[RoomList] 총 {cachedRoomList.Count}개 방 UI 생성 완료");
    }

    private void OnCreateButtonClicked()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("아직 Photon 서버에 연결되지 않았습니다!");
            return;
        }

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true
        };
        PhotonNetwork.CreateRoom(PhotonNetwork.NickName, options);
        ControlPanel(1, false);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 완료");
        ControlPanel(2);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 생성 실패");
        ControlPanel(1);
    }

    void RoomJoin()
    {
        if (selectedRoomButton != null)
        {
            TurnOffAllUI();
            PhotonNetwork.JoinRoom(selectedRoomName);
        }
    }

    public override void OnJoinedRoom()
    {
        TeamManager.Instance.RequestAssignTeam(PhotonNetwork.LocalPlayer);
        AudioManager.Instance.Audio2DPlay("JoinSound", 1, false, EAudioType.SFX);
        ControlPanel(2);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        ControlPanel(1);
    }

    public void RoomSelect(RoomButton rButton)
    {
        if (selectedRoomButton != null)
            selectedRoomButton.SetSelected(false);

        selectedRoomButton = rButton;
        selectedRoomButton.SetSelected(true);
        selectedRoomName = rButton.RoomName;
    }
}
