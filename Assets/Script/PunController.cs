using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class PunController : MonoBehaviourPunCallbacks
{
    static public PunController Inst;
    private TypedLobby _defaultLobby = new TypedLobby("rummyLobby", LobbyType.SqlLobby);
    public Dictionary<string, RoomInfo> cachedRoomList;
    private int mTierIdx;


    public void Awake()
    {
        if (!Inst)
            Inst = this;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void Start()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();
        Login();
    }

    public void Login()
    {
        string playerName = "User_"+Random.Range(0,200);

        if (!playerName.Equals("") || !PhotonNetwork.IsConnected)
        {
            Debug.Log("!PhotonNetwork.IsConnected");
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }

    public void LeaveGame()
    {
        Debug.Log("leave game");
        PhotonNetwork.LeaveRoom();
    }

    public void CreateOrJoinRoom(int tierIdx)
    {
        mTierIdx = tierIdx;
        string roomName = "tag_" + mTierIdx.ToString();
        bool isNewRoom = true;
        Debug.Log("cachedRoom : " + cachedRoomList.Count);
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            if (info.Name.Contains(roomName))
            {
                isNewRoom = false;
                JoinRoom(info.Name);
            }
            else
            {
                Debug.Log(info.Name + "can't join");
            }
        }
        if (isNewRoom)
        {
            CreateRoom(roomName);
        }
    }
    
    public void CreateRoom(string roomName, byte maxPlayers = 8, string AdditionalRoomProperty = "")
    {
        roomName = roomName + "_" + DateTime.Now.ToString("MMddHHmmss");

        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName, null);
    }
    
    public void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            Debug.Log("roomList" + info);
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList || info.PlayerCount == info.MaxPlayers)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    Debug.Log("cachedRoomList.Remove" + info.Name);
                    cachedRoomList.Remove(info.Name);
                }
                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
                Debug.Log("ContainsKey(info.Name)" + info);
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
                Debug.Log("else" + info);
            }
        }
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        else
        {
            Intro.Inst.CheckStep();
        }
    }

    #region PUN CALLBACKS

    public override void OnConnectedToMaster()
    {
        Debug.Log("onConnectedMaster LobbyPun");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby LobbyPun");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Tag");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("join room failed");
        string roomName = "rummy_" + mTierIdx.ToString();
        //CreateRoom(roomName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
    }
    
    public override void OnLeftLobby()
    {
        Debug.Log("OnLeftLobby");
        cachedRoomList.Clear();
    }

    public override void OnLeftRoom()
    {
        
    }

    public override void OnRoomPropertiesUpdate(Hashtable updatedInfo)
    {
     
    }

    #endregion

    #region UI CALLBACKS

    #endregion
}