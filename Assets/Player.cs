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
        
    }

    public void ChangeColor()
    {
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].material = PlayerManager.Inst.GetMaterial(isRed);
        } 
    }
}
