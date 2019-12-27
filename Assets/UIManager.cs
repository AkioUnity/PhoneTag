using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    static public UIManager Inst;
    public EasyTween areIt;
    public EasyTween avoidRed;
    private void Awake()
    {
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnExitBtn()
    {
        SceneManager.LoadScene("Intro");
    }
}
