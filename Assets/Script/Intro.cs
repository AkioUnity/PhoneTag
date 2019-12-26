using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    public GameObject startBtns;
    public GameObject loadingBar;

    public Image m_LoadingProgress;
    public float m_ProgPer = 0;
    public int step = 0;
    public static Intro Inst;

    private void Awake()
    {
        Inst = this;
    }

    public void Start()
    {
        loadingBar.SetActive(true);
        startBtns.SetActive(false);
        step = 0;
        StartCoroutine(ProgWork());
    }
    
    private IEnumerator ProgWork()
    {
        while (m_ProgPer < 1)
        {
            m_ProgPer += Time.deltaTime*0.5f;
            SetProgBar(m_ProgPer);
            yield return null;
        }
        CheckStep();
    }

    private void SetProgBar(float per)
    {
        if (m_LoadingProgress == null)
            return;
        m_LoadingProgress.fillAmount = per;
    }

    public void OnClickStartBtn(bool flag)
    {
        GameManager.Inst.isMultiMode = flag;
        if (flag)
            PunController.Inst.CreateOrJoinRoom(0);
        else
        {
            SceneManager.LoadScene("Tag");
        }
    }

    public void CheckStep()
    {
        step++;
        if (step == 2)
        {
            loadingBar.SetActive(false);
            startBtns.SetActive(true);            
        }
    }
}




