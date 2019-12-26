using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Renderer[] parts;
    public bool isRed;
    public bool isNpc;
    public float npcSpeed = 0.5f;
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

    void Update()
    {
        if (!PlayerManager.isGameStarted)
            return;
        if (isNpc)
        {
            NPCMove();   
        }

        if (!isRed)
        {
            PlayerManager.Inst.CheckTag(this);
        }
    }

    void NPCMove()
    {
        float dist = Vector3.Distance(PlayerManager.Inst.user.transform.position, transform.position);
        if (dist > PlayerManager.MaxDistance && !isRed)
            return;

        Vector3 targetDir;
        if (isRed)
            targetDir = PlayerManager.Inst.user.transform.position - transform.position;
        else
            targetDir = transform.position-PlayerManager.Inst.tagUser.transform.position;
        transform.rotation = Quaternion.LookRotation (targetDir);
        transform.Translate(Vector3.forward * Time.deltaTime*npcSpeed);
    }

    public void SetTag(bool flag)
    {
        isRed = flag;
        ChangeColor();
    }
}
