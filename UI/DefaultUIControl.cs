using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodFighter;

public enum uiPlayer
{
    p1,
    p2
}

public class DefaultUIControl : MonoBehaviour {

    [HideInInspector] public bool isPaused;
    public CGameManager gameMgr;

    protected static bool Input_on;
    protected static float timer = 0;
    protected static bool isRunning;

    protected static bool pausePlayer1;
    protected static bool pausePlayer2;
    protected static bool isOn;
    protected static uiPlayer player;

    protected bool isCommandPanelOn;
    protected bool isControllSetPanelOn;
    protected bool isKeyDisplayPanelOn;
    protected bool isSoundOptionPanelOn;
    protected bool isBackToMenuPanelOn;
    protected const float waitingTime = 0.4f; // 0.2f

    private static DefaultUIControl previousObj = null;
    
    public virtual void OnStart()
    {
        isRunning = false;
    }

    public virtual void FixedUpdateGOF()
    {
        if (isRunning)
        {
            if (!Input_on)
            {
                timer += Time.deltaTime;
                if (timer > waitingTime)
                {
                    Input_on = true;
                }
            }

            if (gameMgr.P1_Left.player.GetButton("Pause") && Input_on && !isOn)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.pause);
                if (!isCommandPanelOn && !isControllSetPanelOn && !isKeyDisplayPanelOn && !isSoundOptionPanelOn && !isBackToMenuPanelOn)
                {
                    pausePlayer1 = true;
                    player = uiPlayer.p1;
                    isOn = true;
                }
            }

            if (gameMgr.P2_Right.player.GetButton("Pause") && Input_on && !isOn)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.pause);
                if (!isCommandPanelOn && !isControllSetPanelOn && !isKeyDisplayPanelOn && !isSoundOptionPanelOn && !isBackToMenuPanelOn)
                {
                    pausePlayer2 = true;
                    player = uiPlayer.p2;
                    isOn = true;
                }
            }

            if ((pausePlayer1 || pausePlayer2)
                && isOn
                && !isCommandPanelOn && !isControllSetPanelOn && !isKeyDisplayPanelOn && !isSoundOptionPanelOn && !isBackToMenuPanelOn
                && Input_on
                && (!gameMgr.P1_Left.isDead && !gameMgr.P2_Right.isDead))
            {
                isOn = false;
                InputResetData();

                bool checkOn = player == uiPlayer.p1 ? true : false;
                if(pausePlayer1 && pausePlayer2)
                {
                    if (checkOn) pausePlayer1 = false;
                    else pausePlayer2 = false;
                }

                if (!checkOn && pausePlayer1) return;
                if (checkOn && pausePlayer2)  return;
                
                if (previousObj == null)
                {
                    GameObject pause = null;
                    if (CGameManager.gameMode == GameMode.TrainingRoom)
                    {
                        previousObj = gameMgr.defaultUiCtrl;
                        pause = Instantiate(Resources.Load("UI/TrainingPause")) as GameObject;

                        gameMgr.defaultUiCtrl = pause.GetComponent<TrainingPauseControl>();
                        if (gameMgr.defaultUiCtrl == null) Debug.LogError("UI / TrainingPauseControl script is NOT FIND!");

                        pause.GetComponent<TrainingPauseControl>().isPaused = true;
                    }
                    else if (CGameManager.gameMode == GameMode.BattleRoom)
                    {
                        previousObj = gameMgr.defaultUiCtrl;
                        pause = Instantiate(Resources.Load("UI/BattlePause")) as GameObject;

                        gameMgr.defaultUiCtrl = pause.GetComponent<BattlePauseControl>();
                        if (gameMgr.defaultUiCtrl == null) Debug.LogError("UI / BattlePauseControl script is NOT FIND!");

                        pause.GetComponent<BattlePauseControl>().isPaused = true;
                    }

                    pause.transform.parent = gameMgr.finalUiObj.transform;
                    pause.transform.localPosition = Vector3.zero;
                    pause.transform.localScale = Vector3.one;

                    gameMgr.OnInitDefaultUi();
                }
                else
                {
                    pausePlayer1 = pausePlayer2 = false;
                    DestroyImmediate(gameMgr.defaultUiCtrl.gameObject);
                    gameMgr.defaultUiCtrl = previousObj;
                    previousObj = null;
                }
            }
        }
    }

    protected void OnImage(int idx, ref GameObject[] objs)
    {
        for (int i = 0; i < objs.Length; ++i)
        {
            if (i == idx)
                objs[i].SetActive(true);
            else
                objs[i].SetActive(false);
        }
    }

    protected void InputResetData()
    {
        Input_on = false;
        timer = 0;
    } 

    public void OnSetRunning(bool value)
    {
        isRunning = value;
    }

    public bool OnGetRunning()
    {
        return isRunning;
    }

    #region all virtual methods
    // in game
    public virtual void OnRoundInit() { }
    public virtual void OnTimeOverView(bool on) { }
    public virtual void OnRoundView(int currentRound) { }
    public virtual void FightLogosInit() { }
    public virtual void AwakeTimeRunning() { }
    public virtual void SetPlayerUiImages(int player, CharacterNames name) { }
    public virtual void OnSetMaxHp(int player, float maxPoints) { }
    public virtual void StunGageCtrl(int player, float gauge) { }
    public virtual void OnStunImageReset(int playerNum) { }
    public virtual void SetHP(int playerNum, float curLife, bool reset) { }
    public virtual void CharStateOption(int type, int player_num) { }
    public virtual void OnComboView(int playerNum, int ComboNum) { }
    public virtual void OnMakeClone(CharacterNames name) { }
    public virtual void IncreaseGuardGaugeBar(int player, float gauge) { }
    public virtual void IncreaseSkillGaugeBar(int Player, float damage) { }
    public virtual void DecreaseSkillGaugeBar(int Player, GaugeSkillType skill) { }
    public virtual void DecreaseGuard(int player) { }
    public virtual void OnHalfSkillImageAnimation(CharacterNames fighterName, int pMirror) { }
    public virtual void KOimgSet(bool on) { }
    public virtual void OnPerfactKOView(bool on) { }
    public virtual void OnWinView(int playernum, CharacterNames fighterName, bool view) { }
    public virtual void OnVictoryView(int playerNum, int roundWinIdx) { }
    public virtual void OnSetTimeControlRoundEvent(bool value) { }
    public virtual bool OnGetTimeControlRoundEvent() { return false; }
    public virtual void ResetTimeDatas(float time) { }
    public virtual float GetTimePreTime() { return 0; }
    public virtual float OnGetCurrentPlayTime() { return 1; }
    public virtual bool GetTimeOverActive() { return false; }

    // pause
    public virtual void PauseMenuSelect() { }
    public virtual void CheckCommandPanelInput() { }
    public virtual void CheckControllSetPanelInput() { }
    public virtual void CheckKeyDisplayPanelInput() { }
    public virtual void CheckSoundOptionPanelInput() { }
    public virtual void CheckBackToMenuPanelInput() { }

    #endregion
}
