using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PUNHandler : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void OnLeftLobby()
    {
        Debug.Log("OnLeftLobby");
        SceneManager.LoadScene("Intro");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftLobby");
        SceneManager.LoadScene("Intro");
    }
}
