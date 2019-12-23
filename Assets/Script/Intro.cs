using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{

    public Image m_LoadingProgress;
    public float m_ProgPer = 0;

    public void Start()
    {
        StartCoroutine(ProgWork());
    }
    
    private IEnumerator ProgWork()
    {
        while (m_ProgPer < 1)
        {
            m_ProgPer += Time.deltaTime*0.2f;
            SetProgBar(m_ProgPer);
            yield return null;
        }
    }

    private void SetProgBar(float per)
    {
        if (m_LoadingProgress == null)
            return;
        m_LoadingProgress.fillAmount = per;
    }
}
