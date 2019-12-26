using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Renderer[] parts;
    public bool isRed;
    public bool isNpc;
    
    // Start is called before the first frame update
    void Start()
    {
        Material newMat = Resources.Load("GreenMat", typeof(Material)) as Material;
        if (PhotonNetwork.IsMasterClient)
            newMat = Resources.Load("RedMat", typeof(Material)) as Material;
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].material = newMat;
        }
    }
}
