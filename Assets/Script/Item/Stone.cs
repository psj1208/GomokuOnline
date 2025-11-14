using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public int x { get; private set; }
    public int y { get; private set; }

    public void Init(int x_, int y_)
    {
        x = x_;
        y = y_;
    }

    private void OnMouseDown()
    {
        Debug.Log($"[{x},{y}] µ¹ ´­¸²");

        if (!PhotonNetwork.IsConnected) return;
        TeamType team = TeamManager.Instance.GetMyTeam();
        BoardManager.Instance.TryPlaceStone(x, y, transform.position, team);
    }
}
