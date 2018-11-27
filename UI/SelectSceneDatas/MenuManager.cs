using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rewired;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class MainMenuBackGround
{
    public GameObject map;
    public Vector3 position;

    public GameObject particle;
    public GameObject uiName;
}

public class MenuManager : MonoBehaviour
{
    public Transform mapCanvas;
    public GameObject[] buttonImgs;
    public MainMenuBackGround[] bgObjs;

    [SerializeField] private GameObject optionBackground;
    [SerializeField] private GameObject optionButtons;
    [SerializeField] private GameObject optionPanels;

    [SerializeField] private GameObject controllSetPanel;
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    [SerializeField] private GameObject controllSet1p_keyboard;
    [SerializeField] private GameObject controllSet1p_gamepad;
    [SerializeField] private GameObject controllSet2p_keyboard;
    [SerializeField] private GameObject controllSet2p_gamepad;

    [SerializeField] private GameObject SoundOptionPanel;
    [SerializeField] private GameObject bgmButton;
    [SerializeField] private GameObject sfxButton;
    [SerializeField] private Slider Bgm_Slider;
    [SerializeField] private Slider Sfx_Slider;
    [SerializeField] private GameObject controllButton;
    [SerializeField] private GameObject soundButton;

    [SerializeField] private GameObject controllOff;


    [SerializeField] private GameObject EndGamePanel;
    [SerializeField] private GameObject EndGameYes;
    [SerializeField] private GameObject EndGameNo;

    private int curIdx;

    private bool is_bgmButtonOn = true;
    private bool is_bgmOptionOn = false;
    private bool is_sfxOptionOn = false;
    private float bgmVol;
    private float sfxVol;

    private float timer;
    private const float waitingTime = 0.25f;
    private bool Input_on;

    private bool isOptionPanelOn = false;
    private bool isControllSetPanelOn = false;
    private bool isSoundOptionPanelOn = false;

    private bool isControllButton = true;
    private bool isControllPlayer1p = false;
    private bool isEndGameMenuPanelOn = false;
    private bool isYesOrNo = true;

    public void Start()
    {
        CGameManager.OnInitAudioSystem(false);
        curIdx = 0;
        OnImage(curIdx);
        timer = 0.0f;
        Input_on = true;

        int ran = Random.Range(0, bgObjs.Length);
        GameObject temp = (GameObject)Instantiate(bgObjs[ran].map);
        temp.transform.localPosition = bgObjs[ran].position;

        temp = (GameObject)Instantiate(bgObjs[ran].uiName);
        temp.transform.parent = mapCanvas.transform;
        temp.transform.localPosition = bgObjs[ran].position;
        temp.transform.localScale = Vector3.one;

        if(bgObjs[ran].particle != null) 
            temp = (GameObject)Instantiate(bgObjs[ran].particle);
    }

    void FixedUpdate()
    {
        if (!isControllSetPanelOn && !isSoundOptionPanelOn && !isOptionPanelOn && !isEndGameMenuPanelOn)
        {
            Timer();

            if (CGameManager.rewiredPlayer1.GetButtonDown("Select") && Input_on)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.select);

                switch (curIdx)
                {
                    case 0:
                        {
                            CGameManager.gameMode = GodFighter.GameMode.BattleRoom;
                            SceneManager.LoadScene("SelectMenu");
                        }
                        break;
                    case 1:
                        {
                            CGameManager.gameMode = GodFighter.GameMode.TrainingRoom;
                            SceneManager.LoadScene("SelectMenu");
                        }
                        break;
                    case 2:
                        {
                            Sequence ButtonAction = DOTween.Sequence();
                            ButtonAction.Insert(0, optionButtons.transform.DOMoveX(Screen.width * 2, 0.0f));
                            ButtonAction.Insert(0.1f, optionButtons.transform.DOMoveX(Screen.width / 2, 0.2f));

                            isOptionPanelOn = true;
                            optionBackground.SetActive(true);
                            controllButton.SetActive(true);
                            soundButton.SetActive(false);
                            isControllButton = true;
                            if (!CGameManager.isPcMode)
                            {
                                controllButton.SetActive(false);
                                soundButton.SetActive(true);
                                isControllButton = false;
                                controllOff.SetActive(true);
                            }
                        }
                        break;
                    case 3:
                        {
                            isEndGameMenuPanelOn = true;
                            EndGamePanel.SetActive(true);
                            EndGameYes.SetActive(true);
                            EndGameNo.SetActive(false);
                        }
                        break;
                    default:
                        Debug.LogError("Pause Menu Index Error!");
                        break;
                }
                Input_on = false;
                timer = 0.0f;
            }

            if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Vertical") < 0 && Input_on)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
                curIdx++;
                if (curIdx >= buttonImgs.Length) curIdx = 0;
                OnImage(curIdx);

                Input_on = false;
                timer = 0.0f;
            }
            else if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Vertical") > 0 && Input_on)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
                curIdx--;
                if (curIdx < 0) curIdx = buttonImgs.Length - 1;
                OnImage(curIdx);

                Input_on = false;
                timer = 0.0f;
            }
            else if (CGameManager.rewiredPlayer1.GetButtonDown("Back") && Input_on)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.back);
                SceneManager.LoadScene("StartGame");
            }
        }
        else
        {
            if (isOptionPanelOn) MenuOption();
            else if (isControllSetPanelOn) CheckControllSetPanelInput();
            else if (isSoundOptionPanelOn) CheckSoundOptionPanelInput();
            else if (isEndGameMenuPanelOn) CheckEndGameMenuPanelInput();
        }
    }

    private void OnImage(int idx)
    {
        for (int i = 0; i < buttonImgs.Length; ++i)
        {
            if (i == idx)
                buttonImgs[i].SetActive(true);
            else
                buttonImgs[i].SetActive(false);
        }
    }

    private void MenuOption()
    {
        Timer();

        if (!isControllSetPanelOn && !isSoundOptionPanelOn)
        {
            if (CGameManager.rewiredPlayer1.GetButtonDown("Back") && Input_on)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.back);
                Sequence ButtonAction = DOTween.Sequence();
                ButtonAction.Insert(0, optionButtons.transform.DOMoveX(Screen.width * -2, 0.0f));
                ButtonAction.Insert(0.1f, optionButtons.transform.DOMoveX(Screen.width / 2, 0.2f));

                isOptionPanelOn = false;
                optionBackground.SetActive(false);
                Input_on = false;
                timer = 0.0f;
            }
            else if (CGameManager.rewiredPlayer1.GetButtonDown("Select") && Input_on)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
                optionButtons.SetActive(false);
                isOptionPanelOn = false;
                Input_on = false;
                timer = 0.0f;
                if (isControllButton && CGameManager.isPcMode)
                {
                    Sequence ButtonAction = DOTween.Sequence();
                    ButtonAction.Insert(0, optionButtons.transform.DOMoveX(Screen.width / 2, 0.0f));
                    ButtonAction.Insert(0.1f, optionButtons.transform.DOMoveX(Screen.width * -2, 0.2f));

                    Sequence ControllAction = DOTween.Sequence();
                    ControllAction.Insert(0, controllSetPanel.transform.DOMoveX(Screen.width * 2, 0.0f));
                    ControllAction.Insert(0.1f, controllSetPanel.transform.DOMoveX(Screen.width / 2, 0.2f));

                    isControllSetPanelOn = true;
                    controllSetPanel.SetActive(true);
                    isControllPlayer1p = false;
                    player1.SetActive(!isControllPlayer1p);
                    player2.SetActive(isControllPlayer1p);
                }
                else if (!isControllButton)
                {
                    Sequence ButtonAction = DOTween.Sequence();
                    ButtonAction.Insert(0, optionButtons.transform.DOMoveX(Screen.width / 2, 0.0f));
                    ButtonAction.Insert(0.1f, optionButtons.transform.DOMoveX(Screen.width * -2, 0.2f));

                    Sequence SoundAction = DOTween.Sequence();
                    SoundAction.Insert(0, SoundOptionPanel.transform.DOMoveX(Screen.width * 2, 0.0f));
                    SoundAction.Insert(0.1f, SoundOptionPanel.transform.DOMoveX(Screen.width / 2, 0.2f));

                    isSoundOptionPanelOn = true;
                    SoundOptionPanel.SetActive(true);
                    bgmVol = CGameManager.bgmVolume;
                    Bgm_Slider.value = bgmVol;
                    sfxVol = CGameManager.soundFXVolume;
                    Sfx_Slider.value = sfxVol;
                    is_bgmOptionOn = true;
                }
            }
            else if ((CGameManager.rewiredPlayer1.GetAxisRaw("Move Vertical") > 0 || CGameManager.rewiredPlayer2.GetAxisRaw("Move Vertical") > 0) && Input_on && CGameManager.isPcMode)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
                if (!isControllButton)
                {
                    controllButton.SetActive(true);
                    soundButton.SetActive(false);
                    isControllButton = true;
                }

                Input_on = false;
                timer = 0.0f;
            }
            else if ((CGameManager.rewiredPlayer1.GetAxisRaw("Move Vertical") < 0 || CGameManager.rewiredPlayer2.GetAxisRaw("Move Vertical") < 0) && Input_on && CGameManager.isPcMode)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
                if (isControllButton)
                {
                    controllButton.SetActive(false);
                    soundButton.SetActive(true);
                    isControllButton = false;
                }

                Input_on = false;
                timer = 0.0f;
            }
        }
    }

    private void Timer()
    {
        if (!Input_on)
        {
            timer += Time.deltaTime;
            if (timer > waitingTime)
            {
                Input_on = true;
            }
        }
    }

    //컨트롤러

    public void CheckControllSetPanelInput()
    {
        Timer();

        if (CGameManager.rewiredPlayer1.GetButton("Back") && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.back);
            Sequence ControllAction = DOTween.Sequence();
            ControllAction.Insert(0, controllSetPanel.transform.DOMoveX(Screen.width / 2, 0.0f));
            ControllAction.Insert(0.1f, controllSetPanel.transform.DOMoveX(Screen.width * -2, 0.2f));

            Sequence ButtonAction = DOTween.Sequence();
            ButtonAction.Insert(0, optionButtons.transform.DOMoveX(Screen.width * 2, 0.0f));
            ButtonAction.Insert(0.1f, optionButtons.transform.DOMoveX(Screen.width / 2, 0.2f));

            CGameManager.rewiredPlayer1.controllers.maps.SetMapsEnabled(CGameManager.is_usedGamepadP1, Rewired.ControllerType.Joystick, 0);
            CGameManager.rewiredPlayer1.controllers.maps.SetMapsEnabled(!CGameManager.is_usedGamepadP1, Rewired.ControllerType.Keyboard, 0);
            CGameManager.rewiredPlayer2.controllers.maps.SetMapsEnabled(CGameManager.is_usedGamepadP2, Rewired.ControllerType.Joystick, 0);
            CGameManager.rewiredPlayer2.controllers.maps.SetMapsEnabled(!CGameManager.is_usedGamepadP2, Rewired.ControllerType.Keyboard, 0);

            optionButtons.SetActive(true);
            isOptionPanelOn = true;
            controllSetPanel.SetActive(false);
            isControllSetPanelOn = false;
            Input_on = false;
            timer = 0.0f;
        }
        else if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Vertical") > 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (isControllPlayer1p)
            {
                player1.SetActive(isControllPlayer1p);
                player2.SetActive(!isControllPlayer1p);
                isControllPlayer1p = false;
            }
            Input_on = false;
            timer = 0.0f;
        }
        else if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Vertical") < 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (!isControllPlayer1p)
            {
                player1.SetActive(isControllPlayer1p);
                player2.SetActive(!isControllPlayer1p);
                isControllPlayer1p = true;
            }
            Input_on = false;
            timer = 0.0f;
        }
        else if ((CGameManager.rewiredPlayer1.GetAxisRaw("Move Horizontal") < 0) && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (!isControllPlayer1p && CGameManager.is_usedGamepadP1)
            {
                CGameManager.is_usedGamepadP1 = false;
                controllSet1p_keyboard.transform.localPosition = new Vector3(110, controllSet1p_keyboard.transform.localPosition.y, controllSet1p_keyboard.transform.localPosition.z);

                controllSet1p_gamepad.transform.DOLocalMoveX(-110, 0.2f);
                controllSet1p_keyboard.transform.DOLocalMoveX(0, 0.2f);
            }
            else if (!isControllPlayer1p && !CGameManager.is_usedGamepadP1)
            {
                CGameManager.is_usedGamepadP1 = true;
                controllSet1p_gamepad.transform.localPosition = new Vector3(110, controllSet1p_gamepad.transform.localPosition.y, controllSet1p_gamepad.transform.localPosition.z);

                controllSet1p_keyboard.transform.DOLocalMoveX(-110, 0.2f);
                controllSet1p_gamepad.transform.DOLocalMoveX(0, 0.2f);
            }
            //2p
            else if (isControllPlayer1p && CGameManager.is_usedGamepadP2)
            {
                CGameManager.is_usedGamepadP2 = false;
                controllSet2p_keyboard.transform.localPosition = new Vector3(110, controllSet2p_keyboard.transform.localPosition.y, controllSet2p_keyboard.transform.localPosition.z);

                controllSet2p_gamepad.transform.DOLocalMoveX(-110, 0.2f);
                controllSet2p_keyboard.transform.DOLocalMoveX(0, 0.2f);
            }
            else if (isControllPlayer1p && !CGameManager.is_usedGamepadP2)
            {
                CGameManager.is_usedGamepadP2 = true;
                controllSet2p_gamepad.transform.localPosition = new Vector3(110, controllSet2p_gamepad.transform.localPosition.y, controllSet2p_gamepad.transform.localPosition.z);

                controllSet2p_keyboard.transform.DOLocalMoveX(-110, 0.2f);
                controllSet2p_gamepad.transform.DOLocalMoveX(0, 0.2f);
            }
            Input_on = false;
            timer = 0.0f;
        }
        else if ((CGameManager.rewiredPlayer1.GetAxisRaw("Move Horizontal") > 0) && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (!isControllPlayer1p && CGameManager.is_usedGamepadP1)
            {
                CGameManager.is_usedGamepadP1 = false;
                controllSet1p_keyboard.transform.localPosition = new Vector3(-110, controllSet1p_keyboard.transform.localPosition.y, controllSet1p_keyboard.transform.localPosition.z);

                controllSet1p_gamepad.transform.DOLocalMoveX(110, 0.2f);
                controllSet1p_keyboard.transform.DOLocalMoveX(0, 0.2f);
            }
            else if (!isControllPlayer1p && !CGameManager.is_usedGamepadP1)
            {
                CGameManager.is_usedGamepadP1 = true;
                controllSet1p_gamepad.transform.localPosition = new Vector3(-110, controllSet1p_gamepad.transform.localPosition.y, controllSet1p_gamepad.transform.localPosition.z);

                controllSet1p_keyboard.transform.DOLocalMoveX(110, 0.2f);
                controllSet1p_gamepad.transform.DOLocalMoveX(0, 0.2f);
            }
            //2p
            else if (isControllPlayer1p && CGameManager.is_usedGamepadP2)
            {
                CGameManager.is_usedGamepadP2 = false;
                controllSet2p_keyboard.transform.localPosition = new Vector3(-110, controllSet2p_keyboard.transform.localPosition.y, controllSet2p_keyboard.transform.localPosition.z);

                controllSet2p_gamepad.transform.DOLocalMoveX(110, 0.2f);
                controllSet2p_keyboard.transform.DOLocalMoveX(0, 0.2f);
            }
            else if (isControllPlayer1p && !CGameManager.is_usedGamepadP2)
            {
                CGameManager.is_usedGamepadP2 = true;
                controllSet2p_gamepad.transform.localPosition = new Vector3(-110, controllSet2p_gamepad.transform.localPosition.y, controllSet2p_gamepad.transform.localPosition.z);

                controllSet2p_keyboard.transform.DOLocalMoveX(110, 0.2f);
                controllSet2p_gamepad.transform.DOLocalMoveX(0, 0.2f);
            }
            Input_on = false;
            timer = 0.0f;
        }
    }

    //사운드
    public void CheckSoundOptionPanelInput()
    {
        Timer();

        if (CGameManager.rewiredPlayer1.GetButton("Back") && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.back);
            Sequence SoundAction = DOTween.Sequence();
            SoundAction.Insert(0, SoundOptionPanel.transform.DOMoveX(Screen.width / 2, 0.0f));
            SoundAction.Insert(0.1f, SoundOptionPanel.transform.DOMoveX(Screen.width * -2, 0.2f));

            Sequence ButtonAction = DOTween.Sequence();
            ButtonAction.Insert(0, optionButtons.transform.DOMoveX(Screen.width * 2, 0.0f));
            ButtonAction.Insert(0.1f, optionButtons.transform.DOMoveX(Screen.width / 2, 0.2f));

            optionButtons.SetActive(true);
            isOptionPanelOn = true;
            SoundOptionPanel.SetActive(false);
            isSoundOptionPanelOn = false;
            Input_on = false;
            timer = 0.0f;
        }
        //효과음배경음 버튼선택
        else if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Vertical") > 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (!is_bgmButtonOn)
            {
                is_bgmOptionOn = true;
                is_sfxOptionOn = false;
                is_bgmButtonOn = true;
            }

            bgmButton.SetActive(is_bgmButtonOn);
            sfxButton.SetActive(!is_bgmButtonOn);

            Input_on = false;
            timer = 0.0f;
        }
        else if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Vertical") < 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (is_bgmButtonOn)
            {
                is_bgmOptionOn = false;
                is_sfxOptionOn = true;
                is_bgmButtonOn = false;
            }

            bgmButton.SetActive(is_bgmButtonOn);
            sfxButton.SetActive(!is_bgmButtonOn);

            Input_on = false;
            timer = 0.0f;
        }
        //효과음 배경음 음량조절
        else if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Horizontal") > 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (is_bgmOptionOn)
            {
                bgmVol += 0.1f;
                if (bgmVol >= 1) bgmVol = 1;
                CGameManager.SetBgmVolume(bgmVol);
                Bgm_Slider.value = bgmVol;
            }
            else if (is_sfxOptionOn)
            {
                sfxVol += 0.1f;
                if (sfxVol >= 1) sfxVol = 1;
                CGameManager.SetSoundFXVolume(sfxVol);
                Sfx_Slider.value = sfxVol;
            }
            Input_on = false;
            timer = 0.0f;
        }
        else if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Horizontal") < 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (is_bgmOptionOn)
            {
                bgmVol -= 0.1f;
                if (bgmVol <= 0) bgmVol = 0;
                CGameManager.SetBgmVolume(bgmVol);
                Bgm_Slider.value = bgmVol;
            }
            else if (is_sfxOptionOn)
            {
                sfxVol -= 0.1f;
                if (sfxVol <= 0) sfxVol = 0;
                CGameManager.SetSoundFXVolume(sfxVol);
                Sfx_Slider.value = sfxVol;
            }
            Input_on = false;
            timer = 0.0f;
        }
    }
    public void CheckEndGameMenuPanelInput()
    {
        Timer();

        if (CGameManager.rewiredPlayer1.GetButtonDown("Select") && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
            if (isYesOrNo)
            {
                Application.Quit();
            }
            else if (!isYesOrNo)
            {
                EndGamePanel.SetActive(false);
                isEndGameMenuPanelOn = false;
                isYesOrNo = true;

                Input_on = false;
                timer = 0.0f;
            }
        }
        else if (CGameManager.rewiredPlayer1.GetButtonDown("Back") && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.back);
            EndGamePanel.SetActive(false);
            isEndGameMenuPanelOn = false;
            isYesOrNo = true;
            Input_on = false;
            timer = 0.0f;
        }
        else if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Vertical") < 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (isYesOrNo)
            {
                EndGameYes.SetActive(false);
                EndGameNo.SetActive(true);
                isYesOrNo = false;
            }
            else if (!isYesOrNo)
            {
                EndGameYes.SetActive(true);
                EndGameNo.SetActive(false);
                isYesOrNo = true;
            }
            Input_on = false;
            timer = 0.0f;
        }
        else if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Vertical") > 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (isYesOrNo)
            {
                EndGameYes.SetActive(false);
                EndGameNo.SetActive(true);
                isYesOrNo = false;
            }
            else if (!isYesOrNo)
            {
                EndGameYes.SetActive(true);
                EndGameNo.SetActive(false);
                isYesOrNo = true;
            }
            Input_on = false;
            timer = 0.0f;
        }
    }
}
