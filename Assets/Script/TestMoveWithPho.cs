using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoveWithPho : MonoBehaviourPun
{
    public float speed = 10f;

    void Update()
    {
        if (photonView.IsMine)
        {
            float move = Input.GetAxis("Vertical") * speed * Time.deltaTime;
            transform.Translate(0, move, 0);
        }
    }
}
