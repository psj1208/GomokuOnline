using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardManager : SingletonServer<BoardManager>
{
    protected override bool dontDestroy => false;
    private const int boardSize = 15;
    private const float boardBound = 0.5f;
    [SerializeField] private int[,] board = new int[boardSize, boardSize];
    private Dictionary<(int, int), Stone> stones = new Dictionary<(int, int), Stone>();

    [SerializeField] private GameObject blackStonePrefab;
    [SerializeField] private GameObject whiteStonePrefab;
    [SerializeField] private Transform boardImage;
    [SerializeField] private bool gameWin;
    (int, int) startPos;
    (int, int) endPos;
    public bool GameWin { get { return gameWin; } }

    Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(1, -1)
    };

    private void Start()
    {
        //어드레서블로 다운하는걸로.
        AddressManager.Instance.LoadAssetAsync<GameObject>("BlackStone", (prefab) =>
        {
            blackStonePrefab = prefab;
        });

        AddressManager.Instance.LoadAssetAsync<GameObject>("WhiteStone", (prefab) =>
        {
            whiteStonePrefab = prefab;
        });

        gameWin = false;
        boardImage = GameObject.Find("Board").transform;

        MakeBoardLine();
    }

    private void MakeBoardLine()
    {
        float boardWidthWithoutBound = boardImage.localScale.x - boardBound * 2;
        float boardWidthTerm = boardWidthWithoutBound / (boardSize - 1);
        float boardWidthStart = -boardWidthWithoutBound / 2;

        for (int a = 0; a < boardSize; a++)
        {
            Vector3 startPos = new Vector3(boardWidthStart + a * boardWidthTerm, -boardWidthStart, 0);
            Vector3 endPos = new Vector3(boardWidthStart + a * boardWidthTerm, boardWidthStart, 0);

            LineDrawer.SetLine(startPos, endPos, boardImage);
        }

        float boardHeightWithoutBound = boardImage.localScale.y - boardBound * 2;
        float boardHeightTerm = boardHeightWithoutBound / (boardSize - 1);
        float boardHeightStart = boardHeightWithoutBound / 2;

        for (int a = 0; a < boardSize; a++)
        {
            Vector3 startPos = new Vector3(-boardHeightStart, boardHeightStart - a * boardHeightTerm, 0);
            Vector3 endPos = new Vector3(boardHeightStart, boardHeightStart - a * boardHeightTerm, 0);

            LineDrawer.SetLine(startPos, endPos, boardImage);
        }


        //cell 생성 과정
        AddressManager.Instance.LoadAssetAsync<GameObject>("Stone", (prefab) =>
        {
            GameObject cell = prefab;

            for (int b = 0; b <= boardSize; b++)
            {
                for (int a = 0; a <= boardSize; a++)
                {
                    float xPos = boardWidthStart + a * boardWidthTerm;
                    float yPos = boardHeightStart - b * boardHeightTerm;

                    GameObject obj = Instantiate(cell, new Vector2(xPos, yPos), Quaternion.identity);
                    float size = 1 / boardSize;
                    Stone stone = obj.GetComponent<Stone>();
                    stone.Init(a, b);
                    obj.name = "Stone" + $" [{a},{b}]";
                    obj.transform.localScale = new Vector2(.5f, .5f);
                    stones[(a, b)] = stone;
                }
            }
        });
    }

    public void TryPlaceStone(int x, int y, Vector3 pos, TeamType team)
    {
        if (GameWin == true)
        {
            Debug.Log("이미 게임이 종료되었습니다.");
            return;
        }

        if (TurnManager.Instance.GetCurrentTurnTeam() != team)
        {
            Debug.Log("현재 진행 중인 팀이 아닙니다.");
            return;
        }

        if (board[x, y] != 0)
        {
            Debug.Log("해당 위치는 이미 돌이 있음.");
            return;
        }

        photonView.RPC("RPC_PlaceStone", RpcTarget.All, x, y, pos, team);  
    }

    [PunRPC]
    private void RPC_PlaceStone(int x, int y, Vector3 pos, TeamType team)
    {
        board[x, y] = team == TeamType.Black ? 1 : 2;

        GameObject prefab = team == TeamType.Black ? blackStonePrefab : whiteStonePrefab;

        if (prefab == null)
        {
            Debug.LogWarning("돌 프리팹이 지정되지 않았음.");
            return;
        }

        Instantiate(prefab, pos, Quaternion.identity);
        AudioManager.Instance.Audio2DPlay("Place", 1, false, EAudioType.SFX);

        if (CheckWin(x, y))
        {
            gameWin = true;
            WinAction();
            return;
        }

        TurnManager.Instance.NextTurn();
    }

    /// <summary>
    /// 서버 연결 안 했을 때 테스트용 메서드.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="pos"></param>
    /// <param name="team"></param>
    public void PlaceStoneNotServer(int x, int y, Vector3 pos, TeamType team)
    {
        board[x, y] = team == TeamType.Black ? 1 : 2;

        GameObject prefab = team == TeamType.Black ? blackStonePrefab : whiteStonePrefab;

        if (prefab == null)
        {
            Debug.LogWarning("돌 프리팹이 지정되지 않았음.");
            return;
        }

        Instantiate(prefab, pos, Quaternion.identity);
    }

    #region 승리 관련
    private bool CheckWin(int x,int y)
    {
        int stone = board[x, y];
        startPos = (x, y);
        endPos = (x, y);
        if (stone == 0)
            return false;

        foreach (var dir in directions)
        {
            int count = 1;

            int index = 1;
            while (true)
            {
                int nx = x + dir.x * index;
                int ny = y + dir.y * index;

                if (nx < 0 || ny < 0 || nx >= boardSize || ny >= boardSize)
                    break;
                if (board[nx, ny] != stone)
                    break;
                count++;
                index++;
                startPos = (nx, ny);
                if (count >= 5)
                    return true;
            }

            index = 1;
            while (true)
            {
                int nx = x - dir.x * index;
                int ny = y - dir.y * index;

                if (nx < 0 || ny < 0 || nx >= boardSize || ny >= boardSize)
                    break;
                if (board[nx, ny] != stone)
                    break;
                count++;
                index++;
                endPos = (nx, ny);
                if (count >= 5)
                    return true;
            }
        }
        
        return false;
    }

    private void WinAction()
    {
        TeamType winner = TurnManager.Instance.GetCurrentTurnTeam();
        Debug.Log($"{winner.ToString()} 승리!");
        LineDrawer.SetAnimatedLine(stones[startPos].transform.position, stones[endPos].transform.position, 3f, () =>
        {
            UIManager.Instance.show<ResultUI>((ui)=>
            {
                ui.Init(winner,TeamManager.Instance.GetPlayerNameByTeam(winner));
            });
        }, null, Color.green);
    }
    #endregion
}
