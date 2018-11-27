using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DG.Tweening;
using UnityEngine.Events;
using GodFighter;
using Rewired;

public class CPlayerCtrl : MonoBehaviour {

    public string myHorizontal;
    public string myVertical;

    [SerializeField] protected CGameManager gameManager;

    public CharacterNames   fighterName;
    public int              player_num;  

    /// 적 파이터
    public CPlayerCtrl opPlayerScript;
    public LayerMask groundMask;

    public Animator animator;
    public float MaxFreezeTime = 0.2f; //.08f
    public int roundsWin = 0;

    public CameraCtrl cameraScript;
    public Player player;

    public BodyCollisionScript myBodyCollisionInfo;
    public BodyCollisionScript opBodyCollisionInfo;

    public HurtControlScript myHurtScript;
    public HitControlScript myHitScript;

    public MoveSetScript moveSetScript;
    public MoveInfo currentMove;
    public MoveInfo storedMove;

    public ColliderInfo currentHit;
    public string currentHitAnimation;

    public string currentAnimStateName = null;
    public string prevAttackingName = null;

    public int mirror;
    public float stunTime;
    public float curAnimSpeed;
    public float freezeTime = 0;
    public float reductionTime = 0;
    public float shakeWeight = 0;
    public float storedMoveTime;

    public PlayerState currentState;
    public PlayerSubState currentSubState;

    public bool isGuarding = false;
    public bool enableReattack = false;
    public bool ready = true;
    public bool isDead;
    public bool isFirstHit;
    public bool isAirRecovering;

    public bool potentialGuard;
    public bool guardStunned;
    public float strongStunnedTime;

    public bool shakeCamera;
    public bool shakeCharacter;
    public bool ignoreCollisionMass;

    public Dictionary<ButtonPress, float> inputHeldDown = new Dictionary<ButtonPress, float>();
    public List<ProjectileMoveScript> projectiles = new List<ProjectileMoveScript>();

    
    [HideInInspector] public CPhysicsScript myPhysicsScript;
    public CCharacterInfo myCharacterInfo;
    public CCharacterInfo opCharacterInfo;
    public float horizontalForce;

    public int comboHits { get; protected set; }
    public bool introPlayed { get; protected set; }

    private float standardYRotation;
    private float hitAnimationSpeed;
    private float hitStunDeceleration = 0;
    private bool outroPlayed;
    private int airJuggleHits;
    
    private float strongStunAfkTimer;
    private float strongStunnedTimer;
    private bool hitedSkill;
    private bool isGoingRefillLife;
    private bool isGoingRefillGauge;

    private float afkTimer;

    // miso MoveInfo debugger..
    //private GUIText P2Debugger;
    //private void Awake()
    //{
    //    /////////////////////////////////////////////////////////////////////////////////////////////////
    //    if (this.tag == "Player2")
    //    {
    //        GameObject P2DebuggerObj = new GameObject("Debugger2");
    //        P2Debugger = P2DebuggerObj.AddComponent<GUIText>();
    //        P2Debugger.pixelOffset = new Vector2(800 * ((float)Screen.width / 1280), 300f * ((float)Screen.height / 750));
    //        P2Debugger.text = "";
    //        P2Debugger.alignment = TextAlignment.Right;
    //        P2Debugger.anchor = TextAnchor.UpperRight;
    //        P2Debugger.color = Color.black;
    //        P2Debugger.richText = true;
    //        P2Debugger.enabled = true;
    //    }
    //    ////////////////////////////////////////////////////////////////////////////////////////////////
    //}

    public void OnStart()
    {
        myBodyCollisionInfo = transform.GetChild(0).Find("BodyCollider").GetComponent<BodyCollisionScript>();
        opBodyCollisionInfo = opPlayerScript.transform.GetChild(0).Find("BodyCollider").GetComponent<BodyCollisionScript>();

        myHurtScript = transform.GetChild(0).Find("HurtCollider").GetComponent<HurtControlScript>();
        myHitScript = transform.GetChild(0).Find("HitCollider").GetComponent<HitControlScript>();

        foreach (ButtonPress bp in System.Enum.GetValues(typeof(ButtonPress)))
        {
            inputHeldDown.Add(bp, 0);
        }
        
        animator = transform.GetChild(0).GetComponent<Animator>();
        currentState = PlayerState.stand;
        currentMove = null;

        if (this.tag == "Player1")
        {
            opCharacterInfo = opPlayerScript.gameObject.GetComponent<CCharacterInfo>();
            mirror = -1;

            if (myCharacterInfo == null)
                Debug.LogError("Player 1 character not found! Make sure you have set the characters correctly in the Global Editor");

            gameManager.defaultUiCtrl.OnSetMaxHp(1, myCharacterInfo.maxLifePoints);
            if (CGameManager.gameMode == GameMode.TrainingRoom)
            {
                myCharacterInfo.currentLifePoints = (float)myCharacterInfo.maxLifePoints * (CSceneManager.Instance.trainingRoomOptions.p1StartingLife / 100);
                myCharacterInfo.currentGaugePoints = (float)myCharacterInfo.maxGaugePoints * (CSceneManager.Instance.trainingRoomOptions.p1StartingGauge / 100);
                myCharacterInfo.currentGuardGaugePoints = (float)myCharacterInfo.maxGuardGaugePoints * (CSceneManager.Instance.trainingRoomOptions.p1StartingGuardGauge / 100);
            }
            else
            {
                myCharacterInfo.currentLifePoints = (float)myCharacterInfo.maxLifePoints;
            }

            player = CGameManager.rewiredPlayer1;
        }
        else if (this.tag == "Player2")
        {
            opCharacterInfo = opPlayerScript.gameObject.GetComponent<CCharacterInfo>();
            mirror = 1;

            if (myCharacterInfo == null)
                Debug.LogError("Player 2 character not found! Make sure you have set the characters correctly in the Global Editor");

            gameManager.defaultUiCtrl.OnSetMaxHp(2, myCharacterInfo.maxLifePoints);
            if (CGameManager.gameMode == GameMode.TrainingRoom)
            {
                myCharacterInfo.currentLifePoints = (float)myCharacterInfo.maxLifePoints * (CSceneManager.Instance.trainingRoomOptions.p2StartingLife / 100);
                myCharacterInfo.currentGaugePoints = (float)myCharacterInfo.maxGaugePoints * (CSceneManager.Instance.trainingRoomOptions.p2StartingGauge / 100);
                myCharacterInfo.currentGuardGaugePoints = (float)myCharacterInfo.maxGuardGaugePoints * (CSceneManager.Instance.trainingRoomOptions.p2StartingGuardGauge / 100);
            }
            else
            {
                myCharacterInfo.currentLifePoints = (float)myCharacterInfo.maxLifePoints;
            }

            player = CGameManager.rewiredPlayer2;
        }

        standardYRotation = transform.rotation.eulerAngles.y;

        if (CSceneManager.Instance.roundOptions.allowMovementStart)
            CSceneManager.Instance.lockMovement = false;
        else
            CSceneManager.Instance.lockMovement = true;

        if(player_num == 2)
        {
            gameManager.OnGameBegins();
        }
        ready = true;
    }

    public void FixedUpdateGOF()
    {
        #region traning mode 
        if ((player_num == 1 && CGameManager.gameMode == GameMode.TrainingRoom && CSceneManager.Instance.trainingRoomOptions.p1Life == LifeBarTrainingMode.refill)
           || (player_num == 2 && CGameManager.gameMode == GameMode.TrainingRoom && CSceneManager.Instance.trainingRoomOptions.p2Life == LifeBarTrainingMode.refill))
        {
            if (!isGoingRefillLife)
            {
                isGoingRefillLife = true;
                Invoke("RefillLife", CSceneManager.Instance.trainingRoomOptions.refillTime);
            }
        }
        if((player_num == 1 && CGameManager.gameMode == GameMode.TrainingRoom && CSceneManager.Instance.trainingRoomOptions.p1Guage == LifeBarTrainingMode.refill)
            || (player_num == 2 && CGameManager.gameMode == GameMode.TrainingRoom && CSceneManager.Instance.trainingRoomOptions.p2Gauge == LifeBarTrainingMode.refill))
        {
            if(!isGoingRefillGauge)
            {
                isGoingRefillGauge = true;
                Invoke("RefillGauge", CSceneManager.Instance.trainingRoomOptions.refillTime);
            }
        }
        #endregion

        // 2. moveinfo debugger
        //if (currentMove != null && player_num == 2)
        //{
        //    P2Debugger.text = "";
        //    P2Debugger.text += "-----Move Info-----" + "\n";
        //    P2Debugger.text += "Move: " + currentMove.moveName + "\n";
        //    P2Debugger.text += "Frames: " + currentMove.currentFrame + " / " + currentMove.totalFrames + "\n";
        //    P2Debugger.text += "Animation Speed: " + moveSetScript.GetAnimationSpeed() + "\n";
        //}

        resolveMove();

        ButtonController buttonController = CSceneManager.Instance.GetButtonController(player_num);
        CheckInputKey(buttonController);

        validateRotation();

        /// training mode input viewer
        if (CGameManager.gameMode == GameMode.TrainingRoom && CSceneManager.Instance.trainingRoomOptions.isView)
        {
            List<ButtonReference> buttonList = new List<ButtonReference>();
            foreach (ButtonReference buttonRef in buttonController.buttons)
            {
                if (inputHeldDown[buttonRef.buttonPressType] > 0 && inputHeldDown[buttonRef.buttonPressType] <= (2f / (float)CSceneManager.Instance.fps))
                {
                    buttonList.Add(buttonRef);
                }
            }
            gameManager.trainingGUI.OnVeiwingButtonIcon(buttonList.ToArray(), player_num);
        }

        /// Force Stand State
        if (!myPhysicsScript.freeze
           && !isDead
           && currentSubState != PlayerSubState.stunned
           && introPlayed
           && myPhysicsScript.IsGrounded()
           && !myPhysicsScript.IsMoving()
           && currentMove == null
           && !myPhysicsScript.isTakingOff
           && !myPhysicsScript.isLanding
           && !guardStunned
           && currentState != PlayerState.crouch
           && !isGuarding
           && strongStunnedTime <= 0
           && !moveSetScript.IsBasicMovePlaying(moveSetScript.basicMoves.idle)
           && !moveSetScript.IsAnimationPlaying("fallStraight"))
        {
            moveSetScript.PlayBasicMove(moveSetScript.basicMoves.idle);
            currentState = PlayerState.stand;
            currentSubState = PlayerSubState.resting;
            hitedSkill = false;
            myPhysicsScript.currentAirJumps = 0;
        }

        /// afk 
        if (moveSetScript.IsAnimationPlaying("idle")
           && !CSceneManager.Instance.lockInputs
           && !CSceneManager.Instance.lockMovement
           && !myPhysicsScript.freeze)
        {
            afkTimer += Time.fixedDeltaTime;
            if (afkTimer >= moveSetScript.basicMoves.idle.restingClipInterval)
            {
                afkTimer = 0;
                int ran = Random.Range(0, 5);
                if (ran >= 2 && moveSetScript.GetAnimationExist("idle2"))
                {
                    moveSetScript.PlayBasicMove(moveSetScript.basicMoves.idle, "idle2", false);
                }
            }
        }
        else
        {
            afkTimer = 0;
        }

        /// body check update
        myBodyCollisionInfo.FiexdUpdateGOF();
        /// hit bix check update
        myHitScript.FiexdUpdateGOF();

        /// stun points decrease
        if (myCharacterInfo.currentStrongStunPoints > 0
            && strongStunnedTime <= 0 // 스턴 걸렸을 때만 0 보다 크다
            && currentSubState != PlayerSubState.stunned)
        {
            strongStunAfkTimer += Time.fixedDeltaTime;
            if (strongStunAfkTimer >= 0.8f)
            {
                strongStunnedTimer += Time.fixedDeltaTime;
                if (strongStunnedTimer >= 0.3f)
                {
                    strongStunnedTimer = 0;
                    myCharacterInfo.currentStrongStunPoints -= 2;
                    gameManager.defaultUiCtrl.StunGageCtrl(player_num, myCharacterInfo.currentStrongStunPoints);
                }
            }
            else
            {
                strongStunnedTimer = 0;
            }
        }

        if (strongStunnedTime > 0)
        {
            strongStunnedTime -= Time.fixedDeltaTime;
            if (strongStunnedTime <= 0)
            {
                strongStunnedTime = 0;
                myCharacterInfo.currentStrongStunPoints = 0;
                gameManager.defaultUiCtrl.OnStunImageReset(player_num);
                myPhysicsScript.freeze = false;
                OnReleaseStun();
                myHitScript.ResetHit();
            }
        }

        if (shakeWeight > 0)
        {
            shakeWeight -= Time.fixedDeltaTime;

            if(myHitScript.isHit && myPhysicsScript.freeze)
            {
                if (shakeCamera) ShakeCam();
                if (shakeCharacter) ShakePlayer();
            }
            else
            {
                if (myPhysicsScript.isGroundBouncing)
                    ShakeCam();

            }
        }
        else if(shakeWeight < 0)
        {
            shakeWeight = 0;
            shakeCamera = false;
            shakeCharacter = false;
        }

        if (currentMove != null)
            CheckMove(currentMove);
       
        // apply stun
        if((currentSubState == PlayerSubState.stunned || guardStunned) && stunTime > 0 && !myPhysicsScript.freeze && !isDead)
            ApplyStun();

        // apply forces
        myPhysicsScript.ApplyForces(currentMove);

        if(moveSetScript.intro == null && !introPlayed)
        {
            introPlayed = true;
            CSceneManager.Instance.CastNewRound();
        }
        else if((player_num == 1 && !introPlayed && currentMove == null) ||
                (player_num == 2 && !introPlayed && opPlayerScript.introPlayed && currentMove == null))
        {
            KillCurrentMove();
            CastMove(moveSetScript.intro, true);
        }
    }

    private void CheckInputKey(ButtonController buttonController)
    {
        if (myPhysicsScript.freeze) return;
        if (CSceneManager.Instance.lockMovement) return;
        if (CSceneManager.Instance.lockInputs) return;
        if (strongStunnedTime > 0) return;
        if (hitedSkill) return;
       
        foreach(ButtonReference btnInput in buttonController.buttons)
        {
            #region
            if (((btnInput.buttonPressType == ButtonPress.Down && buttonController.GetAxisRaw(btnInput, player) >= 0)
                || (btnInput.buttonPressType == ButtonPress.Up && buttonController.GetAxisRaw(btnInput, player) <= 0))
                && myPhysicsScript.IsGrounded()
                && !myHitScript.isHit
                && currentSubState != PlayerSubState.stunned)
            {
                currentState = PlayerState.stand;
            }

            if(btnInput.inputType != InputType.Button
               && buttonController.GetAxisRaw(btnInput, player) == 0
               && inputHeldDown[btnInput.buttonPressType] > 0)
            {
                if(btnInput.buttonPressType == ButtonPress.Back)
                {
                    potentialGuard = false;
                }
                inputHeldDown[btnInput.buttonPressType] = 0;
            }

            if(inputHeldDown[btnInput.buttonPressType] == 0 && btnInput.inputType != InputType.Button)
            {
                btnInput.activeIconImg = buttonController.GetAxisRaw(btnInput, player) > 0 ? btnInput.inputViewerIcon1 : btnInput.inputViewerIcon2;
            }
            #endregion

            if (btnInput.inputType != InputType.Button && buttonController.GetAxisRaw(btnInput, player) != 0)
            {
                #region axis
                if (btnInput.inputType == InputType.HorizontalAxis)
                {
                    if(buttonController.GetAxisRaw(btnInput, player) > 0.2f)        // 0
                    {
                        if (mirror == 1)
                        {
                            inputHeldDown[ButtonPress.Forward] = 0;
                            btnInput.buttonPressType = ButtonPress.Back;

                            if (!myPhysicsScript.isTakingOff 
                                && moveSetScript.basicMoves.blockEnable
                                && myPhysicsScript.currentAirJumps <= 0)
                                potentialGuard = true;
                        }
                        else
                        {
                            inputHeldDown[ButtonPress.Back] = 0;
                            btnInput.buttonPressType = ButtonPress.Forward;
                        }

                        inputHeldDown[btnInput.buttonPressType] += Time.fixedDeltaTime;
                        if (inputHeldDown[btnInput.buttonPressType] == Time.fixedDeltaTime && HeldDownMoveExcution(btnInput.buttonPressType, false)) return;

                        if (currentState == PlayerState.stand
                        && !isGuarding
                        && !guardStunned
                        && myPhysicsScript.IsGrounded()
                        && currentMove == null
                        && currentSubState != PlayerSubState.stunned
                        && moveSetScript.basicMoves.moveEnable)
                        {
                            myPhysicsScript.Move(-mirror, buttonController.GetAxisRaw(btnInput, player));
                        }
                    }

                    if(buttonController.GetAxisRaw(btnInput, player) < -0.2f)
                    {
                        if (mirror == 1)    // right
                        {
                            inputHeldDown[ButtonPress.Back] = 0;
                            btnInput.buttonPressType = ButtonPress.Forward;
                        }
                        else
                        {
                            inputHeldDown[ButtonPress.Forward] = 0;
                            btnInput.buttonPressType = ButtonPress.Back;

                            if (!myPhysicsScript.isTakingOff 
                                && moveSetScript.basicMoves.blockEnable
                                && myPhysicsScript.currentAirJumps <= 0)
                                potentialGuard = true;
                        }

                        inputHeldDown[btnInput.buttonPressType] += Time.fixedDeltaTime;
                        if (inputHeldDown[btnInput.buttonPressType] == Time.fixedDeltaTime && HeldDownMoveExcution(btnInput.buttonPressType, false)) return;

                        if (currentState == PlayerState.stand
                        && !isGuarding
                        && !guardStunned
                        && myPhysicsScript.IsGrounded()
                        && currentMove == null
                        && currentSubState != PlayerSubState.stunned
                        && moveSetScript.basicMoves.moveEnable)
                        {
                            myPhysicsScript.Move(mirror, buttonController.GetAxisRaw(btnInput, player));
                        }
                    }
                }
                else //if (btnInput.inputType == InputType.VerticalAxis)
                {
                    if (buttonController.GetAxisRaw(btnInput, player) > 0.2f)
                    {
                        btnInput.buttonPressType = ButtonPress.Up;
                        if (!myPhysicsScript.isTakingOff && !myPhysicsScript.isLanding)
                        {
                            if (inputHeldDown[btnInput.buttonPressType] == 0)
                            {
                                //2단 점프
                                if (!myPhysicsScript.IsGrounded() && myCharacterInfo.physics.canJump && myCharacterInfo.physics.multiJumps > 1)
                                {
                                    myPhysicsScript.Jump();
                                }

                                if (HeldDownMoveExcution(btnInput.buttonPressType, false)) return;
                            }
                        }

                        if (!myPhysicsScript.freeze
                            && !myPhysicsScript.IsJumping()
                            && currentMove == null
                            && currentState == PlayerState.stand
                            && currentSubState != PlayerSubState.stunned
                            && !isGuarding
                            && !guardStunned
                            && myCharacterInfo.physics.canJump
                            && moveSetScript.basicMoves.jumpEnable)
                        {
                            float delayTime = (float)myCharacterInfo.physics.jumpDelay / CSceneManager.Instance.fps; //60; //fps값
                            myPhysicsScript.isTakingOff = true;
                            potentialGuard = false;

                            StartCoroutine(CallDelayJumpFunction(delayTime));

                            if (moveSetScript.GetAnimationExist(moveSetScript.basicMoves.takeOff.name))
                            {
                                moveSetScript.PlayBasicMove(moveSetScript.basicMoves.takeOff);
                                if (moveSetScript.basicMoves.takeOff.isAutoSpeed)
                                {
                                    moveSetScript.SetAnimationSpeed(moveSetScript.basicMoves.takeOff.name,
                                                                    moveSetScript.GetAnimationLength(moveSetScript.basicMoves.takeOff.name) / delayTime);
                                }
                            }
                        }
                        inputHeldDown[btnInput.buttonPressType] += Time.fixedDeltaTime;
                    }
                    else if(buttonController.GetAxisRaw(btnInput, player) < -0.2f)
                    {
                        btnInput.buttonPressType = ButtonPress.Down;
                        inputHeldDown[btnInput.buttonPressType] += Time.fixedDeltaTime;
                        if (inputHeldDown[btnInput.buttonPressType] == Time.fixedDeltaTime && HeldDownMoveExcution(btnInput.buttonPressType, false)) return;

                        if (!myPhysicsScript.freeze
                           && myPhysicsScript.IsGrounded()
                           && currentMove == null
                           && currentSubState != PlayerSubState.stunned
                           && !myPhysicsScript.isTakingOff
                           && !guardStunned
                           && moveSetScript.basicMoves.crouchEnable)
                        {
                            currentState = PlayerState.crouch;
                            if (!isGuarding)
                            {
                                if (moveSetScript.basicMoves.crouching.clip1 == null)
                                    Debug.LogError("character basic move 'crouching' animation clip no found! input clip data.");

                                moveSetScript.PlayBasicMove(moveSetScript.basicMoves.crouching, false);
                            }
                            else
                            {
                                if (moveSetScript.basicMoves.blockingCrouchingPose.clip1 == null)
                                    Debug.LogError("character basic move 'blockingCrouchingPose' animation clip no found! input clip data.");

                                moveSetScript.PlayBasicMove(moveSetScript.basicMoves.blockingCrouchingPose, false);
                            }
                        }
                    }
                }
                #endregion

                #region axis + button
                foreach (ButtonReference btnInput2 in buttonController.buttons)
                {
                    if (btnInput2.inputType == InputType.Button && buttonController.GetButtonDown(btnInput2, player))
                    {
                        MoveInfo tempMove = moveSetScript.GetMove(new ButtonPress[] { btnInput.buttonPressType, btnInput2.buttonPressType }
                                                                                     , new string[] { btnInput.inputButtonName, btnInput2.inputButtonName }
                                                                                     , 0, currentMove, currentState, player_num, false, false);
                        if (tempMove != null)
                        {
                            storedMove = tempMove;
                            storedMoveTime = ((float)CSceneManager.Instance.executionBufferTime / CSceneManager.Instance.fps);
                            return;
                        }
                    }
                }
                #endregion
            }

            #region button
            if (btnInput.inputType == InputType.Button && !CSceneManager.Instance.lockInputs)
            {
                if (buttonController.GetButton(btnInput, player))
                {
                    inputHeldDown[btnInput.buttonPressType] += Time.fixedDeltaTime;
                    // 버튼 2개 동시에 같이 눌렀을 경우
                    if (inputHeldDown[btnInput.buttonPressType] <= ((float)CSceneManager.Instance.plinkingDelay / (float)CSceneManager.Instance.fps))
                    {
                        foreach (ButtonReference btnInput2 in buttonController.buttons)
                        {
                            if (btnInput2 != btnInput
                                && btnInput2.inputType == InputType.Button
                                && buttonController.GetButtonDown(btnInput2, player))
                            {
                                inputHeldDown[btnInput2.buttonPressType] += Time.fixedDeltaTime;
                                MoveInfo tempMove = moveSetScript.GetMove(new ButtonPress[] { btnInput.buttonPressType, btnInput2.buttonPressType },
                                                                            new string[] { btnInput.inputButtonName, btnInput2.inputButtonName }, 0,
                                                                            currentMove, currentState, player_num, false, true);
                                if (tempMove != null)
                                {
                                    if (currentMove != null && currentMove.currentFrame <= CSceneManager.Instance.plinkingDelay) KillCurrentMove();
                                    storedMove = tempMove;
                                    storedMoveTime = ((float)CSceneManager.Instance.executionBufferTime / CSceneManager.Instance.fps);
                                    return;
                                }
                            }
                        }
                    }
                }

                if (buttonController.GetButtonDown(btnInput, player))
                {
                    MoveInfo tempMove = moveSetScript.GetMove(new ButtonPress[] { btnInput.buttonPressType },
                                                                 new string[] { btnInput.inputButtonName }, 0, currentMove, currentState, player_num, false, false);
                    if (tempMove != null)
                    {
                        storedMove = tempMove;
                        storedMoveTime = ((float)CSceneManager.Instance.executionBufferTime / CSceneManager.Instance.fps);
                        return;
                    }
                }

                if (buttonController.GetButtonUp(btnInput, player))
                {
                    inputHeldDown[btnInput.buttonPressType] = 0;
                }
            }
            #endregion
        }

    }

    public void ApplyStun()
    {
        // 공중에서 맞아서 스턴인 경우
        if(!myPhysicsScript.IsGrounded()
            && currentSubState == PlayerSubState.stunned
            && currentState != PlayerState.down)
        {
            stunTime = 1;
        }
        else
        {
            stunTime -= Time.fixedDeltaTime;
        }

        string standUpAnimation = null;
        float standUpTime = CSceneManager.Instance.knockDownOptions.air.standUpTime;
        if(!isDead && currentMove == null && myPhysicsScript.IsGrounded())
        {
            // 스턴 감속 및 애니메이션 속도 설정
            if (hitStunDeceleration > -(hitAnimationSpeed / 3) && currentMove == null)
            {
                hitAnimationSpeed -= Time.fixedDeltaTime;
                moveSetScript.SetAnimationSpeed(currentHitAnimation, hitAnimationSpeed + hitStunDeceleration);   
            }

            // 넉다운 시 애니메이션 설정
            if(currentState == PlayerState.down)
            {
                if (myCharacterInfo.currentStrongStunPoints >= myCharacterInfo.maxStrongStunPoints)
                {
                    OnStrongStunnedPlay(0.5f); return;
                }

                if (moveSetScript.basicMoves.standUp.clip1 == null)
                    Debug.LogError("character model's basicMoves in 'standUp' animation clip is NULL !!");

                if (stunTime <= CSceneManager.Instance.knockDownOptions.air.standUpTime) // .6f x / .3f o
                {
                    standUpAnimation = moveSetScript.GetAnimationString(moveSetScript.basicMoves.standUp, 1);
                    standUpTime = CSceneManager.Instance.knockDownOptions.air.standUpTime;
                }
            }
        }

        if(standUpAnimation != null && !moveSetScript.IsAnimationPlaying(standUpAnimation))
        {
            moveSetScript.PlayBasicMove(moveSetScript.basicMoves.standUp, standUpAnimation);
            if(moveSetScript.basicMoves.standUp.isAutoSpeed) moveSetScript.SetAnimationSpeed(standUpAnimation, moveSetScript.GetAnimationLength(standUpAnimation) / standUpTime);
            CGameManager.PlaySoundFX(moveSetScript.basicMoves.standUp.soundEffects);
        }

        if(stunTime <= 0)
        {
            OnReleaseStun();
        }
    }

    // 역전
    public void InvertRotation()
    {
        //이후에 죽거나 k.o 당했을때에는 rotation 값을 변경하지 않도록 코드추가
        standardYRotation = -standardYRotation;
        myBodyCollisionInfo.moveMirror *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    private void resolveMove()
    {
        if (myPhysicsScript.freeze) return;
        if (storedMoveTime > 0) storedMoveTime -= Time.fixedDeltaTime;
        if (storedMoveTime <= 0 && storedMove != null)
        {
            storedMoveTime = 0;
            if(CSceneManager.Instance.executionBufferType != ExecutionBufferType.NoBuffer)
                storedMove = null;
        }

        if((currentMove == null || currentMove.cancelable) && storedMove != null)
        {
            bool confirmQueue = false;

            if(CSceneManager.Instance.executionBufferType == ExecutionBufferType.AnyMove ||
                (currentMove == null && storedMoveTime >= ((float)(CSceneManager.Instance.executionBufferTime -2) / (float)CSceneManager.Instance.fps)))
            {
                confirmQueue = true;
            }

            if(confirmQueue)
            {
                KillCurrentMove();
                this.SetMove(storedMove);

                storedMove = null;
                storedMoveTime = 0;
            }
        }
    }

    private void testCharacterRotation(float rotationSpeed)
    {
        testCharacterRotation(rotationSpeed, false);
    }
    
    private void testCharacterRotation(float rotationSpeed, bool forceMirror)
    {
        if ((mirror == -1 || forceMirror) && transform.position.x > opPlayerScript.transform.position.x)
        {
            mirror = 1;
            InvertRotation();
        }
        else if ((mirror == 1 || forceMirror) && transform.position.x < opPlayerScript.transform.position.x)
        {
            mirror = -1;
            InvertRotation();
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(standardYRotation, Vector3.up), Time.fixedDeltaTime * rotationSpeed);
    }

    private void fixCharacterRotation()
    {
        if (currentState == PlayerState.down) return;
        if (transform.rotation != Quaternion.AngleAxis(standardYRotation, Vector3.up))
        {
            transform.rotation = Quaternion.AngleAxis(standardYRotation, Vector3.up);
        }
    }

    private void validateRotation()
    {
        Vector3 temp = transform.position;
        temp.z = 4.51f;
        transform.position = temp;

        if (!myPhysicsScript.IsGrounded() || (myPhysicsScript.freeze && strongStunnedTime <= 0))
            fixCharacterRotation();

        if (myPhysicsScript.freeze && strongStunnedTime <= 0) return;
        if (currentSubState == PlayerSubState.stunned && strongStunnedTime <= 0) return;
        if (currentState == PlayerState.down) return;
        if (myPhysicsScript.IsJumping()) return;

        testCharacterRotation(10);
    }

    public static void DelaySynchronizedAction(CPhysicsScript physics, float seconds = 0f)
    {
        if (Time.fixedDeltaTime > 0f)
        {
            DelaySynchronizedAction(physics, Mathf.FloorToInt(seconds * CSceneManager.Instance.fps)); //fps = 60
        }
        else
        {
            DelaySynchronizedAction(physics, 1);
        }
    }

    #region 코루틴
    IEnumerator CallDelayJumpFunction(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        myPhysicsScript.Jump();
    }

    IEnumerator CallDelayUnPauseFunction(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        HitUnpause();
    }

    IEnumerator CallDelayResetHit(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        myHitScript.ResetHit();
    }
    #endregion

    public void GetHit(ColliderInfo opHurtInfo, Vector3 location)
    {
        if (currentState == PlayerState.down) return;
        if (currentState == PlayerState.crouch && opHurtInfo.attackRange == AttackRange.top) return;
        if (strongStunnedTime > 0) return;

        bool airHit = false;
        bool strongStunned = false;
        bool armiored = false;
        float damageModifer = 1;
        float hitStunModifer = 1;
        BasicMoveInfo currentHitInfo = null;
        hitStunDeceleration = -9999;

        myHitScript.isHit = true;
        myHitScript.hitType = HitType.Strike;
        currentHit = opHurtInfo;

        // stun gauge
        strongStunned = AddStrongStunnd(opHurtInfo.opGainStrongStun);
        strongStunAfkTimer = 0;

        // opponent attack give heal
        if(opHurtInfo.healData.enableHealing)
        {
            opPlayerScript.myCharacterInfo.currentLifePoints += opHurtInfo.healData.healValue;
            if (opPlayerScript.myCharacterInfo.currentLifePoints >= opPlayerScript.myCharacterInfo.maxLifePoints)
                opPlayerScript.myCharacterInfo.currentLifePoints = opPlayerScript.myCharacterInfo.maxLifePoints;
            gameManager.defaultUiCtrl.SetHP(opPlayerScript.player_num, opPlayerScript.myCharacterInfo.currentLifePoints, false);
        }

        #region miso
        //// 현재 애니메이션에서 카운터 무빙이 있는 경우
        //// ex. 계속 맞는 경우 currentMove가 hit 종류 애니메이션일 때 counter 링크 무빙이 있는 경우
        //if(currentMove != null && currentMove.frameLinks.Length > 0)
        //{
        //    foreach(MoveLink frameLink in currentMove.frameLinks)
        //    {
        //        if(currentMove.currentFrame >= frameLink.activeFramesBegins &&
        //           currentMove.currentFrame <= frameLink.activeFramesEnds)
        //        {
        //            if(frameLink.linkType == LinkType.CounterMove)
        //            {

        //            }
        //        }
        //    }
        //}
        #endregion

        if (myPhysicsScript.IsGrounded())
        {
            if (currentState == PlayerState.neutralJump
                || currentState == PlayerState.forwardJump
                ||currentState == PlayerState.backJump) currentState = PlayerState.stand;

            if (strongStunned)
            {
                OnStrongStunnedPlay(); return;
            }
            else
            if (currentState == PlayerState.stand)
            {
                if(opHurtInfo.hurtType == HurtType.launcher)
                {
                    if (moveSetScript.basicMoves.getHitStandingMid.clip1 == null)
                        Debug.LogError("character basic move 'getHitStandingMid' animation clip no found! input clip data.");

                    currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.getHitStandingMid, opHurtInfo.attackLevel);
                    currentHitInfo = moveSetScript.basicMoves.getHitStandingMid;

                    ProjectileMoveScript projectileSctipt = opHurtInfo.gameObject.GetComponent<ProjectileMoveScript>();
                    if (projectileSctipt == null) Debug.LogError("launcher object is NOT found 'ProjectileMoveScript'..");
                    projectileSctipt.DestroyMe();
                }
                else if(opHurtInfo.hurtType == HurtType.pureNormalAttack 
                    || opHurtInfo.hurtType == HurtType.skill)
                {
                    if (opHurtInfo.attackRange == AttackRange.none)
                    {
                        Debug.LogError("공격에 해당되는 공격 범위를 설정하세요. (상/중/하)"); return;
                    }

                    if(opHurtInfo.hurtType == HurtType.skill) hitedSkill = true;

                    if (opHurtInfo.attackRange == AttackRange.top)
                    {
                        if (moveSetScript.basicMoves.getHitStandingHigh.clip1 == null)
                            Debug.LogError("character basic move 'getHitStandingHigh' animation clip no found! input clip data.");

                        currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.getHitStandingHigh, opHurtInfo.attackLevel);
                        currentHitInfo = moveSetScript.basicMoves.getHitStandingHigh;
                    }
                    else if (opHurtInfo.attackRange == AttackRange.mid)
                    {
                        if (moveSetScript.basicMoves.getHitStandingMid.clip1 == null)
                            Debug.LogError("character basic move 'getHitStandingMid' animation clip no found! input clip data.");

                        currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.getHitStandingMid, opHurtInfo.attackLevel);
                        currentHitInfo = moveSetScript.basicMoves.getHitStandingMid;
                    }
                    else if (opHurtInfo.attackRange == AttackRange.low)
                    {
                        if (moveSetScript.basicMoves.getHitStandingLow.clip1 == null)
                            Debug.LogError("character basic move 'getHitStandingLow' animation clip no found! input clip data.");

                        currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.getHitStandingLow, opHurtInfo.attackLevel);
                        currentHitInfo = moveSetScript.basicMoves.getHitStandingLow;
                    }
                }
                else if(opHurtInfo.hurtType == HurtType.pureNormalAttackAir)
                {
                    if (moveSetScript.basicMoves.getHitStandingHigh.clip1 == null)
                        Debug.LogError("character basic move 'getHitStandingHigh' animation clip no found! input clip data.");

                    currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.getHitStandingHigh, opHurtInfo.attackLevel);
                    currentHitInfo = moveSetScript.basicMoves.getHitStandingHigh;
                }
                else if(opHurtInfo.hurtType == HurtType.none)
                {
                    Debug.LogError(" opponent collider 'Hurt Type' no set. enter data. (해당되는 hurt type이 없어서 currentHitInfo 값이 null 입니다. 공격 타입을 설정하세요.)");
                }
            }
            else if(currentState == PlayerState.crouch)
            {
                if (opHurtInfo.attackRange == AttackRange.none)
                {
                    Debug.LogError("공격에 해당되는 공격 범위를 설정하세요. (상/중/하)"); return;
                }

                if (opHurtInfo.hurtType == HurtType.launcher)
                {
                    if (moveSetScript.basicMoves.getHitCrouchingMid.clip1 == null)
                        Debug.LogError("character basic move 'getHitCrouchingMid' animation clip no found! input clip data.");

                    currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.getHitCrouchingMid, opHurtInfo.attackLevel);
                    currentHitInfo = moveSetScript.basicMoves.getHitCrouchingMid;

                    ProjectileMoveScript projectileSctipt = opHurtInfo.gameObject.GetComponent<ProjectileMoveScript>();
                    if (projectileSctipt == null) Debug.LogError("launcher object is NOT found 'ProjectileMoveScript'..");
                    projectileSctipt.DestroyMe();
                }
                else if(opHurtInfo.hurtType == HurtType.pureNormalAttack 
                    || opHurtInfo.hurtType == HurtType.skill)
                {
                    if (opHurtInfo.attackRange == AttackRange.none)
                    {
                        Debug.LogError("공격에 해당되는 공격 범위를 설정하세요. (중/하)");
                        return;
                    }
                    else if (opHurtInfo.attackRange == AttackRange.mid)
                    {
                        if (moveSetScript.basicMoves.getHitCrouchingMid.clip1 == null)
                            Debug.LogError("character basic move 'getHitCrouchingMid' animation clip no found! input clip data.");

                        currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.getHitCrouchingMid, opHurtInfo.attackLevel);
                        currentHitInfo = moveSetScript.basicMoves.getHitCrouchingMid;
                    }
                    else if (opHurtInfo.attackRange == AttackRange.low)
                    {
                        if (moveSetScript.basicMoves.getHitCrouchingLow.clip1 == null)
                            Debug.LogError("character basic move 'getHitCrouchingLow' animation clip no found! input clip data.");

                        currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.getHitCrouchingLow, opHurtInfo.attackLevel);
                        currentHitInfo = moveSetScript.basicMoves.getHitCrouchingLow;
                    }
                    else if (opHurtInfo.hurtType == HurtType.pureNormalAttackAir)
                    {
                        if (moveSetScript.basicMoves.getHitStandingHigh.clip1 == null)
                            Debug.LogError("character basic move 'getHitStandingHigh' animation clip no found! input clip data.");

                        currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.getHitStandingHigh, opHurtInfo.attackLevel);
                        currentHitInfo = moveSetScript.basicMoves.getHitStandingHigh;
                    }
                    else if (opHurtInfo.hurtType == HurtType.none)
                    {
                        Debug.LogError(" opponent collider 'Hurt Type' no set. enter data. (해당되는 hurt type이 없어서 currentHitInfo 값이 null 입니다. 공격 타입을 설정하세요.)");
                    }
                }
            }
        }
        else // air hit
        {
            if (opHurtInfo.attackLevel == AttackLevel.week)
            {
                CSceneManager.Instance.groundBounceOptions.bounceForce = Sizes.small;
                CSceneManager.Instance.groundBounceOptions.maximumBounceForce = 1;
            }
            else if (opHurtInfo.attackLevel == AttackLevel.mid)
            {
                CSceneManager.Instance.groundBounceOptions.bounceForce = Sizes.medium;
                CSceneManager.Instance.groundBounceOptions.maximumBounceForce = 2;
            }
            else if (opHurtInfo.attackLevel == AttackLevel.strong)
                CSceneManager.Instance.groundBounceOptions.bounceForce = Sizes.high;

            if (moveSetScript.basicMoves.getHitAir.clip1 == null)
                Debug.LogError("character basic move 'getHitAir' animation clip no found! input clip data.");

            currentHitAnimation = moveSetScript.basicMoves.getHitAir.name;
            currentHitInfo = moveSetScript.basicMoves.getHitAir;

            gameManager.defaultUiCtrl.CharStateOption(1, player_num);
            airHit = true;
        }

        // hit animation override
        if(opHurtInfo.overrideNormalHitAnimation) // normal command
        {
            if (moveSetScript.basicMoves.getHitStandingHighSkill1.clip1 != null)
            {
                currentHitInfo = moveSetScript.basicMoves.getHitStandingHighSkill1;
                currentHitAnimation = currentHitInfo.name;
            }
        }
        else if(opHurtInfo.overrideUltimateHitAnimation) // ultimate
        {
            switch (opPlayerScript.fighterName)
            {
                case CharacterNames.agni:
                    {
                        if (moveSetScript.ultimateHitMovesAgni.Length > 0
                           && moveSetScript.ultimateHitMovesAgni[opHurtInfo.ultimateHitAnimationIdx].clip1 != null)
                        {
                            currentHitInfo = moveSetScript.ultimateHitMovesAgni[opHurtInfo.ultimateHitAnimationIdx];
                            currentHitAnimation = currentHitInfo.name;
                        }
                    }
                    break;
                case CharacterNames.miho:
                    {
                        if (moveSetScript.ultimateHitMovesMiho.Length > 0 
                            && moveSetScript.ultimateHitMovesMiho[opHurtInfo.ultimateHitAnimationIdx].clip1 != null) 
                        {
                            currentHitInfo = moveSetScript.ultimateHitMovesMiho[opHurtInfo.ultimateHitAnimationIdx];
                            currentHitAnimation = currentHitInfo.name;
                        }
                    }
                    break;
                case CharacterNames.valkiri:
                    {

                    }
                    break;
            }
        }

        HurtOptionDatas _hurtEffect = opHurtInfo.hurtEffects;
        if(!opHurtInfo.overrideHitEffects)
        {
            switch(opHurtInfo.attackLevel)
            {
                case AttackLevel.week:
                        _hurtEffect = CSceneManager.Instance.hurtOptions.weekAttack;
                    break;
                case AttackLevel.mid:
                        _hurtEffect = CSceneManager.Instance.hurtOptions.midAttack;
                    break;
                case AttackLevel.strong:
                        _hurtEffect = CSceneManager.Instance.hurtOptions.strongAttack;
                    break;
            }
        }
        _hurtEffect.attackedSound = opHurtInfo.hurtEffects.attackedSound;

        // conter
        if (currentMove != null 
            && !currentMove.hitAnimationOverride 
            && currentMove.counterFrameData == CounterFrameData.Attack)
        {
            gameManager.defaultUiCtrl.CharStateOption(2, player_num);
            damageModifer += CSceneManager.Instance.counterHitOptions.damageIncrease * 0.01f;
            hitStunModifer += CSceneManager.Instance.counterHitOptions.hitStunIncrease * 0.01f;
            _hurtEffect.attackedSound = CSceneManager.Instance.counterHitOptions.counterHit;
            storedMove = null;
            KillCurrentMove();
        }

        // create hit effect
        if(_hurtEffect != null)
        {
            GameObject hutParticleTemp = (GameObject)Instantiate(opHurtInfo.hurtEffects.attackedParticle, location, Quaternion.identity);
        }

        // shakes
        shakeCamera = _hurtEffect.isShakeCameraOnHit;
        shakeCharacter = _hurtEffect.isShakeCharacterOnHit;
        shakeWeight = _hurtEffect.shakeWeight;

        if(!isFirstHit && !opPlayerScript.isFirstHit)
        {
            opPlayerScript.isFirstHit = true;
            gameManager.defaultUiCtrl.CharStateOption(4, player_num);
        }

        float damage = 0;
        if(!opHurtInfo.damageScaling && CSceneManager.Instance.comboOptions.damageDeterioration == Sizes.none)
        {
            damage = opHurtInfo.damage;
        }
        else if(CSceneManager.Instance.comboOptions.damageDeterioration == Sizes.small)
        {
            damage = opHurtInfo.damage - (opHurtInfo.damage * comboHits * 0.1f);
        }
        else if(CSceneManager.Instance.comboOptions.damageDeterioration == Sizes.medium)
        {
            damage = opHurtInfo.damage - (opHurtInfo.damage * comboHits * 0.2f);
        }

        if (damage < CSceneManager.Instance.comboOptions.minDamage)
            damage = CSceneManager.Instance.comboOptions.minDamage;

        damage *= damageModifer;
        comboHits++;

        AddNormalGauge(opHurtInfo.opGainGaugeOnHit);
        opPlayerScript.AddNormalGauge(opHurtInfo.gainGaugeOnHit);

        if(comboHits > 1 && CSceneManager.Instance.comboOptions.comboDisplay)
        {
            gameManager.defaultUiCtrl.OnComboView(opPlayerScript.player_num, comboHits);
        }

        isDead = DamageMe(damage, opHurtInfo.doesntKill);

        // play sound
        CGameManager.PlaySoundFX(_hurtEffect.attackedSound);
        if (isDead) CGameManager.PlaySoundFX(moveSetScript.deathSound);

        float spaceBetweenHits = 1;

        if(opHurtInfo.attackLevel == AttackLevel.week)
        {
            spaceBetweenHits = 1.1f;
        }
        else if(opHurtInfo.attackLevel == AttackLevel.mid)
        {
            spaceBetweenHits = 1.3f;
        }
        else if(opHurtInfo.attackLevel == AttackLevel.strong)
        {
            spaceBetweenHits = 1.7f;
        }
        StartCoroutine(CallDelayResetHit(_hurtEffect.freezingTime * spaceBetweenHits));

        // stun
        int stunFrames = 0;
        if((currentMove == null || !currentMove.hitAnimationOverride) && (!armiored || isDead))
        {
            currentSubState = PlayerSubState.stunned;

            // only frame
            stunFrames = opHurtInfo.hitStunOnHit; // 스턴 지속 시간을 프레임으로 처리

            if (stunFrames < CSceneManager.Instance.comboOptions.minHitStun)
                stunFrames = (int)CSceneManager.Instance.comboOptions.minHitStun;

            stunTime = (float)stunFrames / (float)CSceneManager.Instance.fps;

            if (CSceneManager.Instance.characterRotationOptions.fixRotationOnHit)
                testCharacterRotation(100);

            stunTime *= hitStunModifer;

             // push force
             Vector2 pushForce;
            if(!myPhysicsScript.IsGrounded() && opHurtInfo.applyDifferentAirForce)
            {
                pushForce = opHurtInfo.pushForceAir;
            }
            else
            {
                pushForce = opHurtInfo.pushForce;
            }

            if (pushForce.y > 0 || isDead)
            {
                pushForce.y -= (pushForce.y * (float)airJuggleHits * .04f);

                if (pushForce.y < CSceneManager.Instance.comboOptions.minPushForce) // 20
                    pushForce.y = CSceneManager.Instance.comboOptions.minPushForce;

                airJuggleHits++;
            }

            if (CSceneManager.Instance.comboOptions.fixJuggleWeight)
            {
                myPhysicsScript.OnApplyNewWeight(CSceneManager.Instance.comboOptions.juggleWeight);
            }

            if (isDead)
                stunTime = 99999;

            if ((airHit || !myPhysicsScript.IsGrounded()) && pushForce.y > 0)
            {
                if (moveSetScript.basicMoves.getHitAir.clip1 == null)
                    Debug.LogError("character basic move 'getHitAir' animation clip no found! input clip data.");

                myPhysicsScript.ResetForces(opHurtInfo.resetPreviousHorizontalPush, opHurtInfo.resetPreviousVerticalPush);
                myPhysicsScript.AddForce(new Vector2(pushForce.x, pushForce.y), -1);

                currentHitAnimation = moveSetScript.basicMoves.getHitAir.name;
                currentHitInfo = moveSetScript.basicMoves.getHitAir;

                moveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, opHurtInfo.resetHitAnimations);

                if (currentHitInfo.isAutoSpeed)
                {
                    float airTime = myPhysicsScript.GetEnableAirTime(pushForce.y);
                    if (stunTime > airTime) stunTime = airTime;
                    moveSetScript.SetAnimationNomalizedSpeed(currentHitAnimation
                                                             , (moveSetScript.GetAnimationLength(currentHitAnimation) / stunTime));
                }
            }
            else
            {
                hitAnimationSpeed = moveSetScript.GetAnimationLength(currentHitAnimation) / stunTime;

                if(hitAnimationSpeed > 2)
                {
                    hitAnimationSpeed -= 1.0f;
                }

                if (!myPhysicsScript.isOverrideAirAnimation)
                {
                    myPhysicsScript.ResetForces(opHurtInfo.resetPreviousHorizontalPush, opHurtInfo.resetPreviousVerticalPush);
                    myPhysicsScript.AddForce(new Vector2(pushForce.x, pushForce.y), -1);
                }

                if(opHurtInfo.overrideHitAcceleration)
                {
                    hitStunDeceleration = hitAnimationSpeed / 3;
                }

                moveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, opHurtInfo.resetHitAnimations);

                if (currentHitInfo.isAutoSpeed && hitAnimationSpeed > 0)
                {
                    moveSetScript.SetAnimationSpeed(currentHitAnimation, hitAnimationSpeed);
                }
            }
        }

        // hit pause
        HitPause(GetHitAnimationSpeed((opHurtInfo.attackLevel)) * .01f);
        StartCoroutine(CallDelayUnPauseFunction(_hurtEffect.freezingTime));
    }

    // 상단 공격의 약/중/강 피격에 따른 애니메이션 클립 구분
    private string GetHitAnimationName(BasicMoveInfo hitMove, AttackLevel level)
    {
        if (level == AttackLevel.week) return hitMove.name;
        if (hitMove.clip2 != null && level == AttackLevel.mid) return moveSetScript.GetAnimationString(hitMove, 2);
        if (hitMove.clip2 != null && level == AttackLevel.strong) return moveSetScript.GetAnimationString(hitMove, 3);

        return hitMove.name;
    }

    // 가드 중 맞으면
    public void GetHitGuarding(ColliderInfo opHurtInfo, Vector3 location) 
    {
        if(opHurtInfo.damageOnGuard >= myCharacterInfo.currentLifePoints)
        {
            GetHit(opHurtInfo, location);
            return;
        }
        else
        {
            DamageMe(opHurtInfo.damageOnGuard);
        }

        if (currentState == PlayerState.crouch && opHurtInfo.attackRange == AttackRange.top)
            return;

        guardStunned = true;
        currentSubState = PlayerSubState.Guarding;

        myHitScript.isHit = true;
        myHitScript.hitType = HitType.Guard;
        hitStunDeceleration = -9999;

        int stunFrames = 0;
        BasicMoveInfo currentHitInfo = moveSetScript.basicMoves.blockingStandingPoseHit;

        stunFrames = (int)opHurtInfo.hitStunWhenBlock;
        if (stunFrames < 1)
            stunFrames = 1;

        stunTime = (float)stunFrames / CSceneManager.Instance.fps;

        // shakes
        shakeCamera = CSceneManager.Instance.guardHitOptions.isShakeCameraOnHit;
        shakeCharacter = CSceneManager.Instance.guardHitOptions.isShakeCharacterOnHit;
        shakeWeight = CSceneManager.Instance.guardHitOptions.shakeWeight;

        opPlayerScript.AddNormalGauge(opHurtInfo.gainGaugeOnHit / 2); // 적이 공격 맞췄을 때
        AddGuardingGauge(opHurtInfo.gainGaugeOnGuard); // 내가 가드했을 때
        AddNormalGauge(opHurtInfo.opGainGaugeOnHit / 2);

        // animation setting
        if (currentState == PlayerState.crouch)
        {
            currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.blockingCrouchingPoseHit, opHurtInfo.attackLevel);
            currentHitInfo = moveSetScript.basicMoves.blockingCrouchingPoseHit;

            if (!moveSetScript.GetAnimationExist(currentHitAnimation))
                Debug.LogError("Guarding stand pose animation not found! you have to input Character -> MoveSetScript -> basicMoves -> blockingCrouchingPoseHit");
        }
        else if (currentState == PlayerState.stand)
        { 
            currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.blockingStandingPoseHit, opHurtInfo.attackLevel);
            currentHitInfo = moveSetScript.basicMoves.blockingStandingPoseHit;

            if (!moveSetScript.GetAnimationExist(currentHitAnimation))
                Debug.LogError("Guarding stand pose animation not found! you have to input Character -> MoveSetScript -> basicMoves -> blockingStandingPoseHit");
        }

        moveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation);
        hitAnimationSpeed = moveSetScript.GetAnimationLength(currentHitAnimation) / stunTime;

        if(currentHitInfo.isAutoSpeed)
        {
            moveSetScript.SetAnimationSpeed(currentHitAnimation, hitAnimationSpeed);
        }

        if(myHitScript.overrideHitAcceleration)
        {
            hitStunDeceleration = hitAnimationSpeed / 3;
        }

        // hit pause
        HitPause(GetHitAnimationSpeed((opHurtInfo.attackLevel)) * .01f);
        StartCoroutine(CallDelayUnPauseFunction(GetHitFreezingTime(opHurtInfo.attackLevel)));

        float spaceBetweenHits = 1;
        if (opHurtInfo.attackLevel == AttackLevel.week)
        {
            spaceBetweenHits = 1.1f;
        }
        else if (opHurtInfo.attackLevel == AttackLevel.mid)
        {
            spaceBetweenHits = 1.3f;
        }
        else if (opHurtInfo.attackLevel == AttackLevel.strong)
        {
            spaceBetweenHits = 1.7f;
        }
        StartCoroutine(CallDelayResetHit(GetHitFreezingTime(opHurtInfo.attackLevel) * spaceBetweenHits));

        myPhysicsScript.ResetForces(opHurtInfo.resetPreviousHorizontalPush, opHurtInfo.resetPreviousVertical); // true, true
        myPhysicsScript.AddForce(new Vector3(opHurtInfo.pushForce.x, 0, 0), -1);
    }

    // 그냥 가드 자세만 취하는 거임
    public void CheckGuarding(bool flag, ButtonPress opAttackType)
    {
        if (myPhysicsScript.freeze) return;
        if (myPhysicsScript.isTakingOff) return;

        if(flag)
        {
            if(currentMove != null)
            {
                potentialGuard = false;
                return;
            }
            
            if(currentState == PlayerState.crouch)
            {
                if (opPlayerScript.currentState == PlayerState.stand && opAttackType == ButtonPress.SmallKick)
                {
                    if (moveSetScript.basicMoves.blockingCrouchingPose.clip1 == null)
                        Debug.LogError("'blockingCrouchingPose' Animation not Exist!! plz input MoveInfo animation at Character-> MoveSetScript -> baskcMove / Mecanim -> all moves");

                    moveSetScript.PlayBasicMove(moveSetScript.basicMoves.blockingCrouchingPose, false);
                    isGuarding = true;
                }
                else return;
            }
            else if(currentState == PlayerState.stand)
            {
                if(moveSetScript.basicMoves.blockingStandingPose.clip1 == null)
                    Debug.LogError("'blockingStandingPose' Animation not Exist!! plz input MoveInfo animation at Character-> MoveSetScript -> baskcMove / Mecanim -> all moves");

                moveSetScript.PlayBasicMove(moveSetScript.basicMoves.blockingStandingPose, false);
                isGuarding = true;
            }
        }
        else if(!guardStunned)
        {
            isGuarding = false;
            myHitScript.SetAllHitBoxes(false);
        }
    }

    public bool IsGround()
    {
        if(Physics.RaycastAll(transform.position + new Vector3(0,2f,0), Vector3.down, 2.02f, groundMask).Length >0)
        {
            if(transform.position.y != 0 )
            {
                transform.Translate(new Vector3(0, -transform.position.y, 0));
            }
            return true;
        }
        return false;
    }

    public void KillCurrentMove()
    {
        if (currentMove == null) return;

        foreach (ApplyForce resetForce in currentMove.applyForces)
        {
            resetForce.casted = false;
        }
        currentMove.voiceIdx = -1;
        foreach (SoundEffects soundeffect in currentMove.soundEffects)
        {
            soundeffect.casted = false;
        }
        foreach(CameraMovement cameraMovement in currentMove.cameraMovements)
        {
            cameraMovement.over = false;
            cameraMovement.casted = false;
            cameraMovement.time = 0;
        }
        foreach(MoveParticleEffect moveEffect in currentMove.particleEffects)
        {
            moveEffect.casted = false;
        }
        foreach (DistanceParticleEffect distanceEffect in currentMove.distanceEffects)
        {
            if (distanceEffect.casted) distanceEffect.casted = false;
        }
        foreach(ColliderInfo info in currentMove.hurtInfo)
        {
            info.curHitEnabled = false;
            if (info.hurtBoxes.Length > 0)
            {
                for (int i = 0; i < info.hurtBoxes.Length; i++)
                {
                    if(info.hurtBoxes[i].casted)
                    {
                        myHurtScript.AttackColliderOff();
                        info.hurtBoxes[i].casted = false;
                    }
                }
            }
        }
        foreach (Projectile projectile in currentMove.projectile)
        {
            projectile.casted = false;
        }

        if (currentMove.gaugeSkillType == GaugeSkillType.Guard && opPlayerScript.hitedSkill)
        {
            for (int i = 0; i < currentMove.distanceEffects.Length; i++)
            {
                // 본인 힐 스킬이면
                if (currentMove.distanceEffects[i].isMine && currentMove.distanceEffects[i].healingData.enableHealing)
                {
                    opPlayerScript.PausePlayAnimation(false);
                    opPlayerScript.hitedSkill = false;
                    opPlayerScript.myPhysicsScript.freeze = false;
                }
            }
        }

        currentMove.currentFrame = 0;
        currentMove.currentTick = 0;

        ignoreCollisionMass = false;

        if (currentMove.invertRotationLeft && mirror == -1)
            InvertRotation();
        if (currentMove.invertRotationRight && mirror == 1)
            InvertRotation();

        this.SetMove(null);
        OnReleaseCam();
    }

    public void CastMove(MoveInfo move, bool forceGrounded)
    {
        if (move == null) return;

        SetMove(move);
        if (forceGrounded) myPhysicsScript.ForceGrounded();
    }

    public void SetMove(MoveInfo move)
    {
        currentMove = move;
    }

    public bool CheckGuardRange(AttackRange range, HurtType type)
    {
        if (range == AttackRange.none) return false;
        if (range == AttackRange.top  && currentState == PlayerState.crouch) return false;
        if (type == HurtType.launcher && !myPhysicsScript.IsGrounded()) return false;
        return true;
    }

    void HitPause(float animSpeed)
    {
        if (shakeCamera)
            Camera.main.transform.position += Vector3.forward / 2;

        myPhysicsScript.freeze = true;
        PausePlayAnimation(true, animSpeed);
    }

    void HitUnpause()
    {
        if (cameraScript.cinematicFreeze)
            return;

        myPhysicsScript.freeze = false;
        PausePlayAnimation(false);
    }

    private void PausePlayAnimation(bool pause)
    {
        PausePlayAnimation(pause, 0);
    }

    private void PausePlayAnimation(bool pause, float animSpeed)
    {
        if (animSpeed < 0)
            animSpeed = 0;

        if(pause)
        {
            moveSetScript.SetAnimationSpeed(animSpeed);
        }
        else
        {
            moveSetScript.RestoreAnimationSpeed();
        }
    }

    // currentMove > 호출된 애니메이션에 대한 정보 체크
    public void CheckMove(MoveInfo move)
    {
        if (move == null) return;

        potentialGuard = false;

        if(move.currentTick == 0)
        {
            if (!moveSetScript.GetAnimationExist(move.moveName))
                Debug.LogError("Animation not Exist!! plz input MoveInfo animation, name : " + move.name);

            if(myPhysicsScript.IsGrounded())
            {
                myPhysicsScript.isTakingOff = false;
                myPhysicsScript.isLanding = false;
            }

            if(currentState == PlayerState.neutralJump ||
               currentState == PlayerState.forwardJump ||
               currentState == PlayerState.backJump)
            {
                moveSetScript.totalAirMoves++;
            }

            float nomalizedTimeConv = moveSetScript.GetAnimationNomalizedTime(move.overrideStartUpFrame, move);

            if(move.overrideBlendingIn)
            {
                moveSetScript.PlayAnimation(move.moveName, move.blendingIn, nomalizedTimeConv);
            }
            else
            {
                moveSetScript.PlayAnimation(move.moveName, myCharacterInfo.blendingTime, nomalizedTimeConv);
            }

            if (currentMove.invertRotationLeft && mirror == -1) InvertRotation();
            if (currentMove.invertRotationRight && mirror == 1) InvertRotation();

            move.currentTick = move.overrideStartUpFrame;
            move.currentFrame = move.overrideStartUpFrame;
            move.animationSpeedTemp = move.animationSpeed;

            moveSetScript.SetAnimationSpeed(move.moveName, move.animationSpeed);
        }

        // animation frame data
        if (moveSetScript.animationPaused)
            move.currentTick += Time.fixedDeltaTime * CSceneManager.Instance.fps * moveSetScript.GetAnimationSpeed();
        else
            move.currentTick += Time.fixedDeltaTime * CSceneManager.Instance.fps;

        if (move.currentTick > move.currentFrame)
            move.currentFrame++;

        // create launcher animation's projectiles
        foreach(Projectile projectile in move.projectile)
        {
            if(!projectile.casted &&
                projectile.projectilePrefab != null &&
                move.currentFrame >= projectile.castingFrame)
            {
                projectile.casted = true;

                Vector3 newPos = myHitScript.GetPosition(projectile.bodyPart);
                newPos.z = projectile.castingOffSet.z;
                GameObject temp = (GameObject)Instantiate(projectile.projectilePrefab
                                                          , newPos
                                                          , Quaternion.identity);

                ProjectileMoveScript projectileMoveScript = temp.GetComponent<ProjectileMoveScript>();
                if (projectileMoveScript == null) Debug.LogError("'ProjectileMoveScript' in Projectile is NOT Found! Input Script & Datas. ");
                projectileMoveScript.speed = projectile.speed;
                projectileMoveScript.mirror = mirror;

                ColliderInfo collInfo = temp.GetComponent<ColliderInfo>();
                if (collInfo == null) Debug.LogError("'ColliderInfo' in Projectile is NOT Found! Input Script & Datas. ");
                collInfo.myScript = this;

                projectiles.Add(projectileMoveScript);
                projectileMoveScript.OnStart(projectile.duration);
            }
        }

        // move particle effect
        foreach(MoveParticleEffect particleEffect in move.particleEffects)
        {
            if(!particleEffect.casted
                && particleEffect.particleEffect.prefab != null
                && move.currentFrame >= particleEffect.castingFrame)
             {
                particleEffect.casted = true;
                GameObject temp = (GameObject)Instantiate(particleEffect.particleEffect.prefab);

                if(particleEffect.isAttachMesh)
                {
                    Transform parent = moveSetScript.transform;
                    temp.transform.parent = parent;

                    PSMeshRendererUpdater meshScript = temp.GetComponent<PSMeshRendererUpdater>();
                    meshScript.MeshObject = moveSetScript.GetOwnerPartObject(particleEffect.particleEffect.bodyPart);
                    meshScript.UpdateMeshEffect();
                    meshScript.UpdatePSMesh();
                }
                else
                {
                    Transform parent = moveSetScript.GetOwnerPartObject(particleEffect.particleEffect.bodyPart).transform;
                    temp.transform.parent = parent;
                }

                Vector3 newPosition = Vector3.zero;
                newPosition.x = particleEffect.particleEffect.positionOffSet.x * -mirror;
                newPosition.y = particleEffect.particleEffect.positionOffSet.y;
                newPosition.z = particleEffect.particleEffect.positionOffSet.z;
                temp.transform.localPosition = newPosition;
                Destroy(temp, particleEffect.duration);
            }
        }

        // distance effects
        foreach (DistanceParticleEffect dEffect in move.distanceEffects)
        {
            if (!dEffect.casted
                && dEffect.prefab != null
                && move.currentFrame >= dEffect.castingFrame)
            {
                dEffect.casted = true;
                GameObject temp = (GameObject)Instantiate(dEffect.prefab);

                Vector3 newPosition = Vector3.zero;
                if (dEffect.isMine)
                {
                    newPosition = this.transform.position;
                }
                else
                {
                    newPosition = opPlayerScript.transform.position;
                }
                
                newPosition.x += dEffect.positionOffSet.x;
                newPosition.y += dEffect.positionOffSet.y;
                newPosition.z += dEffect.positionOffSet.z;
                temp.transform.position = newPosition;
                dEffect.distroyClone = temp;
                Destroy(temp, dEffect.duration);

                if(dEffect.isMine && dEffect.healingData.enableHealing)
                {
                    myCharacterInfo.currentLifePoints += dEffect.healingData.healValue;
                    if (myCharacterInfo.currentLifePoints > myCharacterInfo.maxLifePoints)
                        myCharacterInfo.currentLifePoints = myCharacterInfo.maxLifePoints;
                    gameManager.defaultUiCtrl.SetHP(player_num, myCharacterInfo.currentLifePoints, false);
                    return;
                }

                ColliderInfo collInfo = temp.GetComponent<ColliderInfo>();
                if (collInfo == null) Debug.LogError("'ColliderInfo' in DistanceParticleEffect is NOT Found! Input Script & Datas. ");
                collInfo.myScript = this;
                collInfo.healData = dEffect.healingData;
            }
            else if (dEffect.casted && move.currentFrame >= dEffect.castingEndFrame)
            {
                SphereCollider collider = dEffect.distroyClone.GetComponent<SphereCollider>();
                if (collider != null) collider.enabled = false;
            }
        }

        // check apply forces
        foreach (ApplyForce addedForce in move.applyForces)
        {
            if (!addedForce.casted && move.currentFrame >= addedForce.castingFrame)
            {
                myPhysicsScript.ResetForces(addedForce.resetPreviousHorizontal, addedForce.resetPreviousVertical);
                myPhysicsScript.AddForce(addedForce.force, 1);

                addedForce.casted = true;
            }
        }

        // check sound effects
        if(move.voiceRandom)
        {
            if(move.voiceIdx < 0)
            {
                move.voiceIdx = Random.Range(0, move.soundEffects.Length);
            }

            if (!move.soundEffects[move.voiceIdx].casted 
                && move.currentFrame >= move.soundEffects[move.voiceIdx].castingFrame)
            {
                move.soundEffects[move.voiceIdx].casted = true;
                //                      skill                                   dash, intro
                if (move.gaugeSkillType != GaugeSkillType.none || move.attackStateType == PlayerState.none)
                {
                    CGameManager.PlaySoundFX(move.soundEffects[move.voiceIdx].sound);
                }
                else  //    normal attacks, basic command 
                {
                    if (move.soundEffects[move.voiceIdx].playRandomValue == 0)
                        CGameManager.PlaySoundFX(move.soundEffects[move.voiceIdx].sound);
                    else
                    {
                        if (CSceneManager.Instance.MyRandom(move.soundEffects[move.voiceIdx].playRandomValue))
                            CGameManager.PlaySoundFX(move.soundEffects[move.voiceIdx].sound);
                    }
                }
            }
        }
        else
        {
            foreach (SoundEffects soundEffect in move.soundEffects)
            {
                if (!soundEffect.casted && move.currentFrame >= soundEffect.castingFrame)
                {
                    soundEffect.casted = true;
                    //                      skill                                   dash, intro
                    if (move.gaugeSkillType != GaugeSkillType.none || move.attackStateType == PlayerState.none)
                    {
                        CGameManager.PlaySoundFX(soundEffect.sound);
                    }
                    else  //    normal attacks, basic command 
                    {

                        if (soundEffect.playRandomValue == 0)
                            CGameManager.PlaySoundFX(soundEffect.sound);
                        else
                        {
                            if (CSceneManager.Instance.MyRandom(soundEffect.playRandomValue))
                                CGameManager.PlaySoundFX(soundEffect.sound);
                        }
                    }
                }
            }
        }

        // check Cinematic Camera Movements
        foreach (CameraMovement cameraMovement in move.cameraMovements)
        {
            if (cameraMovement.over) continue;

            // 시네마팅(근접) 카메라 무빙 끝났을 경우
            if(cameraMovement.casted && !cameraMovement.over && cameraMovement.time >= cameraMovement.duration && CSceneManager.freeCamera)
            {
                if(move.gaugeSkillType == GaugeSkillType.Skill2)
                {
                    cameraMovement.over = true;
                    PausePlayAnimation(true, 0);
                    opPlayerScript.PausePlayAnimation(true, 0);
                    gameManager.defaultUiCtrl.OnMakeClone(fighterName);
                }
                else
                {
                    cameraMovement.over = true;
                    OnReleaseCam();
                }
            }

            if(move.currentFrame >= cameraMovement.castingFrame)
            {
                cameraMovement.time += Time.fixedDeltaTime;
                if (cameraMovement.casted) continue;
                cameraMovement.casted = true;

                PausePlayAnimation(true, cameraMovement.myAnimationSpeed * .01f);

                if (!cameraMovement.inSkillUseCineCamMove)
                {
                    opPlayerScript.PausePlayAnimation(true, cameraMovement.opAnimationSpeed * .01f);

                    CSceneManager.freezePhysics = cameraMovement.freezePhysics;
                    myPhysicsScript.freeze = cameraMovement.freezePhysics;
                    opPlayerScript.myPhysicsScript.freeze = cameraMovement.freezePhysics;
                    cameraScript.cinematicFreeze = cameraMovement.freezePhysics;

                    // 값은 1p 기준
                    //Vector3 movement = cameraMovement.position;
                    //movement.x *= mirror;
                    Vector3 targetPosition = transform.TransformPoint(cameraMovement.position);
                    Vector3 targetRotation = cameraMovement.rotation;
                    targetRotation.y *= -mirror;
                    cameraScript.MoveCameraToLocation(targetPosition, targetRotation, cameraMovement.fieldOfView, cameraMovement.camSpeed, gameObject.name);
                }
                else
                {
                    cameraScript.MoveCameraToLocation(gameObject.name);
                }
            }
        }

        // check guardable area
        if(move.hasGuardableArea) // 내가 가드 영역을 가지고 있고
        {
            if(!opPlayerScript.isGuarding //  적이 아직 가드 자세를 취하지 않았고
                && !opPlayerScript.guardStunned // 적이 가드 상태에서 안맞았고
                && opPlayerScript.currentSubState != PlayerSubState.stunned // 적이 맞은 상태가 아니고
                && opPlayerScript.myHitScript.GetHitBoxesGuardableAreaHit() // 가드 콜라이더 충돌한게 1개라도 잇으면
                && (myHurtScript.hurtBoxObj != null && myHurtScript.hurtBoxObj.activeSelf))
            {
                opPlayerScript.CheckGuarding(true, move.attackButtonPress);
            }
            else if (myHurtScript.hurtBoxObj == null
                     && opPlayerScript.currentSubState != PlayerSubState.stunned)
            {
                opPlayerScript.CheckGuarding(false, move.attackButtonPress);
            }
        }

        // check Frame Links
        // 현재 애니메이션 도중 다른 애니메이션으로 즉시 링크 가능한 경우
        foreach(MoveLink frameLink in move.frameLinks)
        {
            if(move.currentFrame >= frameLink.activeFramesBegins &&
               move.currentFrame <= frameLink.activeFramesEnds)
            {
                if(frameLink.linkType == LinkType.NoConditions ||
                    (frameLink.linkType == LinkType.HitConfirm &&
                    (currentMove.hitConfirmOnStrike && frameLink.onStrike) ||
                    (currentMove.hitConfirmOnGuard && frameLink.onGuard)))
                {
                    frameLink.cancelable = true;
                    move.cancelable = true;
                }
            }
            else
            {
                frameLink.cancelable = false;
            }
        }

        // hurtboxes on/off / my hutybox opponent hit true 
        foreach (ColliderInfo info in move.hurtInfo)
        {
            if (move.currentFrame >= info.activeFrameBegin &&
                move.currentFrame <= info.activeFrameEnd)
            {
                if (info.hurtBoxes.Length > 0)
                {
                    if (info.curHitEnabled) continue;

                    // collider on
                    for (int i = 0; i < info.hurtBoxes.Length; i++)
                    {
                        if (!info.hurtBoxes[i].casted)
                        {
                            myHurtScript.AttackColliderOn(info.hurtBoxes[i].colliderStr);
                            info.hurtBoxes[i].casted = true;
                        }
                    }

                    #region push force
                    if (opPlayerScript.myHitScript.isHit)
                    {
                        if (myHitScript.hitType == HitType.Guard)
                            move.hitConfirmOnGuard = true;
                        else if (myHitScript.hitType == HitType.Strike)
                            move.hitConfirmOnStrike = true;

                        // 기본으로 밀리는 값 appliedForce
                        myPhysicsScript.ResetForces(info.resetPreviousHorizontal, info.resetPreviousVertical);
                        myPhysicsScript.AddForce(info.appliedForce, -1);

                        // 적이 벽에 닿았을 경우 내가 밀리는 값 pushForce
                        if ((opPlayerScript.transform.position.x >= CGameManager.selectStateOption.rightBoundary - 0.5f ||
                            opPlayerScript.transform.position.x <= CGameManager.selectStateOption.leftBoundary + 0.5f)
                            && myPhysicsScript.IsGrounded()
                            && !CSceneManager.Instance.comboOptions.neverCornerPush
                            && info.cornerPush
                            && info.hurtType != HurtType.skill
                            && opPlayerScript.strongStunnedTime <= 0)
                        {
                            myPhysicsScript.ResetForces(info.resetPreviousHorizontal, false);
                            myPhysicsScript.AddForce(new Vector2(info.pushForce.x +
                                                                (opPlayerScript.myPhysicsScript.airTime *
                                                                    opCharacterInfo.physics.friction), 0), -1);
                        }

                        if (opPlayerScript.myPhysicsScript.freeze && opPlayerScript.strongStunnedTime <= 0)
                        {
                            float newAnimSpeed = info.hitEffects.animationSpeed;
                            float freezingTime = info.hitEffects.freezingTime;

                            if (!info.overrideHitEffects)
                            {
                                newAnimSpeed = GetHitAnimationSpeed(info.attackLevel);
                                freezingTime = GetHitFreezingTime(info.attackLevel);
                            }

                            HitPause(newAnimSpeed * .01f);
                            StartCoroutine(CallDelayUnPauseFunction(freezingTime));
                        }
                        if (!info.isContinueHit) info.curHitEnabled = true;
                    }
                    #endregion
                }
            }
            else if (move.currentFrame > info.activeFrameEnd)
            {
                // collider off
                if (info.hurtBoxes.Length > 0)
                {
                    for (int i = 0; i < info.hurtBoxes.Length; i++)
                    {
                        if (info.hurtBoxes[i].casted)
                        {
                            myHurtScript.AttackColliderOff();
                            info.hurtBoxes[i].casted = false;
                        }
                    }
                }
            }
        }

        if (move.currentFrame >= move.totalFrames)
        {
            if(move.moveName == "Intro")
            {
                introPlayed = true;
                CSceneManager.Instance.CastNewRound();
            }

            KillCurrentMove();
        }
    }

    public float GetHitAnimationSpeed(AttackLevel hurtLevel)
    {
        switch(hurtLevel)
        {
            case AttackLevel.week:
                return CSceneManager.Instance.hurtOptions.weekAttack.animationSpeed;
            case AttackLevel.mid:
                return CSceneManager.Instance.hurtOptions.midAttack.animationSpeed;
            case AttackLevel.strong:
                return CSceneManager.Instance.hurtOptions.strongAttack.animationSpeed;
        }
        return 0;
    }

    public float GetHitFreezingTime(AttackLevel hurtLevel)
    {
        switch (hurtLevel)
        {
            case AttackLevel.week:
                return CSceneManager.Instance.hurtOptions.weekAttack.freezingTime;
            case AttackLevel.mid:
                return CSceneManager.Instance.hurtOptions.midAttack.freezingTime;
            case AttackLevel.strong:
                return CSceneManager.Instance.hurtOptions.strongAttack.freezingTime;
        }

        return 0;
    }

    private void OnReleaseStun()
    {
        if (currentSubState != PlayerSubState.stunned && !guardStunned) return;

        comboHits = 0;
        currentHit = null;
        currentSubState = PlayerSubState.resting;
        stunTime = 0;
        airJuggleHits = 0;
        potentialGuard = false;
        guardStunned = false;
        CheckGuarding(false, ButtonPress.none);
        myHitScript.hitType = HitType.none;
        myPhysicsScript.OnResetApplyWeight();
        myPhysicsScript.isWallBouncing = false;
        myPhysicsScript.isOverrideAirAnimation = false;

        if (!myPhysicsScript.IsGrounded()) isAirRecovering = true;
        if (myPhysicsScript.IsGrounded()) currentState = PlayerState.stand;

        ButtonController buttonController = CSceneManager.Instance.GetButtonController(player_num);
        CheckInputKey(buttonController);
    }

    public void OnReleaseCam()
    {
        if (cameraScript.GetCameraOwner() != gameObject.name) return;
        if (outroPlayed && CSceneManager.Instance.roundOptions.freezeCamAfterOutro) return;

        cameraScript.ReleaseCam();
        CSceneManager.freezePhysics = false;
        myPhysicsScript.freeze = false;
        PausePlayAnimation(false);

        // 가드스킬을 쓴 경우
        if (opPlayerScript.hitedSkill && currentMove != null && currentMove.gaugeSkillType == GaugeSkillType.Guard)
        {
            for (int i = 0; i < currentMove.distanceEffects.Length; i++)
            {
                // 본인 힐 스킬이면
                if(currentMove.distanceEffects[i].isMine && currentMove.distanceEffects[i].healingData.enableHealing)
                {
                    return;
                }
            }
        }

        opPlayerScript.PausePlayAnimation(false); // 적의 일시정지를 풀어준다
        opPlayerScript.myPhysicsScript.freeze = false;
    } 

    public bool AddStrongStunnd(float gainValue)
    {
        myCharacterInfo.currentStrongStunPoints += gainValue;
        if (myCharacterInfo.currentStrongStunPoints >= myCharacterInfo.maxStrongStunPoints)
        {
            myCharacterInfo.currentStrongStunPoints = myCharacterInfo.maxStrongStunPoints;
            gameManager.defaultUiCtrl.StunGageCtrl(player_num, myCharacterInfo.currentStrongStunPoints);
            return true;
        }
        gameManager.defaultUiCtrl.StunGageCtrl(player_num, myCharacterInfo.currentStrongStunPoints);
        return false;
    }

    public void AddNormalGauge(float gainValue)
    {
        myCharacterInfo.currentGaugePoints += gainValue;
        if (myCharacterInfo.currentGaugePoints > myCharacterInfo.maxGaugePoints)
            myCharacterInfo.currentGaugePoints = myCharacterInfo.maxGaugePoints;

        gameManager.defaultUiCtrl.IncreaseSkillGaugeBar(player_num, gainValue);
    }

    public void AddGuardingGauge(float gainValue)
    {
        myCharacterInfo.currentGuardGaugePoints += gainValue;
        if (myCharacterInfo.currentGuardGaugePoints > myCharacterInfo.maxGuardGaugePoints)
            myCharacterInfo.currentGuardGaugePoints = myCharacterInfo.maxGuardGaugePoints;

        gameManager.defaultUiCtrl.IncreaseGuardGaugeBar(player_num, gainValue);
    }

    public void UseGauge(GaugeSkillType skillType)
    {
        if ((isDead || opPlayerScript.isDead) && CSceneManager.Instance.roundOptions.inhibitGaugeGain) return;
        if (CGameManager.gameMode == GameMode.TrainingRoom && player_num == 1 && CSceneManager.Instance.trainingRoomOptions.p1Guage == LifeBarTrainingMode.infinite) return;
        if (CGameManager.gameMode == GameMode.TrainingRoom && player_num == 2 && CSceneManager.Instance.trainingRoomOptions.p2Gauge == LifeBarTrainingMode.infinite) return;
        if (skillType == GaugeSkillType.none) return;

        if(skillType == GaugeSkillType.Guard)
        {
            myCharacterInfo.currentGuardGaugePoints -= myCharacterInfo.maxGuardGaugePoints;
            gameManager.defaultUiCtrl.DecreaseGuard(player_num);
            opPlayerScript.hitedSkill = true;
        }
        else if(skillType == GaugeSkillType.Skill2) // 궁극기
        {
            myCharacterInfo.currentGaugePoints -= myCharacterInfo.maxGaugePoints;
            gameManager.defaultUiCtrl.DecreaseSkillGaugeBar(player_num, skillType);
        }
        else if(skillType == GaugeSkillType.Skill1)
        {
            myCharacterInfo.currentGaugePoints -= myCharacterInfo.maxGaugePoints / 2;
            gameManager.defaultUiCtrl.OnHalfSkillImageAnimation(fighterName, mirror);
            gameManager.defaultUiCtrl.DecreaseSkillGaugeBar(player_num, skillType);
        }

        if (myCharacterInfo.currentGuardGaugePoints < 0 || myCharacterInfo.currentGaugePoints < 0)
            Debug.LogError("gauge value is minus !! ");

        if ((player_num == 1 && CGameManager.gameMode == GameMode.TrainingRoom && CSceneManager.Instance.trainingRoomOptions.p1Guage == LifeBarTrainingMode.refill)
            || (player_num == 2 && CGameManager.gameMode == GameMode.TrainingRoom && CSceneManager.Instance.trainingRoomOptions.p2Gauge == LifeBarTrainingMode.refill))
        {
            if (!isGoingRefillGauge)
            {
                isGoingRefillGauge = true;
                Invoke("RefillGauge", CSceneManager.Instance.trainingRoomOptions.refillTime);
            }
        }
    }

    public void ResetDrainGauges()
    {
        myCharacterInfo.currentGaugePoints = 0;
        myCharacterInfo.currentGuardGaugePoints = 0;
        gameManager.defaultUiCtrl.OnRoundInit();
    }

    private bool HeldDownMoveExcution(ButtonPress axisPress, bool inputUp)
    {
        return HeldDownMoveExcution(axisPress, inputUp, false);
    }

    private bool HeldDownMoveExcution(ButtonPress axisPress, bool inputUp, bool forceExecution)
    {
        MoveInfo tempMove = moveSetScript.GetMove(new ButtonPress[] { axisPress }, new string[] { "" }, 0, currentMove, currentState, player_num, inputUp, forceExecution);
        if(tempMove != null)
        {
            storedMove = tempMove;
            storedMoveTime = ((float)CSceneManager.Instance.executionBufferTime / CSceneManager.Instance.fps);
            return true;
        }
        return false;
    }

    private void ShakeCam()
    {
        float range = Random.Range(-.2f * shakeWeight, .2f * shakeWeight);
        Camera.main.transform.position += new Vector3(range, range, 0);
    }

    private void ShakePlayer()
    {
        float range = Random.Range(-.1f * shakeWeight, .2f * shakeWeight);
        Camera.main.transform.position += new Vector3(range, 0, 0);
    }
    private bool DamageMe(float damage, bool doesntKill) 
    {
        if (doesntKill && damage >= myCharacterInfo.currentLifePoints)
            damage = myCharacterInfo.currentLifePoints - 1;

        return DamageMe(damage);
    }

    private bool DamageMe(float damage)
    {
        if (CGameManager.gameMode == GameMode.TrainingRoom && player_num == 1 && CSceneManager.Instance.trainingRoomOptions.p1Life == LifeBarTrainingMode.infinite) return false;
        if (CGameManager.gameMode == GameMode.TrainingRoom && player_num == 2 && CSceneManager.Instance.trainingRoomOptions.p2Life == LifeBarTrainingMode.infinite) return false;
        if (myCharacterInfo.currentLifePoints <= 0 || opCharacterInfo.currentLifePoints <= 0) return true;
        if (gameManager.defaultUiCtrl.GetTimePreTime() <= 0 && CSceneManager.Instance.roundOptions.hasTimer) return true;

        myCharacterInfo.currentLifePoints -= damage;
        if (myCharacterInfo.currentLifePoints < 0) myCharacterInfo.currentLifePoints = 0;
        gameManager.defaultUiCtrl.SetHP(player_num, myCharacterInfo.currentLifePoints, false);

        if ((player_num == 1 && CGameManager.gameMode == GameMode.TrainingRoom && CSceneManager.Instance.trainingRoomOptions.p1Life == LifeBarTrainingMode.refill) ||
            (player_num == 2 && CGameManager.gameMode == GameMode.TrainingRoom && CSceneManager.Instance.trainingRoomOptions.p2Life == LifeBarTrainingMode.refill))
        {
            if (myCharacterInfo.currentLifePoints == 0) myCharacterInfo.currentLifePoints = myCharacterInfo.maxLifePoints; 
            if(!isGoingRefillLife)
            {
                isGoingRefillLife = true;
                Invoke("RefillLife", CSceneManager.Instance.trainingRoomOptions.refillTime);
            }
        }

        if (CGameManager.gameMode == GameMode.TrainingRoom && player_num == 1 && CSceneManager.Instance.trainingRoomOptions.p1Life != LifeBarTrainingMode.normal) return false;
        if (CGameManager.gameMode == GameMode.TrainingRoom && player_num == 2 && CSceneManager.Instance.trainingRoomOptions.p2Life != LifeBarTrainingMode.normal) return false;

        if(myCharacterInfo.currentLifePoints == 0)
        {
            gameManager.defaultUiCtrl.KOimgSet(true);
            CGameManager.PlaySoundFX(CSceneManager.Instance.announcerSounds.ko);

            // timer pause
            gameManager.defaultUiCtrl.OnSetTimeControlRoundEvent(true);

            if (CSceneManager.Instance.roundOptions.allowMovementEnd)
            {
                CSceneManager.Instance.lockMovement = true;
                CSceneManager.Instance.lockInputs = true;
            }

            if(CSceneManager.Instance.roundOptions.slowMotionKO)
            {
                gameManager.defaultUiCtrl.OnSetRunning(false);
                Time.timeScale = Time.timeScale * CSceneManager.Instance.roundOptions.slowMotionSpeed;
                Invoke("ReturnTimeScale", CSceneManager.Instance.roundOptions.slowMotionTimer * CSceneManager.Instance.roundOptions.slowMotionSpeed);
            }
            else
            {
                Invoke("EndRound", 1.5f);
            }

            return true;
        }

        return false;
    }

    private void RefillLife()
    {
        myCharacterInfo.currentLifePoints = myCharacterInfo.maxLifePoints;
        gameManager.defaultUiCtrl.SetHP(player_num, myCharacterInfo.currentLifePoints, true);
        isGoingRefillLife = false;
    }

    private void RefillGauge()
    {
        isGoingRefillGauge = false;
        AddNormalGauge(myCharacterInfo.maxGaugePoints);
        AddGuardingGauge(myCharacterInfo.maxGuardGaugePoints);
    }

    private void ReturnTimeScale()
    {
        Time.timeScale = CSceneManager.Instance.gameSpeed;
        Invoke("EndRound", 2f);
    }

    public void EndRound(float delay)
    {
        Invoke("EndRound", delay);
    }

    private void EndRound()
    {
        CSceneManager.Instance.lockMovement = true;
        CSceneManager.Instance.lockInputs = true;

        if (!opPlayerScript.myPhysicsScript.IsGrounded() || !myPhysicsScript.IsGrounded())
        {
            Invoke("EndRound", .5f);
            return;
        }

        // reset stats
        KillCurrentMove();
        opPlayerScript.KillCurrentMove();

        // clear all colliders
        myBodyCollisionInfo.gameObject.SetActive(false);
        myHitScript.gameObject.SetActive(false);
        myHitScript.gameObject.SetActive(false);

        // round options reset
        gameManager.defaultUiCtrl.KOimgSet(false);
        gameManager.defaultUiCtrl.OnTimeOverView(false);

        // perfect
        if (opPlayerScript.myCharacterInfo.currentLifePoints == opPlayerScript.myCharacterInfo.maxLifePoints)
            gameManager.defaultUiCtrl.OnPerfactKOView(true);
        else // opponent win
            gameManager.defaultUiCtrl.OnWinView(opPlayerScript.player_num, opPlayerScript.fighterName, true);
        gameManager.defaultUiCtrl.OnVictoryView(opPlayerScript.player_num, ++opPlayerScript.roundsWin);

        // start new round event
        if (opPlayerScript.roundsWin > Mathf.Ceil(CSceneManager.Instance.roundOptions.totalRound / 2))
        {
            opPlayerScript.SetMoveFinalWinOutro();
            Invoke("EndGame", CSceneManager.Instance.roundOptions.endGameDelay);
        }
        else
        {
            Invoke("NewRound", CSceneManager.Instance.roundOptions.newRoundDelay);
        }
    }

    public void SetMoveFinalWinOutro()
    {
        if(player_num == 1)         CGameManager.PlaySoundFX(CSceneManager.Instance.announcerSounds.player1Win);
        else if (player_num == 1)   CGameManager.PlaySoundFX(CSceneManager.Instance.announcerSounds.player2Win);

        SetMove(moveSetScript.outro);

        if(currentMove != null)
        {
            currentMove.currentFrame = 0;
            currentMove.currentTick = 0;
        }
        outroPlayed = true;
    }

    private void EndGame()
    {
        gameManager.defaultUiCtrl.OnWinView(opPlayerScript.player_num, opPlayerScript.fighterName, false);
        gameManager.defaultUiCtrl.OnPerfactKOView(false);
        cameraScript.killCamMove = true;

        DestroyImmediate(gameManager.defaultUiCtrl.gameObject);
        GameObject endGame = Instantiate(Resources.Load("UI/EndGame")) as GameObject;
        endGame.transform.parent = gameManager.finalUiObj.transform;
        endGame.transform.localPosition = Vector3.zero;
        endGame.transform.localScale = Vector3.one;

        gameManager.defaultUiCtrl = (DefaultUIControl)endGame.GetComponent<EndGameControl>();
        if (gameManager.defaultUiCtrl == null) Debug.LogError("UI / EndGameControl script is NOT FIND!");
        gameManager.OnInitDefaultUi();
    }

    public void NewRound(float delay)
    {
        Invoke("NewRound", delay);
    }

    public void NewRound()
    {
        potentialGuard = false;
        opPlayerScript.potentialGuard = false;
        Invoke("StartNewRound", 1);
    }

    private void StartNewRound()
    {
        CSceneManager.Instance.currentRound++;
        if (CSceneManager.Instance.currentRound > 3)
        {
            if (roundsWin == opPlayerScript.roundsWin)
                CSceneManager.Instance.currentRound = 3;
            else
                CSceneManager.Instance.currentRound = 1;
        }

        gameManager.defaultUiCtrl.ResetTimeDatas(CSceneManager.Instance.roundOptions.timer);
        gameManager.defaultUiCtrl.OnTimeOverView(false);
        gameManager.defaultUiCtrl.OnPerfactKOView(false);

        ResetData(true);
        opPlayerScript.ResetData(true);

        if(CSceneManager.Instance.roundOptions.resetPositions)
        {
            cameraScript.ResetCam();
        }

        CSceneManager.Instance.lockInputs = true;
        CSceneManager.Instance.ResetRoundCast();
        CSceneManager.Instance.CastNewRound();

        if(CSceneManager.Instance.roundOptions.allowMovementStart)
        {
            CSceneManager.Instance.lockMovement = false;
        }
        else
        {
            CSceneManager.Instance.lockMovement = true;
        }
    }

    public void ResetData(bool resetLife)
    {
        if(CSceneManager.Instance.roundOptions.resetPositions)
        {
            if(player_num == 1)
            {
                transform.position = new Vector3(-2, 0, 4.51f);
                transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
            }
            else
            {
                transform.position = new Vector3(2, 0, 4.51f);
                transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
            }

            moveSetScript.PlayBasicMove(moveSetScript.basicMoves.idle, moveSetScript.basicMoves.idle.name, 0, true);
            myPhysicsScript.ForceGrounded();

            myBodyCollisionInfo.gameObject.SetActive(true);
            myHitScript.gameObject.SetActive(true);
            myHurtScript.gameObject.SetActive(true);
        }
        else if(currentState == PlayerState.down && myPhysicsScript.IsGrounded())
        {
            moveSetScript.PlayAnimation("standUp", 0);
        }

        if(resetLife || CSceneManager.Instance.roundOptions.resetLifePoints)
        {
            if(player_num == 1 && CGameManager.gameMode == GameMode.TrainingRoom)
            {
                myCharacterInfo.currentLifePoints = (float)myCharacterInfo.maxLifePoints * (CSceneManager.Instance.trainingRoomOptions.p1StartingLife / 100);
            }
            else if(player_num == 2 && CGameManager.gameMode == GameMode.TrainingRoom)
            {

                myCharacterInfo.currentLifePoints = (float)myCharacterInfo.maxLifePoints * (CSceneManager.Instance.trainingRoomOptions.p2StartingLife / 100);
            }
            else
            {
                myCharacterInfo.currentLifePoints = (float)myCharacterInfo.maxLifePoints;
            }

            ResetDrainGauges();
        }
        
        stunTime = 0;
        comboHits = 0;
        airJuggleHits = 0;
        guardStunned = false;
        CheckGuarding(false, ButtonPress.none);
        hitedSkill = false;
        isDead = false;
        myPhysicsScript.isTakingOff = false;
        myPhysicsScript.isLanding = false;

        myPhysicsScript.OnResetApplyWeight();
        myCharacterInfo.currentStrongStunPoints = 0;
        gameManager.defaultUiCtrl.SetHP(player_num, myCharacterInfo.currentLifePoints, true);
        gameManager.defaultUiCtrl.OnStunImageReset(player_num);

        currentState = PlayerState.stand;
        currentSubState = PlayerSubState.resting;
    }

    private void OnStrongStunnedPlay(float yPos = 0)
    {
        if (moveSetScript.basicMoves.strongStunned.clip1 == null)
            Debug.LogError("character basic move 'strongStunned' animation clip no found! input clip data.");

        currentHitAnimation = GetHitAnimationName(moveSetScript.basicMoves.strongStunned, currentHit.attackLevel);

        currentSubState = PlayerSubState.stunned;
        myPhysicsScript.freeze = true;
        strongStunnedTime = CSceneManager.Instance.hurtOptions.strongStunnedMaxTime;
        storedMove = null;
        KillCurrentMove();
        gameManager.defaultUiCtrl.CharStateOption(3, player_num);
        moveSetScript.PlayBasicMove(moveSetScript.basicMoves.strongStunned, currentHitAnimation, currentHit.resetHitAnimations);
    }
}