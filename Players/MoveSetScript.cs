using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodFighter;

public class AxisSequenceRecord
{
    public ButtonPress axisPress;
    public float chargeTime;

    public AxisSequenceRecord(ButtonPress axisPress_, float chargeTime_)
    {
        axisPress = axisPress_;
        chargeTime = chargeTime_;
    }
}

[System.Serializable]
public struct OwnerPartPack
{
    public GameObject head;

    public GameObject leftHand;
    public GameObject leftFoot;

    public GameObject rightHand;
    public GameObject rightFoot;

    public GameObject footSteps;
    public GameObject body;
}

public class MoveSetScript : MonoBehaviour
{
    public BasicMoves basicMoves;
    public BasicMoveInfo[] ultimateHitMovesAgni;
    public BasicMoveInfo[] ultimateHitMovesMiho;
    public MoveInfo[] attackMoves;
    public MoveInfo[] moves;

    public MoveInfo intro;
    public MoveInfo outro;

    public AudioClip deathSound;

    public int titalAirMoves;
    public MecanimScript MecanimControl { get { return this.mecanimContrl; } }
    public CPlayerCtrl playerScript;
    public OwnerPartPack ownerParts;
    public int totalAirMoves;
    public bool animationPaused;
    public List<AxisSequenceRecord> lastAxisPresses = new List<AxisSequenceRecord>();

    private MecanimScript mecanimContrl;
    private float lastTimePress;

    private void Awake()
    {
        mecanimContrl = transform.gameObject.GetComponent<MecanimScript>();
    }

    private void Start()
    {
        mecanimContrl.SetMirror(playerScript.mirror > 0);
        mecanimContrl.currentAnimationData = null;
        PlayBasicMove(basicMoves.idle);
        OnAttackMovesFill();
    }

    private void OnAttackMovesFill()
    {
        mecanimContrl.overrideAnimationUpdate = true;

        moves = new MoveInfo[attackMoves.Length];
        for (int i = 0; i < moves.Length; i++)
        {
            moves[i] = (MoveInfo)Instantiate(attackMoves[i]);
        }
    }

    public void PlayBasicMove(BasicMoveInfo basicMove)
    {
        PlayBasicMove(basicMove, basicMove.name);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName)
    {
        PlayBasicMove(basicMove, basicMove.name, playerScript.myCharacterInfo.blendingTime, true);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, bool replay)
    {
        PlayBasicMove(basicMove, basicMove.name, replay);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, bool replay)
    {
        PlayBasicMove(basicMove, clipName, playerScript.myCharacterInfo.blendingTime, replay);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, float blendingTime, bool replay)
    {
        if (IsAnimationPlaying(clipName) && !replay)
        {
            return;
        }

        PlayAnimation(clipName, blendingTime);

        if(playerScript.strongStunnedTime > 0)
            PlayWithEffects(basicMove, true);
        else
            PlayWithEffects(basicMove, false);
    }

    private void PlayWithEffects(BasicMoveInfo basicMove, bool stun)
    {
        if(basicMove == null)
        {
            Debug.Log("@ miso @ MoveInfo is NULL");
        }

        if (CSceneManager.Instance.MyRandom(basicMove.soundPlayRandomValue))
            CGameManager.PlaySoundFX(basicMove.soundEffects);

        if (basicMove.hitEffect.prefab != null)
        {
            if (stun)
            {
                GameObject stunnedParticle = (GameObject)Instantiate(basicMove.hitEffect.prefab);
                stunnedParticle.transform.parent = GetOwnerPartObject(basicMove.hitEffect.bodyPart).transform;
                Vector3 newPos = new Vector3(-0.4f, 0, 0);
                newPos.z += basicMove.hitEffect.positionOffSet.z;
                stunnedParticle.transform.localPosition = newPos;
                stunnedParticle.transform.localRotation = Quaternion.Euler(new Vector3(0, -90, 0));
                stunnedParticle.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                Destroy(stunnedParticle, CSceneManager.Instance.hurtOptions.strongStunnedMaxTime);
            }
            else  
            {
                GameObject temp = (GameObject)Instantiate(basicMove.hitEffect.prefab);
                Vector3 newPos = playerScript.myHitScript.GetPosition(basicMove.hitEffect.bodyPart);
                newPos.x += basicMove.hitEffect.positionOffSet.x * playerScript.mirror;
                newPos.y += basicMove.hitEffect.positionOffSet.y;
                newPos.z += basicMove.hitEffect.positionOffSet.z;
                temp.transform.position = newPos;
            }
        }
    }

    public GameObject GetOwnerPartObject(BodyPart myPart)
    {
        if (myPart == BodyPart.head && ownerParts.head != null) return ownerParts.head;
        if (myPart == BodyPart.leftHand && ownerParts.leftHand != null) return ownerParts.leftHand;
        if (myPart == BodyPart.leftFoot && ownerParts.leftFoot != null) return ownerParts.leftFoot;
        if (myPart == BodyPart.rightHand && ownerParts.rightHand != null) return ownerParts.rightHand;
        if (myPart == BodyPart.rightFoot && ownerParts.rightFoot != null) return ownerParts.rightFoot;
        if (myPart == BodyPart.footSteps && ownerParts.footSteps != null) return ownerParts.footSteps;
        if (myPart == BodyPart.custom1 && ownerParts.body != null) return ownerParts.body;

        Debug.LogError("@ miso @ MoveInfo Particle Effects Effect Body Part is NULL !");
        return null;
    }

    public void PlayAnimation(string animationName, float blendingTime)
    {
        PlayAnimation(animationName, blendingTime, 0);
    }

    public void PlayAnimation(string animationName, float blendingTime, float nomalizedTime)
    {
        mecanimContrl.Play(animationName, blendingTime, nomalizedTime, (playerScript.mirror > 0));
    }

    public bool IsAnimationPlaying(string animationName)
    {
        return IsAnimationPlaying(animationName, 1);
    }

    public bool IsAnimationPlaying(string animationName, float weight)
    {
        return mecanimContrl.IsPlaying(animationName, weight);
    }

    public bool IsBasicMovePlaying(BasicMoveInfo basicMove)
    {
        if (basicMove.clip1 != null && IsAnimationPlaying(basicMove.name)) return true;
        if (basicMove.clip2 != null && IsAnimationPlaying(basicMove.name + "2")) return true;
        if (basicMove.clip3 != null && IsAnimationPlaying(basicMove.name + "3")) return true;

        return false;
    }

    public void RestoreAnimationSpeed()
    {
        if (playerScript.currentMove != null && IsAnimationPlaying(playerScript.currentMove.name))
        {
            playerScript.currentMove.currentFrame =
                (int)Mathf.Round(mecanimContrl.GetCurrentClipPosition() * (float)playerScript.currentMove.totalFrames);

            playerScript.currentMove.currentTick = playerScript.currentMove.currentFrame;
        }

        mecanimContrl.RestoreSpeed();
        animationPaused = false;
    }

    public float GetAnimationLength(string animName)
    {
        return mecanimContrl.GetAnimationData(animName).length;
    }

    public float GetAnimationSpeed()
    {
        return mecanimContrl.GetSpeed();
    }

    public void SetAnimationSpeed(string animName, float speed)
    {
        if (speed < 1)
            animationPaused = true;

        mecanimContrl.SetSpeed(animName, speed);
    }

    public void SetAnimationSpeed(float speed)
    {
        if (speed < 1)
            animationPaused = true;

        mecanimContrl.SetSpeed(speed);
    }

    public void SetAnimationNomalizedSpeed(string animName, float nomalizedSpeed)
    {
        mecanimContrl.SetNomalizedSpeed(animName, nomalizedSpeed);
    }

    public bool GetAnimationExist(string animName)
    {
        return (mecanimContrl.GetAnimationData(animName) != null);
    }

    public MoveInfo GetMove(ButtonPress[] axisPress, string[] buttonName, float charge, MoveInfo curMove, PlayerState playerState, int playerNumber, bool inputUp, bool forceExecution)
    {
        if (playerScript.isAirRecovering) return null;

        if (axisPress.Length > 0 && Time.time - lastTimePress <= playerScript.myCharacterInfo.executionTiming)
        {
            foreach (MoveInfo move in moves)
            {
                if (move == null) continue;
                if (!GaugeSkillEqual(move.gaugeSkillType)) continue;

                MoveInfo newMove = null;
                newMove = AdmitMove(axisPress, move, curMove, "", playerState, playerNumber, true);
                if (newMove != null)
                {
                    playerScript.UseGauge(newMove.gaugeSkillType);
                    return newMove;
                }
            }
        }

        if (axisPress.Length > 0)
        {
            if (Time.time - lastTimePress > playerScript.myCharacterInfo.executionTiming)
            {
                lastAxisPresses.Clear();
                lastTimePress = 0;
            }

            if (!forceExecution)
            {
                lastTimePress = Time.time;
                if (!inputUp || charge > playerScript.myCharacterInfo.executionTiming)
                {
                    lastAxisPresses.Add(new AxisSequenceRecord(axisPress[0], charge));
                }
            }
        }

        if (buttonName[0] != "")
        {
            foreach (MoveInfo move in moves)
            {
                MoveInfo newMove = null;
                newMove = AdmitMove(axisPress, move, curMove, buttonName[0], playerState, playerNumber, false);
                if (newMove != null)
                    return newMove;
            }
        }

        return null;
    }

    public MoveInfo AdmitMove(ButtonPress[] btnPress, MoveInfo move, MoveInfo curMove, string buttonName, PlayerState playerState, int playerNum, bool fromSequence)
    {
        string strPlayerNumber;

        if (playerNum == 1) strPlayerNumber = "P1" + move.InputName;
        else if (playerNum == 2) strPlayerNumber = "P2" + move.InputName;
        else return null;

        System.Array.Sort(btnPress);

        if (curMove == null)
        {
            if (move.axisPress != ButtonPress.none)
            {
                if ((playerState == PlayerState.neutralJump || playerState == PlayerState.forwardJump || playerState == PlayerState.backJump)) return null;

                if (fromSequence)
                {
                    if (move.axisSequence.Length == 0) return null;
                    #region charge move
                    if (move.chargeMove)
                    {
                        bool charged = false;
                        foreach (AxisSequenceRecord asr in lastAxisPresses)
                        {
                            if (asr.axisPress == move.axisSequence[0] && asr.chargeTime >= move.chargeTiming)
                            {
                                charged = true;
                            }
                        }

                        if (!charged) return null;
                    }
                    #endregion

                    List<ButtonPress> axisPressesList = new List<ButtonPress>();
                    foreach (AxisSequenceRecord asr in lastAxisPresses)
                    {
                        if (asr.chargeTime == 0)
                        {
                            axisPressesList.Add(asr.axisPress);
                        }
                    }

                    if (axisPressesList.Count >= move.axisSequence.Length)
                    {
                        ButtonPress[] compareSequence;
                        int compareRange = axisPressesList.Count - move.axisSequence.Length;
                        if (compareRange < 0) compareRange = 0;

                        // List.GetRange(idx, count) : idx부터 count까지의 단순 복사본을 만듭니다. (자료형 int)
                        compareSequence = axisPressesList.GetRange(compareRange, move.axisSequence.Length).ToArray();

                        if (!ArrayEqual<ButtonPress>(compareSequence, move.axisSequence))
                            return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (move.axisSequence.Length > 0) return null;
                }

                if (!ArrayEqual<ButtonPress>(btnPress, move.buttonExecution)) return null;
                if (playerScript.storedMove != null && move.moveName == playerScript.storedMove.moveName)
                    return playerScript.storedMove;

                return move;
            }
            else
            {
                if (strPlayerNumber != buttonName) return null;
                if ((playerState == PlayerState.neutralJump || playerState == PlayerState.forwardJump || playerState == PlayerState.backJump)
                    && move.attackStateType == PlayerState.neutralJump && totalAirMoves < playerScript.myCharacterInfo.possibleAirMoves)
                {
                    return move;
                }
                    
                if (move.attackStateType != playerState) return null;

                return move;
            }
        }
        return null;
    }

    private bool searchMove(string moveName)
    {
        foreach (MoveInfo move in attackMoves)
        {
            if (move == null) continue;
            if (moveName == move.moveName) return true;
        }

        return false;
    }

    public float GetAnimationNomalizedTime(float animFrame, MoveInfo move)
    {
        if (move == null) return 0;

        if (move.animationSpeed < 0)
        {
            return ((float)animFrame / (float)move.totalFrames) + 1;
        }
        else
        {
            return (float)animFrame / (float)move.totalFrames;
        }
    }

    public string GetAnimationString(BasicMoveInfo basicMove, int clipNum)
    {
        if (clipNum == 1) return basicMove.name;
        if (clipNum == 2 && basicMove.clip2 != null) return basicMove.name + "_2";
        if (clipNum == 3 && basicMove.clip3 != null) return basicMove.name + "_3";

        return basicMove.name;
    }

    private bool ArrayEqual<T>(T[] a1, T[] a2)
    {
        // ReferenceEquals : object 끼리 비교하여 동일한지 판단
        if (ReferenceEquals(a1, a2)) return true;
        if (a1 == null || a2 == null) return false;
        if (a1.Length != a2.Length) return false;

        // Default > 지정한 형식(T)의 기본 같음 비교연산자를 반환
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < a1.Length; i++) 
        {
            if (!comparer.Equals(a1[i], a2[i])) return false;
        }

        return true;
    }

    private bool GaugeSkillEqual(GaugeSkillType skillType)
    {
        if (skillType == GaugeSkillType.Guard && playerScript.myCharacterInfo.currentGuardGaugePoints < playerScript.myCharacterInfo.maxGuardGaugePoints)
            return false;
        if (skillType == GaugeSkillType.Skill1 && playerScript.myCharacterInfo.currentGaugePoints < (playerScript.myCharacterInfo.maxGaugePoints / 2))
            return false;
        if (skillType == GaugeSkillType.Skill2 && playerScript.myCharacterInfo.currentGaugePoints < playerScript.myCharacterInfo.maxGaugePoints)
            return false;

        return true;
    }
}
