using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst;
    public bool isMultiMode;
    void Awake()
    {
        if (Inst)
        {
            Destroy(this.gameObject);
            return;
        }
            
        Inst = this;
        DontDestroyOnLoad(this.gameObject);
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
