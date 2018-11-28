using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodFighter;
using UnityEngine.UI;

// FixedUpdateGOF 관리 
public class CSceneManager : MonoBehaviour
{
    public static bool freeCamera;
    public static bool freezePhysics;
    private static bool newRoundCasted;

    public CGameManager gameMgr;
    public CameraCtrl cameraControl;

    public ExecutionBufferType executionBufferType;
    public int executionBufferTime = 30;
    public int fps = 60;
    public float gravity = 0.42f;
    public float gameSpeed = 1;
    public int plinkingDelay = 2;

    public AnnouncerOptions announcerSounds;
    public ComboOptions comboOptions;
    public CharacterRotationOptions characterRotationOptions;
    public KnockdownOptions knockDownOptions;
    public HurtOptions hurtOptions;
    public BounceOptions groundBounceOptions;
    public HurtOptionDatas guardHitOptions;
    public TrainingRoomOptions trainingRoomOptions;
    public RoundOptions roundOptions;
    public CounterHitOptions counterHitOptions;

    public ButtonReference[] player1_Buttons = new ButtonReference[0];
    public ButtonReference[] player2_Buttons = new ButtonReference[0];

    public bool lockInputs { get; set; }
    public bool lockMovement { get; set; }
    public int currentRound { get; set; }

    private ButtonController p1ButtonController;
    private ButtonController p2ButtonController;

    private static CSceneManager _instance = null;

    public static CSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(CSceneManager)) as CSceneManager;

                if (_instance == null)
                {
                    Debug.LogError("There's no active CSceneManager object.");
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        cameraControl = Camera.main.gameObject.GetComponent<CameraCtrl>();
        if (cameraControl == null)
            Debug.LogError("카메라 스크립트를 못찾았습니다.");

        currentRound = 1;
        lockInputs = false;
        lockMovement = false;

        p1ButtonController = gameObject.AddComponent<ButtonController>();
        p1ButtonController.player_number = 1;

        p2ButtonController = gameObject.AddComponent<ButtonController>();
        p2ButtonController.player_number = 2;

        p1ButtonController.OnInitialize(player1_Buttons);
        p2ButtonController.OnInitialize(player2_Buttons);

        gameMgr.OnStart();
    }

    private void FixedUpdate()
    {
        if (gameMgr != null && gameMgr.P1_Left != null && gameMgr.P2_Right != null)
        {
            bool bothReady = (gameMgr.P1_Left.ready && gameMgr.P2_Right.ready);

            if (bothReady)
            {
                if (gameMgr.enableTimeUpdate) gameMgr.defaultUiCtrl.FixedUpdateGOF();
                if (gameMgr.defaultUiCtrl.isPaused) return;

                cameraControl.FixedUpdateGOF();
                if(gameMgr.defaultUiCtrl.OnGetCurrentPlayTime() <= 0 && gameMgr.defaultUiCtrl.OnGetRunning())
                {
                    float p1Life = gameMgr.P1_Left.myCharacterInfo.currentLifePoints / (float)gameMgr.P1_Left.myCharacterInfo.maxLifePoints;
                    float p2Life = gameMgr.P2_Right.myCharacterInfo.currentLifePoints / (float)gameMgr.P2_Right.myCharacterInfo.maxLifePoints;
                    lockMovement = true;
                    lockInputs = true;

                    gameMgr.defaultUiCtrl.OnSetRunning(false);
                    gameMgr.defaultUiCtrl.OnTimeOverView(true);
                    CGameManager.PlaySoundFX(CSceneManager.Instance.announcerSounds.timeOver);

                    if (p1Life > p2Life)
                    {
                        gameMgr.P2_Right.EndRound(3f);
                    }
                    else if (p1Life == p2Life)
                    {
                        gameMgr.P1_Left.NewRound(3f);
                    }
                    else
                    {
                        gameMgr.P1_Left.EndRound(3f);
                    }
                }

                if (gameMgr.P1_Left.moveSetScript != null && gameMgr.P1_Left.moveSetScript.MecanimControl != null)
                {
                    gameMgr.P1_Left.moveSetScript.MecanimControl.FixedUpdateGOF();
                    if (gameMgr.P1_Left.projectiles.Count > 0)
                    {
                        // item => item.IsDestroyed()는      delegate( item ) { item.IsDestroyed(); }
                        // 무명 함수의 첫번째 인자
                        gameMgr.P1_Left.projectiles.RemoveAll(item => item.IsDestroyed() || item == null);

                        foreach (ProjectileMoveScript projectileMoveScript in gameMgr.P1_Left.projectiles)
                        {
                            if (projectileMoveScript != null) projectileMoveScript.FixedUpdateGOF();
                        }
                    }
                    gameMgr.P1_Left.FixedUpdateGOF();
                }

                if (gameMgr.P2_Right.moveSetScript != null && gameMgr.P2_Right.moveSetScript.MecanimControl != null)
                {
                    gameMgr.P2_Right.moveSetScript.MecanimControl.FixedUpdateGOF();
                    // projectile update
                    if (gameMgr.P2_Right.projectiles.Count > 0)
                    {
                        gameMgr.P2_Right.projectiles.RemoveAll(item => item.IsDestroyed() || item == null);

                        foreach (ProjectileMoveScript projectileMoveScript in gameMgr.P2_Right.projectiles)
                        {
                            if (projectileMoveScript != null) projectileMoveScript.FixedUpdateGOF();
                        }
                    }
                    gameMgr.P2_Right.FixedUpdateGOF();
                }
            }
        }
    }

    public ButtonController GetButtonController(int pNum)
    {
        if (pNum == 1) return p1ButtonController;
        else if (pNum == 2) return p2ButtonController;
        else return null;
    }

    public void ResetRoundCast()
    {
        newRoundCasted = false;
    }

    public void CastNewRound()
    {
        if (newRoundCasted) return;

        if(gameMgr.P1_Left.introPlayed && gameMgr.P2_Right.introPlayed)
        {
            gameMgr.defaultUiCtrl.OnRoundView(currentRound);

            Invoke("StartFight", 2f);
            newRoundCasted = true;
        }
    }

    public void StartFight()
    {
        gameMgr.defaultUiCtrl.FightLogosInit();
        Invoke("StartTime", 1.5f);
    }

    public void StartTime()
    {
        lockInputs = false;
        lockMovement = false;
        gameMgr.defaultUiCtrl.AwakeTimeRunning();
    }

    public bool MyRandom(float enableValue)
    {
        int rand = Random.Range(0, 101);
        if (rand <= enableValue)
            return true;

        return false;
    }
}

[System.Serializable]
public class AnnouncerOptions
{
    public AudioClip round1;
    public AudioClip round2;
    public AudioClip round3;
    public AudioClip fight;
    public AudioClip player1Win;
    public AudioClip player2Win;
    public AudioClip perfect;
    public AudioClip timeOver;
    public AudioClip ko;
}

[System.Serializable]
public class ComboOptions
{
    public Sizes damageDeterioration;
    public AirJuggleDeteriorationType airJuggleDeteriorationType = AirJuggleDeteriorationType.AirHits;

    public float minHitStun = 1;
    public float minDamage = 5;
    public float minPushForce = 20;
    public int maxCombo = 99;
    public bool fixJuggleWeight = true;
    public float juggleWeight = 300;
    public float knockBackMinForce = 0;
    public bool resetFallForceOnHit = true;
    public bool neverCornerPush;
    public bool comboDisplay = true;
}

[System.Serializable]
public class CharacterRotationOptions
{
    public bool autoMirror = true;
    public bool rotateWhileJumping = false;
    public bool fixRotationWhenGuarding = true;
    public bool fixRotationOnHit = true;
    public float rotationSpeed = 10;
    public float mirrorBlending = .1f;
}

[System.Serializable]
public class KnockdownOptions
{
    public KnockdownOptionDatas air;
    public KnockdownOptionDatas wallbounce;
}

[System.Serializable]
public class KnockdownOptionDatas
{
    public float knockedOutTime = .5f;
    public float standUpTime = .3f; //.6f
    public bool isQuickStand;
    public ButtonPress[] quickStandButtons = new ButtonPress[0]; // 어떤 버튼을 입력하면 빠르게 일어나는지
    public float minQuickStandTime; // 입력조건 최소시간
    public bool isDelayedStand;
    public ButtonPress[] delayedStandButtons = new ButtonPress[0];
    public float maxDelayedStandTime;
}

[System.Serializable]
public class HurtOptionDatas
{
    public GameObject attackedParticle;
    public AudioClip attackedSound;
    public float freezingTime;
    public float animationSpeed = 0; 
    public bool isShakeCharacterOnHit = true;
    public bool isShakeCameraOnHit = true;
    public float shakeWeight = .8f;
}

[System.Serializable]
public class HurtOptions
{
    public bool resetAnimationOnHit = true;
    public HurtOptionDatas weekAttack;
    public HurtOptionDatas midAttack;
    public HurtOptionDatas strongAttack;

    public float strongStunnedMaxTime = 3;
}

[System.Serializable]
public class BounceOptions
{
    public Sizes bounceForce;
    public GameObject bounceEffectPrefab;
    public float bounceKillTime = 2;
    public float minimumBounceForce = 30;
    public float maximumBounceForce = 1;
    public float shakeWeight = .6f;
    public AudioClip bounceSound;
}

[System.Serializable]
public class StageMapOptions
{
    public string stageName;
    public GameObject prefab;
    public AudioClip bgm;
    public float groundFriction = 100;
    public float leftBoundary = -9.5f;
    public float rightBoundary = 12f;
}

[System.Serializable]
public class TrainingRoomOptions
{
    public bool isView = true;
    public float p1StartingLife = 1000f;
    public float p2StartingLife = 1000f;
    public float p1StartingGauge = 0f;
    public float p2StartingGauge = 0f;
    public float p1StartingGuardGauge = 0;
    public float p2StartingGuardGauge = 0;
    public LifeBarTrainingMode p1Life;
    public LifeBarTrainingMode p1Guage;
    public LifeBarTrainingMode p2Life;
    public LifeBarTrainingMode p2Gauge;
    public float refillTime = 2f;
}

[System.Serializable]
public class RoundOptions
{
    public int totalRound = 3;
    public bool hasTimer = true;
    public int timer = 99;
    public float endGameDelay = 4f;
    public float newRoundDelay = 1;
    public float slowMotionTimer = 2f;
    public float slowMotionSpeed = .2f;

    public AudioClip victoryMusic;
    public bool resetLifePoints = true;
    public bool resetPositions = true;
    public bool allowMovementStart = false;
    public bool allowMovementEnd = true;
    public bool inhibitGaugeGain = true; // 죽을 때 게이지 억제할 것인지
    public bool rotateBodyKO = true;
    public bool slowMotionKO = true;
    public bool cameraZoomKO = true;
    public bool freezeCamAfterOutro = true;
}

[System.Serializable]
public class CounterHitOptions
{
    public float damageIncrease = 10;
    public float hitStunIncrease = 50;
    public AudioClip counterHit;
}