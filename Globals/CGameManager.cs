using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodFighter;
using UnityEngine.UI;
using Rewired;

[System.Serializable]
public class FxSoundData
{
    public AudioClip select;
    public AudioClip move;
    public AudioClip pause;
    public AudioClip back;
    public AudioClip chrSelect;
}

public class CGameManager : MonoBehaviour {

    public static CharacterNames P1_name;
    public static CharacterNames P2_name;
    public static GameMode gameMode;

    public static Player rewiredPlayer1;
    public static Player rewiredPlayer2;

    public static StageMapOptions selectStateOption;
    public static AudioSource bgmAudioSource;
    public static AudioSource soundFXAudioSource;

    public static FxSoundData fxSounds;

    public static bool isBgm = true;
    public static float bgmVolume = 0.2f;
    public static bool isSoundFX = true;
    public static float soundFXVolume = 1f;

    public static bool is_usedGamepadP1 = true;
    public static bool is_usedGamepadP2 = true;

    public static bool isPcMode = true; //true -> PC모드, false -> 콘솔모드

    [HideInInspector] public CPlayerCtrl P1_Left;
    [HideInInspector] public CPlayerCtrl P2_Right;
    
    public GameObject       finalUiObj;
    public TrainingInputGUI trainingGUI;
    public DefaultUIControl defaultUiCtrl;
    public bool             enableTimeUpdate = true;

    private SwapTexture agniTextureScript;
    private SwapTexture mihoTextureScript;
    private SwapTexture valkiriTextureScript;

    [SerializeField] private string agni_obj_name;
    [SerializeField] private string miho_obj_name;
    [SerializeField] private string valkiri_obj_name;

    public void OnStart()
    {
        //// debugging /////////////////////////////////////////
        //CGameManager.rewiredPlayer1 = ReInput.players.GetPlayer(0);
        //CGameManager.rewiredPlayer1.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Joystick, 0);
        //CGameManager.rewiredPlayer1.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Keyboard, 0);

        //CGameManager.rewiredPlayer2 = ReInput.players.GetPlayer(1);
        //CGameManager.rewiredPlayer2.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Joystick, 0);
        //CGameManager.rewiredPlayer2.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Keyboard, 0);

        //gameMode = GameMode.TrainingRoom;

        //selectStateOption = new StageMapOptions();
        //selectStateOption.stageName = "MihoBaseMap";
        //selectStateOption.groundFriction = 100;
        //selectStateOption.leftBoundary = -10f;
        //selectStateOption.rightBoundary = 9.5f;

        //P1_name = CharacterNames.agni;
        //P2_name = CharacterNames.valkiri;
        ////////////////////////////////////////////////////////

        GameObject map = Instantiate(selectStateOption.prefab) as GameObject;
        GameObject ui = Instantiate(Resources.Load("UI/InGame")) as GameObject;
        ui.transform.parent = finalUiObj.transform;
        ui.transform.localPosition = Vector3.zero;
        ui.transform.localScale = Vector3.one;

        defaultUiCtrl = (DefaultUIControl)ui.GetComponent<InGameUIControl>();
        if (defaultUiCtrl == null) Debug.LogError("UI / InGameUIControl script is NOT FIND!");
        OnInitDefaultUi();

        trainingGUI = finalUiObj.GetComponent<TrainingInputGUI>();
        if (trainingGUI == null) Debug.LogError("UI / TrainingInputGUI script is not find!");

        //OnInitAudioSystem(true); // debugging
        OnInitAudioSystem(false);
        OnInitPlayersData();
    }

    public static void OnInitSoundFx(FxSoundData data)
    {
        fxSounds.chrSelect = data.chrSelect;
        fxSounds.move = data.move;
        fxSounds.pause = data.pause;
        fxSounds.select = data.select;
        fxSounds.back = data.back;
    }

    public void OnInitDefaultUi()
    {
        defaultUiCtrl.gameMgr = this;
        defaultUiCtrl.OnStart();
    }

    public static void OnInitAudioSystem(bool isMapBgm)
    {
        Camera cam = Camera.main;

        bgmAudioSource = cam.GetComponent<AudioSource>();
        if (bgmAudioSource == null)
            bgmAudioSource = cam.gameObject.AddComponent<AudioSource>();

        if(isMapBgm)
            bgmAudioSource.clip = selectStateOption.bgm;
        bgmAudioSource.loop = true;
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.volume = bgmVolume;

        soundFXAudioSource = cam.gameObject.AddComponent<AudioSource>();
        soundFXAudioSource.loop = false;
        soundFXAudioSource.playOnAwake = false;
        soundFXAudioSource.volume = 1f;
    }

    void OnInitPlayersData()
    {
        GameObject playerTemp = null;

        // 1. 선택에 따른 프리팹 생성 후 자식으로 대입
        switch (CGameManager.P1_name)
        {
            case CharacterNames.agni:
                {
                    playerTemp = Instantiate(Resources.Load("Characters/" + agni_obj_name),
                                                            Vector3.zero, Quaternion.identity) as GameObject;
                    agniTextureScript = null;
                    P1_Left.fighterName = CharacterNames.agni;
                    P1_Left.myCharacterInfo.physics.jumpForce = 16.5f;
                }
                break;
            case CharacterNames.miho:
                {
                    playerTemp = Instantiate(Resources.Load("Characters/" + miho_obj_name),
                                                            Vector3.zero, Quaternion.identity) as GameObject;
                    mihoTextureScript = null;
                    P1_Left.fighterName = CharacterNames.miho;
                }
                break;
            case CharacterNames.valkiri:
                {
                    playerTemp = Instantiate(Resources.Load("Characters/" + valkiri_obj_name),
                                                            Vector3.zero, Quaternion.identity) as GameObject;
                    valkiriTextureScript = null;
                    P1_Left.fighterName = CharacterNames.valkiri;
                }
                break;
        }
        playerTemp.transform.parent = P1_Left.gameObject.transform;
        playerTemp.transform.localPosition = Vector3.zero;
        playerTemp.transform.localRotation = Quaternion.identity;

        P1_Left.moveSetScript = playerTemp.GetComponent<MoveSetScript>();
        P1_Left.moveSetScript.playerScript = P1_Left;

        defaultUiCtrl.SetPlayerUiImages(1, P1_Left.fighterName);

        switch (CGameManager.P2_name)
        {
            case CharacterNames.agni:
                {
                    playerTemp = Instantiate(Resources.Load("Characters/" + agni_obj_name),
                                                            Vector3.zero, Quaternion.identity) as GameObject;
                    playerTemp.transform.parent = P2_Right.gameObject.transform;

                    agniTextureScript = P2_Right.transform.GetChild(0).GetComponent<SwapTexture>();

                    playerTemp.transform.localPosition = Vector3.zero;
                    playerTemp.transform.localRotation = Quaternion.identity;
                    playerTemp.transform.localScale = new Vector3(playerTemp.transform.localScale.x * -1, playerTemp.transform.localScale.y, playerTemp.transform.localScale.z);

                    P2_Right.moveSetScript = playerTemp.GetComponent<MoveSetScript>();
                    P2_Right.moveSetScript.playerScript = P2_Right;

                    P2_Right.fighterName = CharacterNames.agni;
                    P2_Right.myCharacterInfo.physics.jumpForce = 16.5f;
                    if (P1_name == P2_name && agniTextureScript != null)
                    {
                        agniTextureScript.OnSetOtherTexture();
                    }
                }
                break;
            case CharacterNames.miho:
                {
                    playerTemp = Instantiate(Resources.Load("Characters/" + miho_obj_name),
                                                            Vector3.zero, Quaternion.identity) as GameObject;
                    playerTemp.transform.parent = P2_Right.gameObject.transform;

                    mihoTextureScript = P2_Right.transform.GetChild(0).GetComponent<SwapTexture>();

                    playerTemp.transform.localPosition = Vector3.zero;
                    playerTemp.transform.localRotation = Quaternion.identity;
                    playerTemp.transform.localScale = new Vector3(playerTemp.transform.localScale.x * -1, playerTemp.transform.localScale.y, playerTemp.transform.localScale.z);

                    P2_Right.moveSetScript = playerTemp.GetComponent<MoveSetScript>();
                    P2_Right.moveSetScript.playerScript = P2_Right;

                    P2_Right.fighterName = CharacterNames.miho;
                    if (P1_name == P2_name && mihoTextureScript != null)
                    {
                        mihoTextureScript.OnSetOtherTexture();
                    }
                }
                break;
            case CharacterNames.valkiri:
                {
                    playerTemp = Instantiate(Resources.Load("Characters/" + valkiri_obj_name),
                                                            Vector3.zero, Quaternion.identity) as GameObject;
                    playerTemp.transform.parent = P2_Right.gameObject.transform;

                    valkiriTextureScript = P2_Right.transform.GetChild(0).GetComponent<SwapTexture>();

                    playerTemp.transform.localPosition = Vector3.zero;
                    playerTemp.transform.localRotation = Quaternion.identity;
                    playerTemp.transform.localScale = new Vector3(playerTemp.transform.localScale.x * -1, playerTemp.transform.localScale.y, playerTemp.transform.localScale.z);

                    P2_Right.moveSetScript = playerTemp.GetComponent<MoveSetScript>();
                    P2_Right.moveSetScript.playerScript = P2_Right;

                    P2_Right.fighterName = CharacterNames.valkiri;
                    if (P1_name == P2_name && valkiriTextureScript != null)
                    {
                        valkiriTextureScript.OnSetOtherTexture();
                    }
                }
                break;
        }
        defaultUiCtrl.SetPlayerUiImages(2, P2_Right.fighterName);

        P1_Left.player_num = 1;
        P2_Right.player_num = 2;

        P1_Left.OnStart();
        P2_Right.OnStart();
    }

    public void OnGameBegins()
    {
        P1_Left.roundsWin = 0;
        P2_Right.roundsWin = 0;

        SetBgmMusic(isBgm);
        SetBgmVolume(bgmVolume);
        SetSoundFX(isSoundFX);
        SetSoundFXVolume(soundFXVolume);
    }

    #region Sound FX
    public static void SetSoundFX(bool on)
    {
        isSoundFX = on;
    }

    public static void SetSoundFXVolume(float volume)
    {
        soundFXVolume = volume;
        if (soundFXAudioSource != null)
            soundFXAudioSource.volume = volume;
    }

    public static void PlaySoundFX(AudioClip soundFX)
    {
        PlaySoundFX(soundFX, soundFXVolume);
    }

    public static void PlaySoundFX(AudioClip soundFX, float volume)
    {
        if(isSoundFX && soundFX != null && soundFXAudioSource != null)
            soundFXAudioSource.PlayOneShot(soundFX, volume);
    }
    #endregion

    #region BGM
    public static void PlayBgmMusic()
    {
        if (isBgm && !bgmAudioSource.isPlaying && bgmAudioSource.clip != null)
            bgmAudioSource.Play();
    }

    public static void StopBgmMusic()
    {
        if (bgmAudioSource.clip != null)
            bgmAudioSource.Stop();
    }

    public static void SetBgmVolume(float volume = 1)
    {
        bgmVolume = volume;
        if (bgmAudioSource != null)
            bgmAudioSource.volume = volume;
    }

    public static void SetBgmMusic(bool on)
    {
        isBgm = on;

        if (on && !bgmAudioSource.isPlaying)         PlayBgmMusic();
        else if (!on && bgmAudioSource.isPlaying)    StopBgmMusic();
    }
    #endregion
}
