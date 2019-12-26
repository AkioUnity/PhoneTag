﻿using System.Collections;
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
    public Player tagUser;
    
    public static float tagDistance = 10;
    public static float WaitTime = 5f;
    public static bool isGameStarted = false;
    public static float Scale = 1.9f;
    public static float MaxDistance = 20*Scale;
    
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
            tagUser = user;
            tagUser.SetTag(true);
            StartCoroutine(SpawnNPC());
        }
    }

    IEnumerator SpawnNPC()
    {
        yield return new WaitForSeconds(2);
        npc = Instantiate(npc);
        npc.isNpc = true;
        npc.SetTag(false);
        npc.transform.position = RandomCircle(user.transform.position);
        isGameStarted = true;
    }

    public Material GetMaterial(bool isRed)
    {
        return (isRed)?RedMat:GreenMat;
    }
    
    Vector3 RandomCircle ( Vector3 center)
    {
        float radius = (tagDistance+2)*Scale;
        float ang = Random.value * 360;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.y = center.y;
        return pos;
    }

    public void CheckTag(Player user0)
    {
        float dist = Vector3.Distance(user0.transform.position, tagUser.transform.position);
        if (dist < tagDistance*Scale)
        {
            ChangeTagUser(user0);
        }
    }

    public void ChangeTagUser(Player user0)
    {
        Debug.Log("ChangeTag");
        tagUser.SetTag(false);
        tagUser = user0;
        tagUser.SetTag(true);
        StartCoroutine(WaitStartGame());
    }

    IEnumerator WaitStartGame()
    {
        isGameStarted = false;
        yield return new WaitForSeconds(WaitTime*60);
        isGameStarted = true;
    }

    private void Update()
    {
        
    }
}

