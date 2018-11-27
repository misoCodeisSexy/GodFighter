using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeControlScript : MonoBehaviour
{
    public GameObject Option;   //옵션
    public GameObject[] Time_Image_1;   //시간의 10의자리
    public GameObject[] Time_Image_2;   //시간의 1의자리
    public static TimeControlScript _Option;
    public float Present_Time;  //현재 시간
    public int Total_Time;              //총 시간 = 99초

    [SerializeField] private GameObject infinityTime;
    private int image1_time = 9;    //10의자리 숫자 초기화
    private int image2_time = 9;    //1의자리 숫자 초기화
    private bool zerotime;
    private bool zerotime_2;

    public bool is_RoundEventEnd = true;

    public void OnStart()
    {
        _Option = this;
        ResetTimeDatas(CSceneManager.Instance.roundOptions.timer);
    }

    public void FixedUpdateGOF()
    {
        // 트레이닝 모드일 시, 무한대로 처리
        if (CGameManager.gameMode == GodFighter.GameMode.TrainingRoom)
        {
            for (int i = 0; i < Time_Image_1.Length; i++)
            {
                Time_Image_1[i].SetActive(false);
            }
            for (int i = 0; i < Time_Image_2.Length; i++)
            {
                Time_Image_2[i].SetActive(false);
            }
            infinityTime.SetActive(true);

            return;
        }

        //UI이미지로 시간 띄우기
        if (!is_RoundEventEnd)
        {
            Present_Time -= Time.fixedDeltaTime;

            if (Present_Time < 0)
            {
                Present_Time = 0;
                is_RoundEventEnd = true;
            }

            image1_time = (int)Present_Time / 10;
            image2_time = (int)Present_Time - (image1_time * 10);

            Test_Time(image1_time, image2_time);
        }
    }

    public void ResetTimeDatas(float time)
    {
        for (int i = 0; i < Time_Image_1.Length; i++)
        {
            Time_Image_1[i].SetActive(false);
        }
        for (int i = 0; i < Time_Image_2.Length; i++)
        {
            Time_Image_2[i].SetActive(false);
        }

        Present_Time = time;

        image1_time = (int)Present_Time / 10;
        image2_time = (int)Present_Time - (image1_time * 10);
        Test_Time();
    }

    public void Test_Time()
    {
        Test_Time(image1_time, image2_time);
    }

    public void Test_Time(int image1_time , int image2_time)
    {      
        SimpleTimer(image1_time, ref Time_Image_1, 0);
        SimpleTimer(image2_time, ref Time_Image_2, 1);
    }
    public void Test_TimerInit()
    {
        InitSimpleTimer();
    }

    private void SimpleTimer(int _TimeSet, ref GameObject[] Image, int operater)
    {
        if ((_TimeSet) > Image.Length)
        {
            return;
        }
        else if (((_TimeSet + 1) == Image.Length) && zerotime_2 == false)
        {
            Image[0].SetActive(false);
        }

        if (_TimeSet < 0 || zerotime_2 == true)
        {
            return;
        }

        if (operater == 0)
        {
            Image[_TimeSet].SetActive(true);

            if ((_TimeSet + 1) < Image.Length)
            {
                Image[_TimeSet + 1].SetActive(false);
            }

            if (_TimeSet == 0)
            {
                zerotime = true;
            }
        }
        else if (operater == 1)
        {
            Image[_TimeSet].SetActive(true);

            if ((_TimeSet + 1) < Image.Length)
            {
                Image[_TimeSet + 1].SetActive(false);
            }
        }
    }

    // 심플타이머 초기상태 초기화
    private void InitSimpleTimer()
    {
        zerotime = false;
        zerotime_2 = false;
    }
}
