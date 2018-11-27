using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine;
using Spine.Unity;
using GodFighter;

public class SpineController : MonoBehaviour
{
    public CharacterNames name;
    public GameObject Spine_1;
    public GameObject Spine_2;
    public GameObject Spine_3;
    public float SpainTime_1;
    public float SpainTime_2;
    public float SpainTime_3;
    public float Ui_OnTime;
    public CGameManager mgr;

    private void Awake()
    {
        mgr = GameObject.FindGameObjectWithTag("MainManager").GetComponent<CGameManager>();
        mgr.enableTimeUpdate = false;
        mgr.defaultUiCtrl.gameObject.SetActive(false);

        if (name == CharacterNames.miho)
        {
            myDataInit(0.0f, 1.5f, 3.1f, 5.5f);
            PlayMihoSpine();
        }
        else if(name == CharacterNames.agni)
        {
            myDataInit(0.0f, 0.2f, 3.6f, 4.8f);
            PlayAgniSpine();
        }
        else if (name == CharacterNames.valkiri)
        {
            myDataInit(0.0f, 2.16f, 4.3f, 6.0f);
            PlayValkiriSpine();
        }
    }

    private void myDataInit(float time1, float time2, float time3, float uiTime)
    {
        SpainTime_1 = time1;
        SpainTime_2 = time2;
        SpainTime_3 = time3;
        Ui_OnTime = uiTime;
    }
    #region Agni
    public void PlayAgniSpine()
    {
        Invoke("PlayAgniSpine_1", SpainTime_1);
        Invoke("PlayAgniSpine_2", SpainTime_2);
        Invoke("PlayAgniSpine_3", SpainTime_3);
        Invoke("Ui_On", Ui_OnTime);
    }

    private void PlayAgniSpine_1()
    {
        Spine_1.SetActive(true);
    }

    private void PlayAgniSpine_2()
    {
        Spine_2.SetActive(true);

        Sequence Play_AgniSpine_2 = DOTween.Sequence();
        Play_AgniSpine_2.Insert(0, Spine_2.transform.DOMoveX(Screen.width * 2, 0.0f));
        Play_AgniSpine_2.Insert(0.2f, Spine_2.transform.DOMoveX(Screen.width / 2, 0.7f));
        Play_AgniSpine_2.Insert(1.5f, Spine_2.transform.DOMoveX(Screen.width * -2, 0.7f));
    }

    private void PlayAgniSpine_3()
    {
        Spine_2.SetActive(false);
        Spine_3.SetActive(true);
    }
    #endregion

    #region miho
    public void PlayMihoSpine()
    {
        Invoke("PlayMihoSpine_1", SpainTime_1);
        Invoke("PlayMihoSpine_2", SpainTime_2);
        Invoke("PlayMihoSpine_3", SpainTime_3);
        Invoke("Ui_On", Ui_OnTime);
    }

    private void PlayMihoSpine_1()
    {
        Spine_1.SetActive(true);
    }

    private void PlayMihoSpine_2()
    {
        Spine_2.SetActive(true);
    }

    private void PlayMihoSpine_3()
    {
        Spine_3.SetActive(true);
    }
    #endregion

    #region valkiri
    public void PlayValkiriSpine()
    {
        Invoke("PlayValkiriSpine_1", SpainTime_1);
        Invoke("PlayValkiriSpine_2", SpainTime_2);
        Invoke("PlayValkiriSpine_2_1", SpainTime_2 + 0.13f);
        Invoke("PlayValkiriSpine_3", SpainTime_3);
        Invoke("Ui_On", Ui_OnTime);
    }

    private void PlayValkiriSpine_1()
    {
        Spine_1.SetActive(true);
    }
    private void PlayValkiriSpine_2()
    {
        Spine_2.SetActive(true);
    }
    private void PlayValkiriSpine_2_1()
    {
        Spine_1.SetActive(false);
    }
    private void PlayValkiriSpine_3()
    {
        Spine_3.SetActive(true);
    }
    #endregion

    void Ui_On()
    {
        mgr.enableTimeUpdate = true;
        mgr.defaultUiCtrl.gameObject.SetActive(true);

        mgr.P1_Left.OnReleaseCam();
        mgr.P2_Right.OnReleaseCam();

        Destroy(gameObject);
    }

}
