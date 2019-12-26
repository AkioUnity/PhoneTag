using System.Collections;
using System.Collections.Generic;
using GoShared;
using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Material RedMat;
    public Material GreenMat;
    public static PlayerManager Inst;
    public MoveAvatar moveAvatar;
    public Player user;
    public Player npc;
    
    private void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        moveAvatar.avatarFigure=PhotonNetwork.Instantiate("Player", new Vector3(0,10.74f,0), Quaternion.identity, 0);
        user = moveAvatar.avatarFigure.GetComponent<Player>();
        user.transform.SetParent(moveAvatar.transform);
        GOOrbit.Inst.target = user.transform;

        if (!GameManager.Inst.isMultiMode)
        {
            npc = Instantiate(npc);
            npc.isNpc = true;
            npc.isRed = false;
            npc.ChangeColor();
        }
    }

    public Material GetMaterial(bool isRed)
    {
        return (isRed)?RedMat:GreenMat;
    }
}
