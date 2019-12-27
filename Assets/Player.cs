using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Renderer[] parts;
    public bool isRed;
    public bool isNpc;
    public bool isMe;
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
        if (isNpc)
        {
            NPCMove();   
        }
        
        if (!PlayerManager.isGameStarted)
            return;

        if (!isRed)
        {
            PlayerManager.Inst.CheckTag(this);
        }
    }

    void NPCMove()
    {
        float dist = Vector3.Distance(PlayerManager.Inst.user.transform.position, transform.position);
        if (dist > PlayerManager.MaxDistance && !isRed)
            transform.position = PlayerManager.Inst.RandomCircle();
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
        if (isMe)
        {
            if (isRed)
                UIManager.Inst.areIt.OpenCloseObjectAnimation();
            else
                UIManager.Inst.avoidRed.OpenCloseObjectAnimation();
        }
    }
}
