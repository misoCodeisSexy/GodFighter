using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GodFighter;
using UnityEngine.UI;
using DG.Tweening;

public class TrainingPauseControl : DefaultUIControl
{
    [SerializeField] private GameObject bgPause;
    [SerializeField] private GameObject[] buttonImgs;

    [SerializeField] private GameObject controllSetPanel;
    [SerializeField] private GameObject controllSet_keyboard;
    [SerializeField] private GameObject controllSet_gamepad;

    [SerializeField] private GameObject keyDisplayPanel;
    [SerializeField] private GameObject keyDisplayPanel_buttonOn;
    [SerializeField] private GameObject keyDisplayPanel_buttonOff;

    [SerializeField] private GameObject SoundOptionPanel;
    [SerializeField] private GameObject bgmButton;
    [SerializeField] private GameObject sfxButton;
    [SerializeField] private Slider Bgm_Slider;
    [SerializeField] private Slider Sfx_Slider;

    [SerializeField] private GameObject commandPanel;
    [SerializeField] private GameObject commandNameP1;
    [SerializeField] private GameObject commandNameP2;
    [SerializeField] private GameObject commandPanelP1;
    [SerializeField] private GameObject commandPanelP2;
    [SerializeField] private Sprite[] chrCommandInfos; // max : 3

    [SerializeField] private GameObject backToMenuPanel;
    [SerializeField] private GameObject backToMenuYes;
    [SerializeField] private GameObject backToMenuNo;

    [SerializeField] private GameObject controllOff1p;
    [SerializeField] private GameObject controllOff2p;

    private int curIdx;
    private bool is1PCommandView = true;

    private int setController_playernum = 0;

    private bool is_bgmButtonOn = true;
    private bool is_bgmOptionOn = false;
    private bool is_sfxOptionOn = false;
    private float bgmVol;
    private float sfxVol;

    private static bool is_viewKeyDisplay;

    private bool isMainMenu;
    private bool isYesOrNo = true;

    private bool enableAxisInput;
    private float axisTimer;
    private const float axisWaitingTime = 0.25f;

    private bool enableBtnInput;
    private float btnTimer;
    private const float btnWaitingTime = 0.13f;

    public override void OnStart()
    {
        curIdx = 0;
        ResetMyEnableTime();

        if (!CGameManager.isPcMode)
        {
            controllOff1p.SetActive(true);
            controllOff2p.SetActive(true);
        }

        switch (gameMgr.P1_Left.fighterName)
        {
            case CharacterNames.agni:
                {
                    commandPanelP1.GetComponent<Image>().sprite = chrCommandInfos[0];
                }
                break;
            case CharacterNames.miho:
                {
                    commandPanelP1.GetComponent<Image>().sprite = chrCommandInfos[1];
                }
                break;
            case CharacterNames.valkiri:
                {
                    commandPanelP1.GetComponent<Image>().sprite = chrCommandInfos[2];
                }
                break;
        }

        switch (gameMgr.P2_Right.fighterName)
        {
            case CharacterNames.agni:
                {
                    commandPanelP2.GetComponent<Image>().sprite = chrCommandInfos[0];
                }
                break;
            case CharacterNames.miho:
                {
                    commandPanelP2.GetComponent<Image>().sprite = chrCommandInfos[1];
                }
                break;
            case CharacterNames.valkiri:
                {
                    commandPanelP2.GetComponent<Image>().sprite = chrCommandInfos[2];
                }
                break;
        }
    }

    public override void FixedUpdateGOF()
    {
        base.FixedUpdateGOF();

        if (!enableAxisInput)
        {
            axisTimer += Time.fixedDeltaTime;
            if (axisTimer > axisWaitingTime) enableAxisInput = true;
        }

        if (!enableBtnInput)
        {
            btnTimer += Time.fixedDeltaTime;
            if (btnTimer > btnWaitingTime) enableBtnInput = true;
        }

        if (!pausePlayer1)
            controllOff1p.SetActive(true);
        else if (!pausePlayer2)
            controllOff2p.SetActive(true);

        if (!isCommandPanelOn 
            && !isControllSetPanelOn 
            && !isKeyDisplayPanelOn 
            && !isSoundOptionPanelOn 
            && !isBackToMenuPanelOn
            && enableAxisInput && (!gameMgr.P1_Left.isDead && !gameMgr.P2_Right.isDead))
        {
            if ((gameMgr.P1_Left.player.GetAxisRaw("Move Vertical") < 0 && pausePlayer1)
                || (gameMgr.P2_Right.player.GetAxisRaw("Move Vertical") < 0 && pausePlayer2))
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
                curIdx++;
                if (curIdx >= buttonImgs.Length) curIdx = 0;
                if (!CGameManager.isPcMode && (curIdx == 1 || curIdx == 2)) curIdx = 3;
                if (pausePlayer1 && curIdx == 2) curIdx = 3;
                else if (pausePlayer2 && curIdx == 1) curIdx = 2;
                OnImage(curIdx, ref buttonImgs);
                ResetMyEnableTime();
            }
            else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Vertical") > 0 && pausePlayer1)
                || (gameMgr.P2_Right.player.GetAxisRaw("Move Vertical") > 0 && pausePlayer2))
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
                curIdx--;
                if (curIdx < 0) curIdx = buttonImgs.Length - 1;
                if (!CGameManager.isPcMode && (curIdx == 1 || curIdx == 2)) curIdx = 0;
                if (pausePlayer1 && curIdx == 2) curIdx = 0;
                else if (pausePlayer2 && curIdx == 1) curIdx = 0;
                OnImage(curIdx, ref buttonImgs);
                ResetMyEnableTime();
            }
        }
        PauseMenuSelect();
    }

    public override void PauseMenuSelect()
    {
        if (!isCommandPanelOn && !isControllSetPanelOn && !isKeyDisplayPanelOn && !isSoundOptionPanelOn && !isBackToMenuPanelOn)
        {
            if ((gameMgr.P1_Left.player.GetButtonDown("Select")
                || gameMgr.P2_Right.player.GetButtonDown("Select")) && enableBtnInput)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
                switch (curIdx)
                {
                    case 0:     ///캐릭터 맵선택
                        {
                            bgPause.SetActive(true);
                            backToMenuPanel.SetActive(true);
                            isMainMenu = false;
                            backToMenuYes.SetActive(true);
                            backToMenuNo.SetActive(false);
                            isYesOrNo = true;

                            isBackToMenuPanelOn = true;
                            ResetMyEnableTime();
                        }
                        break;
                    case 3:     ///키 디스플레이
                        {
                            bgPause.SetActive(true);
                            keyDisplayPanel.SetActive(true);
                            isKeyDisplayPanelOn = true;

                            ResetKeyDisplayDatas();
                        }
                        break;
                    case 4:     ///커맨드리스트
                        {
                            bgPause.SetActive(true);
                            commandPanel.SetActive(true);
                            isCommandPanelOn = true;
                        }
                        break;
                    case 5:     ///사운드설정
                        {
                            bgPause.SetActive(true);

                            bgmVol = CGameManager.bgmVolume;
                            Bgm_Slider.value = bgmVol;
                            sfxVol = CGameManager.soundFXVolume;
                            Sfx_Slider.value = sfxVol;
                            is_bgmOptionOn = true;
                            SoundOptionPanel.SetActive(true);
                            isSoundOptionPanelOn = true;
                        }
                        break;
                    case 6:     ///메인메뉴
                        {
                            bgPause.SetActive(true);
                            backToMenuPanel.SetActive(true);
                            isMainMenu = true;
                            backToMenuYes.SetActive(true);
                            backToMenuNo.SetActive(false);
                            isYesOrNo = true;

                            isBackToMenuPanelOn = true;
                            ResetMyEnableTime();
                        }
                        break;
                }
                //1p 2p 조작설정
                if (gameMgr.P1_Left.player.GetButtonDown("Select") && Input_on)
                {
                    CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
                    if (curIdx == 1)     ///플레이어1 조작설정
                    {
                        bgPause.SetActive(true);
                        if (CGameManager.is_usedGamepadP1)
                        {
                            controllSet_gamepad.transform.localPosition = new Vector3(0, controllSet_gamepad.transform.localPosition.y, controllSet_gamepad.transform.localPosition.z);
                            controllSet_keyboard.transform.localPosition = new Vector3(110, controllSet_keyboard.transform.localPosition.y, controllSet_keyboard.transform.localPosition.z);
                        }
                        if (!CGameManager.is_usedGamepadP1)
                        {
                            controllSet_gamepad.transform.localPosition = new Vector3(110, controllSet_gamepad.transform.localPosition.y, controllSet_gamepad.transform.localPosition.z);
                            controllSet_keyboard.transform.localPosition = new Vector3(0, controllSet_keyboard.transform.localPosition.y, controllSet_keyboard.transform.localPosition.z);

                        }

                        controllSetPanel.SetActive(true);
                        isControllSetPanelOn = true;
                        setController_playernum = 1;
                        InputResetData();
                    }
                }
                if (gameMgr.P2_Right.player.GetButtonDown("Select") && enableBtnInput)
                {
                    CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
                    if (curIdx == 2)     ///플레이어1 조작설정
                    {
                        bgPause.SetActive(true);
                        if (CGameManager.is_usedGamepadP2)
                        {
                            controllSet_gamepad.transform.localPosition = new Vector3(0, controllSet_gamepad.transform.localPosition.y, controllSet_gamepad.transform.localPosition.z);
                            controllSet_keyboard.transform.localPosition = new Vector3(110, controllSet_keyboard.transform.localPosition.y, controllSet_keyboard.transform.localPosition.z);
                        }
                        if (!CGameManager.is_usedGamepadP2)
                        {
                            controllSet_gamepad.transform.localPosition = new Vector3(110, controllSet_gamepad.transform.localPosition.y, controllSet_gamepad.transform.localPosition.z);
                            controllSet_keyboard.transform.localPosition = new Vector3(0, controllSet_keyboard.transform.localPosition.y, controllSet_keyboard.transform.localPosition.z);

                        }

                        controllSetPanel.SetActive(true);
                        isControllSetPanelOn = true;
                        setController_playernum = 2;
                        InputResetData();
                    }
                }
                ResetMyEnableTime();
                return;
            }
        }

        else
        {
            if (isCommandPanelOn) CheckCommandPanelInput();
            else if (isKeyDisplayPanelOn) CheckKeyDisplayPanelInput();
            else if (isControllSetPanelOn) CheckControllSetPanelInput();
            else if (isSoundOptionPanelOn) CheckSoundOptionPanelInput();
            else if (isBackToMenuPanelOn) CheckBackToMenuPanelInput();
        }
    }

    //컨트롤러
    public override void CheckControllSetPanelInput()
    {
        if ((gameMgr.P1_Left.player.GetButtonDown("Select")
                || gameMgr.P2_Right.player.GetButtonDown("Select")) && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
            //1p 컨트롤러 활성화
            if (setController_playernum == 1)
            {
                gameMgr.P1_Left.player.controllers.maps.SetMapsEnabled(CGameManager.is_usedGamepadP1, Rewired.ControllerType.Joystick, 0);
                gameMgr.P1_Left.player.controllers.maps.SetMapsEnabled(!CGameManager.is_usedGamepadP1, Rewired.ControllerType.Keyboard, 0);
            }
            //2p 컨트롤러 활성화
            else if (setController_playernum == 2)
            {
                gameMgr.P2_Right.player.controllers.maps.SetMapsEnabled(CGameManager.is_usedGamepadP2, Rewired.ControllerType.Joystick, 0);
                gameMgr.P2_Right.player.controllers.maps.SetMapsEnabled(!CGameManager.is_usedGamepadP2, Rewired.ControllerType.Keyboard, 0);
            }

            bgPause.SetActive(false);
            controllSetPanel.SetActive(false);
            isControllSetPanelOn = false;
            setController_playernum = 0;
            InputResetData();

        }
        else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Horizontal") < 0
                || gameMgr.P2_Right.player.GetAxisRaw("Move Horizontal") < 0) && enableAxisInput)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            //1p 버튼
            if (setController_playernum == 1 && CGameManager.is_usedGamepadP1)
            {
                CGameManager.is_usedGamepadP1 = false;
                controllSet_keyboard.transform.localPosition = new Vector3(110, controllSet_keyboard.transform.localPosition.y, controllSet_keyboard.transform.localPosition.z);

                controllSet_gamepad.transform.DOLocalMoveX(-110, 0.2f);
                controllSet_keyboard.transform.DOLocalMoveX(0, 0.2f);
            }
            else if (setController_playernum == 1 && !CGameManager.is_usedGamepadP1)
            {
                CGameManager.is_usedGamepadP1 = true;
                controllSet_gamepad.transform.localPosition = new Vector3(110, controllSet_gamepad.transform.localPosition.y, controllSet_gamepad.transform.localPosition.z);

                controllSet_keyboard.transform.DOLocalMoveX(-110, 0.2f);
                controllSet_gamepad.transform.DOLocalMoveX(0, 0.2f);
            }
            if (setController_playernum == 2 && CGameManager.is_usedGamepadP2)
            {
                CGameManager.is_usedGamepadP2 = false;
                controllSet_keyboard.transform.localPosition = new Vector3(110, controllSet_keyboard.transform.localPosition.y, controllSet_keyboard.transform.localPosition.z);

                controllSet_gamepad.transform.DOLocalMoveX(-110, 0.2f);
                controllSet_keyboard.transform.DOLocalMoveX(0, 0.2f);
            }
            else if (setController_playernum == 2 && !CGameManager.is_usedGamepadP2)
            {
                CGameManager.is_usedGamepadP2 = true;
                controllSet_gamepad.transform.localPosition = new Vector3(110, controllSet_gamepad.transform.localPosition.y, controllSet_gamepad.transform.localPosition.z);

                controllSet_keyboard.transform.DOLocalMoveX(-110, 0.2f);
                controllSet_gamepad.transform.DOLocalMoveX(0, 0.2f);
            }

            ResetMyEnableTime();
        }
        else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Horizontal") > 0
                || gameMgr.P2_Right.player.GetAxisRaw("Move Horizontal") > 0) && enableAxisInput)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (setController_playernum == 1 && CGameManager.is_usedGamepadP1)
            {
                CGameManager.is_usedGamepadP1 = false;
                controllSet_keyboard.transform.localPosition = new Vector3(-110, controllSet_keyboard.transform.localPosition.y, controllSet_keyboard.transform.localPosition.z);

                controllSet_gamepad.transform.DOLocalMoveX(110, 0.2f);
                controllSet_keyboard.transform.DOLocalMoveX(0, 0.2f);
            }
            else if (setController_playernum == 1 && !CGameManager.is_usedGamepadP1)
            {
                CGameManager.is_usedGamepadP1 = true;
                controllSet_gamepad.transform.localPosition = new Vector3(-110, controllSet_gamepad.transform.localPosition.y, controllSet_gamepad.transform.localPosition.z);

                controllSet_keyboard.transform.DOLocalMoveX(110, 0.2f);
                controllSet_gamepad.transform.DOLocalMoveX(0, 0.2f);
            }
            if (setController_playernum == 2 && CGameManager.is_usedGamepadP2)
            {
                CGameManager.is_usedGamepadP2 = false;
                controllSet_keyboard.transform.localPosition = new Vector3(-110, controllSet_keyboard.transform.localPosition.y, controllSet_keyboard.transform.localPosition.z);

                controllSet_gamepad.transform.DOLocalMoveX(110, 0.2f);
                controllSet_keyboard.transform.DOLocalMoveX(0, 0.2f);
            }
            else if (setController_playernum == 2 && !CGameManager.is_usedGamepadP2)
            {
                CGameManager.is_usedGamepadP2 = true;
                controllSet_gamepad.transform.localPosition = new Vector3(-110, controllSet_gamepad.transform.localPosition.y, controllSet_gamepad.transform.localPosition.z);

                controllSet_keyboard.transform.DOLocalMoveX(110, 0.2f);
                controllSet_gamepad.transform.DOLocalMoveX(0, 0.2f);
            }

            ResetMyEnableTime();
        }
    }

    //키 디스플레이
    public override void CheckKeyDisplayPanelInput()
    {
        if ((gameMgr.P1_Left.player.GetButtonDown("Select")
                || gameMgr.P2_Right.player.GetButtonDown("Select")) && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
            bgPause.SetActive(false);
            keyDisplayPanel.SetActive(false);
            isKeyDisplayPanelOn = false;
            is_viewKeyDisplay = !is_viewKeyDisplay;
            CSceneManager.Instance.trainingRoomOptions.isView = !is_viewKeyDisplay;
            InputResetData();
        }
        else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Horizontal") > 0
                || gameMgr.P1_Left.player.GetAxisRaw("Move Horizontal") < 0
                || gameMgr.P2_Right.player.GetAxisRaw("Move Horizontal") > 0
                || gameMgr.P2_Right.player.GetAxisRaw("Move Horizontal") < 0)
                && enableAxisInput)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            ResetKeyDisplayDatas();
            ResetMyEnableTime();
        }
    }

    //사운드
    public override void CheckSoundOptionPanelInput()
    {
        if ((gameMgr.P1_Left.player.GetButtonDown("Select")
                || gameMgr.P2_Right.player.GetButtonDown("Select")) && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
            bgPause.SetActive(false);
            SoundOptionPanel.SetActive(false);
            isSoundOptionPanelOn = false;
            InputResetData();
        }
        //효과음배경음 버튼선택
        else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Vertical") > 0
                || gameMgr.P2_Right.player.GetAxisRaw("Move Vertical") > 0) && enableAxisInput)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (is_bgmButtonOn)
            {
                is_bgmOptionOn = false;
                is_sfxOptionOn = true;
                is_bgmButtonOn = false;
            }
            else
            {
                is_bgmOptionOn = true;
                is_sfxOptionOn = false;
                is_bgmButtonOn = true;
            }

            bgmButton.SetActive(is_bgmButtonOn);
            sfxButton.SetActive(!is_bgmButtonOn);
            ResetMyEnableTime();
        }
        else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Vertical") < 0
                || gameMgr.P2_Right.player.GetAxisRaw("Move Vertical") < 0) && enableAxisInput)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (is_bgmButtonOn)
            {
                is_bgmOptionOn = false;
                is_sfxOptionOn = true;
                is_bgmButtonOn = false;
            }
            else
            {
                is_bgmOptionOn = true;
                is_sfxOptionOn = false;
                is_bgmButtonOn = true;
            }

            bgmButton.SetActive(is_bgmButtonOn);
            sfxButton.SetActive(!is_bgmButtonOn);
            ResetMyEnableTime();
        }
        //효과음 배경음 음량조절
        else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Horizontal") > 0
                || gameMgr.P2_Right.player.GetAxisRaw("Move Horizontal") > 0) && enableAxisInput /* && (is_bgmOptionOn || is_sfxOptionOn)*/)
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
            ResetMyEnableTime();
        }
        else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Horizontal") < 0
                || gameMgr.P2_Right.player.GetAxisRaw("Move Horizontal") < 0) && enableAxisInput/* && (is_bgmOptionOn || is_sfxOptionOn)*/)
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
            ResetMyEnableTime();
        }
    }

    //커맨드
    public override void CheckCommandPanelInput()
    {
        if ((gameMgr.P1_Left.player.GetButtonDown("Select") || gameMgr.P2_Right.player.GetButtonDown("Select"))
            && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
            bgPause.SetActive(false);
            commandPanel.SetActive(false);
            isCommandPanelOn = false;
            InputResetData();
        }
        else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Horizontal") > 0
                        || gameMgr.P2_Right.player.GetAxisRaw("Move Horizontal") > 0) && enableAxisInput)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (is1PCommandView)
            {
                commandNameP2.transform.position = new Vector3(Screen.width * -2, commandNameP2.transform.position.y, commandNameP2.transform.position.z);

                Sequence PlayCommandPanelMov = DOTween.Sequence();
                PlayCommandPanelMov.Insert(0, commandNameP2.transform.DOMoveX(Screen.width / 2, 0.4f));
                PlayCommandPanelMov.Insert(0, commandNameP1.transform.DOMoveX(Screen.width * 2, 0.4f));

                is1PCommandView = false;
            }
            else// if (!is_1pCommandView)
            {
                commandNameP1.transform.position = new Vector3(Screen.width * -2, commandNameP1.transform.position.y, commandNameP1.transform.position.z);

                Sequence PlayCommandPanelMov = DOTween.Sequence();
                PlayCommandPanelMov.Insert(0, commandNameP1.transform.DOMoveX(Screen.width / 2, 0.4f));
                PlayCommandPanelMov.Insert(0, commandNameP2.transform.DOMoveX(Screen.width * 2, 0.4f));

                is1PCommandView = true;
            }
            ResetMyEnableTime();
        }
        else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Horizontal") < 0
                || gameMgr.P2_Right.player.GetAxisRaw("Move Horizontal") < 0) && enableAxisInput)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (is1PCommandView)
            {
                commandNameP2.transform.position = new Vector3(Screen.width * 2, commandNameP2.transform.position.y, commandNameP2.transform.position.z);

                Sequence PlayCommandPanelMov = DOTween.Sequence();
                PlayCommandPanelMov.Insert(0, commandNameP2.transform.DOMoveX(Screen.width / 2, 0.4f));
                PlayCommandPanelMov.Insert(0, commandNameP1.transform.DOMoveX(Screen.width * -2, 0.4f));

                is1PCommandView = false;
            }
            else// if (!is_1pCommandView)
            {
                commandNameP1.transform.position = new Vector3(Screen.width * 2, commandNameP1.transform.position.y, commandNameP1.transform.position.z);

                Sequence PlayCommandPanelMov = DOTween.Sequence();
                PlayCommandPanelMov.Insert(0, commandNameP1.transform.DOMoveX(Screen.width / 2, 0.4f));
                PlayCommandPanelMov.Insert(0, commandNameP2.transform.DOMoveX(Screen.width * -2, 0.4f));

                is1PCommandView = true;
            }
            ResetMyEnableTime();
        }
    }

    //메뉴 돌아가기
    public override void CheckBackToMenuPanelInput()
    {
        if ((gameMgr.P1_Left.player.GetButtonDown("Select")
                || gameMgr.P2_Right.player.GetButtonDown("Select")) && enableBtnInput)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
            //메인메뉴
            if (isMainMenu && isYesOrNo)
            {
                CSceneManager.Instance.ResetRoundCast();
                SceneManager.LoadScene("MainMenu");
            }
            //캐맵선택
            else if (!isMainMenu && isYesOrNo)
            {
                CSceneManager.Instance.ResetRoundCast();
                SceneManager.LoadScene("SelectMenu");
            }
            else if (!isYesOrNo)
            {
                bgPause.SetActive(false);
                backToMenuPanel.SetActive(false);
                isBackToMenuPanelOn = false;
                InputResetData();
            }
        }
        else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Vertical") < 0
                || gameMgr.P2_Right.player.GetAxisRaw("Move Vertical") < 0) && enableAxisInput)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (isYesOrNo)
            {
                backToMenuYes.SetActive(false);
                backToMenuNo.SetActive(true);
                isYesOrNo = false;
            }
            else if (!isYesOrNo)
            {
                backToMenuYes.SetActive(true);
                backToMenuNo.SetActive(false);
                isYesOrNo = true;
            }
            ResetMyEnableTime();
        }
        else if ((gameMgr.P1_Left.player.GetAxisRaw("Move Vertical") > 0
                || gameMgr.P2_Right.player.GetAxisRaw("Move Vertical") > 0) && enableAxisInput)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (isYesOrNo)
            {
                backToMenuYes.SetActive(false);
                backToMenuNo.SetActive(true);
                isYesOrNo = false;
            }
            else if (!isYesOrNo)
            {
                backToMenuYes.SetActive(true);
                backToMenuNo.SetActive(false);
                isYesOrNo = true;
            }
            ResetMyEnableTime();
        }
    }

    private void ResetMyEnableTime()
    {
        enableAxisInput = false;
        axisTimer = 0;

        enableBtnInput = false;
        btnTimer = 0;
    }
    private void ResetKeyDisplayDatas()
    {
        if (!is_viewKeyDisplay)
        {
            is_viewKeyDisplay = true;
            keyDisplayPanel_buttonOn.SetActive(true);
            keyDisplayPanel_buttonOff.SetActive(false);
        }
        else if (is_viewKeyDisplay)
        {
            is_viewKeyDisplay = false;
            keyDisplayPanel_buttonOn.SetActive(false);
            keyDisplayPanel_buttonOff.SetActive(true);
        }
    }
}
