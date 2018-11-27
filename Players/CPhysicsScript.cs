using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodFighter;

public class CPhysicsScript : MonoBehaviour
{
    public bool freeze;
    public float airTime = 0;
    public bool isGroundBouncing;
    public bool isWallBouncing;
    public bool isTakingOff = false;
    public bool isLanding;
    public bool isOverrideAirAnimation;
    public int currentAirJumps;

    private float moveDirection = 0;
    [SerializeField] private float verticalForce = 0;
    private float horizontalForce = 0;
    private float verticalTotalForce = 0;
    private float groundBounceTimes;

    private float appliedGravity;
    private float gravity_;
    private float airHitGravity;

    private int groundLayer;
    private int groundMask;

    [SerializeField]
    private CPlayerCtrl myControlsScript;

    public void Start()
    {
        myControlsScript = GetComponent<CPlayerCtrl>();

        if (myControlsScript.myCharacterInfo == null)
            myControlsScript.myCharacterInfo = GetComponent<CCharacterInfo>();

        appliedGravity = myControlsScript.myCharacterInfo.physics.weight * CSceneManager.Instance.gravity;

        groundLayer = LayerMask.NameToLayer("Floor");
        groundMask = 1 << groundLayer;

        horizontalForce = 0;

        gravity_ = 0.42f;
        airHitGravity = 0.37f;
    }

    public void Move(int mirror, float direction)
    {
        if (freeze) return;

        if ((transform.gameObject.name == "Player2" && myControlsScript.mirror == 1) ||
            transform.gameObject.name == "Player1" && myControlsScript.mirror == 1)
            direction *= -1;

        direction = direction < 0 ? -1 : 1;
        moveDirection = direction;

        if (mirror == 1)
        {
            myControlsScript.currentSubState = PlayerSubState.movingForward;
            myControlsScript.horizontalForce = horizontalForce = myControlsScript.myCharacterInfo.physics.moveForwardSpeed * direction;
        }
        else
        {
            myControlsScript.currentSubState = PlayerSubState.movingBack;
            myControlsScript.horizontalForce = horizontalForce = myControlsScript.myCharacterInfo.physics.moveBackSpeed * direction;
        }
    }

    public void AddForce(Vector2 push, float mirror)
    {
        push.x *= mirror;
        isGroundBouncing = false;

        if (!myControlsScript.myCharacterInfo.physics.cumulativeForce)
        {
            horizontalForce = 0;
            verticalForce = 0;
        }

        if (verticalForce < 0 && push.y > 0 && CSceneManager.Instance.comboOptions.resetFallForceOnHit)
            verticalForce = 0;

        horizontalForce += push.x;
        verticalForce += push.y;
        setVerticalData(verticalForce);
    }

    public void ApplyForces(MoveInfo move)
    {
        // pure freeze
        if (freeze && myControlsScript.strongStunnedTime < 0) return;

        float appliedFriction = (moveDirection != 0 || myControlsScript.myCharacterInfo.physics.highMovingFriction) ?
            CGameManager.selectStateOption.groundFriction : myControlsScript.myCharacterInfo.physics.friction;

        if (!IsGrounded())
        {
            appliedFriction = 0;
            if (verticalForce == 0) verticalForce = -.1f;
        }

        if (horizontalForce != 0 && !isTakingOff)
        {
            if (horizontalForce > 0)
            {
                horizontalForce -= appliedFriction * Time.fixedDeltaTime;
                horizontalForce = Mathf.Max(0, horizontalForce);
            }
            else if (horizontalForce < 0)
            {
                horizontalForce += appliedFriction * Time.fixedDeltaTime;
                horizontalForce = Mathf.Min(0, horizontalForce);
            }

            transform.Translate(0, 0, horizontalForce * Time.fixedDeltaTime);
        }

        if(move == null || (move != null && !move.ignoreGravity))
        {
            if((verticalForce < 0 && !IsGrounded()) || verticalForce > 0)
            {
                verticalForce -= appliedGravity * Time.fixedDeltaTime;  
                transform.Translate(moveDirection * myControlsScript.myCharacterInfo.physics.jumpDistance * Time.fixedDeltaTime
                                    ,verticalForce * Time.fixedDeltaTime
                                    , 0);
            }
            else if(verticalForce < 0 && IsGrounded() && myControlsScript.currentSubState != PlayerSubState.stunned)
            {
                verticalForce = 0;
            }
        }

        float minDist = myControlsScript.opPlayerScript.transform.position.x - myControlsScript.cameraScript.maxDistance;
        float maxDist = myControlsScript.opPlayerScript.transform.position.x + myControlsScript.cameraScript.maxDistance;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, minDist, maxDist),
                                                     transform.position.y,
                                                     transform.position.z);
        transform.position = new Vector3(Mathf.Clamp(transform.position.x,
                                               CGameManager.selectStateOption.leftBoundary,
                                               CGameManager.selectStateOption.rightBoundary)
                                               , transform.position.y
                                               , transform.position.z);

        // 2017.07.14 ~ add script kimmiso
        if (IsGrounded() && myControlsScript.currentState != PlayerState.down)
        {
            if (verticalForce != 0)
            {
                #region ground bouncing

                if(groundBounceTimes < CSceneManager.Instance.groundBounceOptions.maximumBounceForce
                    && myControlsScript.currentSubState == PlayerSubState.stunned
                    && CSceneManager.Instance.groundBounceOptions.bounceForce != Sizes.none
                    && verticalForce <= -CSceneManager.Instance.groundBounceOptions.minimumBounceForce
                    && myControlsScript.currentHit.groundBounce)
                {
                    switch(CSceneManager.Instance.groundBounceOptions.bounceForce)
                    {
                        case Sizes.small:
                            AddForce(new Vector2(0, -verticalForce / 1.7f), -1);
                            break;
                        case Sizes.medium:
                            AddForce(new Vector2(0, -verticalForce / 1.5f), -1); 
                            break;
                        case Sizes.high:
                            AddForce(new Vector2(0, -verticalForce / 1.2f), -1);
                            break;
                    }

                    groundBounceTimes++;

                    if(!isGroundBouncing)
                    {
                        myControlsScript.stunTime += airTime + CSceneManager.Instance.knockDownOptions.air.knockedOutTime;

                        if(myControlsScript.moveSetScript.basicMoves.fallingFromGroundBounce.clip1 != null)
                        {
                            myControlsScript.currentHitAnimation = myControlsScript.moveSetScript.basicMoves.fallingFromGroundBounce.name;
                            myControlsScript.moveSetScript.PlayBasicMove(myControlsScript.moveSetScript.basicMoves.fallingFromGroundBounce);
                        }
                        
                        if(CSceneManager.Instance.groundBounceOptions.bounceEffectPrefab != null)
                        {

                        }
                        // play sound

                        isGroundBouncing = true;
                    }
                    return;
                }

                #endregion

                verticalTotalForce = 0;
                airTime = 0;
                myControlsScript.moveSetScript.totalAirMoves = 0;
                currentAirJumps = 0;

                BasicMoveInfo airAnimation = null;
                string downAnimtion = "";

                isGroundBouncing = false;
                groundBounceTimes = 0;

                float animationSpeed = 0;
                float delayTime = 0;

                if (myControlsScript.currentMove != null && myControlsScript.currentMove.hitAnimationOverride)
                    return;

                if( myControlsScript.currentSubState == PlayerSubState.stunned)
                {
                    myControlsScript.stunTime = CSceneManager.Instance.knockDownOptions.air.knockedOutTime + CSceneManager.Instance.knockDownOptions.air.standUpTime;

                    if (myControlsScript.moveSetScript.basicMoves.fallDownAirHit.clip1 == null)
                        Debug.LogError("'fallDownAirHit' animation clip is not found! have to input it data on Player num -> model name -> MecanimScript -> 'fallDownAirHit'");

                    airAnimation = myControlsScript.moveSetScript.basicMoves.fallDownAirHit;
                    downAnimtion = myControlsScript.moveSetScript.GetAnimationString(airAnimation, 1);

                    myControlsScript.currentState = PlayerState.down;
                }
                else if(myControlsScript.currentState != PlayerState.stand)
                {
                    //  landing  //
                    if(myControlsScript.moveSetScript.basicMoves.landing.clip1 != null
                        && (myControlsScript.currentMove == null || 
                            (myControlsScript.currentMove != null && myControlsScript.currentMove.cancelMoveWheLanding)))
                    {
                        myControlsScript.isAirRecovering = false;
                        airAnimation = myControlsScript.moveSetScript.basicMoves.landing;
                        moveDirection = 0;
                        myControlsScript.currentSubState = PlayerSubState.resting;
                        isLanding = true;
                        myControlsScript.KillCurrentMove();
                        delayTime = (float)myControlsScript.myCharacterInfo.physics.landingDelay / CSceneManager.Instance.fps;
                        
                        //ResetLanding(); >>>> 코루틴으로 처리하겠음
                        StartCoroutine(CallResetLandingFunc(delayTime));
                        
                        if(airAnimation.isAutoSpeed)
                        {
                            animationSpeed = myControlsScript.moveSetScript.GetAnimationLength(airAnimation.name) / delayTime;
                        }
                    }

                    if (myControlsScript.currentState != PlayerState.crouch)
                        myControlsScript.currentState = PlayerState.stand;
                }

                if (airAnimation != null)
                {
                    if (downAnimtion != "")
                    {
                        myControlsScript.moveSetScript.PlayBasicMove(airAnimation, downAnimtion);
                    }
                    else
                    {
                        myControlsScript.moveSetScript.PlayBasicMove(airAnimation);
                    }

                    if(animationSpeed != 0)
                    {
                        myControlsScript.moveSetScript.SetAnimationSpeed(airAnimation.name, animationSpeed);
                    }
                }
            }

            #region walk / back-walk
            if (myControlsScript.currentSubState != PlayerSubState.stunned
                && !myControlsScript.isGuarding
                && !myControlsScript.guardStunned
                && move == null
                && !isTakingOff
                && !isLanding
                && myControlsScript.currentState == PlayerState.stand)
            {
                if(moveDirection > 0 && myControlsScript.mirror == -1 || // 1p
                   moveDirection > 0 && myControlsScript.mirror == 1) 
                {
                    if (myControlsScript.moveSetScript.basicMoves.walkForward.clip1 == null)
                    {
                        Debug.LogError("walkForward 애니메이션 클립 없음 넣으셈");
                    }

                    if (!myControlsScript.moveSetScript.IsAnimationPlaying(myControlsScript.moveSetScript.basicMoves.walkForward.name))
                    {
                        myControlsScript.moveSetScript.PlayBasicMove(myControlsScript.moveSetScript.basicMoves.walkForward);
                    }
                }
                else if(moveDirection < 0 && myControlsScript.mirror == 1 ||
                        moveDirection < 0 && myControlsScript.mirror == -1) // 1p
                {
                    if (myControlsScript.moveSetScript.basicMoves.walkBack.clip1 == null)
                    {
                        Debug.LogError("walkBack 애니메이션 클립 없음 넣으셈");
                    }

                    if (!myControlsScript.moveSetScript.IsAnimationPlaying(myControlsScript.moveSetScript.basicMoves.walkBack.name))
                    {
                        myControlsScript.moveSetScript.PlayBasicMove(myControlsScript.moveSetScript.basicMoves.walkBack);
                    }
                }
            }
            #endregion
        }
        else if (verticalForce > 0 || !IsGrounded())
        {
            //  jump straight, fall straight  //

            if (move != null && myControlsScript.currentState == PlayerState.stand)
                myControlsScript.currentState = PlayerState.neutralJump;

            if(move == null && verticalForce/verticalTotalForce > 0 && verticalForce/verticalTotalForce <= 1)
            {
                if (isGroundBouncing) return;

                if(moveDirection == 0)
                {
                    myControlsScript.currentState = PlayerState.neutralJump;
                }
                else
                {
                    if(moveDirection > 0)
                    {
                        myControlsScript.currentState = PlayerState.forwardJump;
                    }

                    if(moveDirection < 0)
                    {
                        myControlsScript.currentState = PlayerState.backJump;
                    }
                }

                BasicMoveInfo airAnimation = myControlsScript.moveSetScript.basicMoves.jumpStraight;
                if (myControlsScript.currentSubState == PlayerSubState.stunned)
                {
                    if(isWallBouncing && myControlsScript.moveSetScript.basicMoves.airWallBounce.clip1 != null)
                    {
                        airAnimation = myControlsScript.moveSetScript.basicMoves.airWallBounce;
                    }
                    else if(myControlsScript.moveSetScript.basicMoves.getHitKnockBack.clip1 != null
                            && Mathf.Abs(horizontalForce) > CSceneManager.Instance.comboOptions.knockBackMinForce
                            && CSceneManager.Instance.comboOptions.knockBackMinForce > 0)
                    {
                        airAnimation = myControlsScript.moveSetScript.basicMoves.getHitKnockBack;
                        airTime *= 2;
                    }
                    else
                    {
                        if(myControlsScript.moveSetScript.basicMoves.getHitAir.clip1 == null)
                            Debug.LogError("'getHitAir' animation clip not found. you have input animation at MoveSetScript -> BasicMoves -> 'getHitAir' clip1 ");

                        airAnimation = myControlsScript.moveSetScript.basicMoves.getHitAir;
                    }
                }
                else if( myControlsScript.isAirRecovering
                    && (myControlsScript.moveSetScript.basicMoves.airRecovery.clip1 != null))
                {
                    airAnimation = myControlsScript.moveSetScript.basicMoves.airRecovery;
                }

                if(!isOverrideAirAnimation && !myControlsScript.moveSetScript.IsAnimationPlaying(airAnimation.name))
                {
                    myControlsScript.moveSetScript.PlayBasicMove(airAnimation);

                    if(airAnimation.isAutoSpeed)
                    {
                        myControlsScript.moveSetScript.SetAnimationNomalizedSpeed(airAnimation.name, myControlsScript.moveSetScript.GetAnimationLength(airAnimation.name) / airTime);
                    }
                }

            }
            else if(move == null && (verticalForce / verticalTotalForce <= 0))
            {
                BasicMoveInfo airAnimation = myControlsScript.moveSetScript.basicMoves.fallStraight;
                if(isGroundBouncing && myControlsScript.moveSetScript.basicMoves.fallingFromGroundBounce.clip1 != null)
                {
                    airAnimation = myControlsScript.moveSetScript.basicMoves.fallingFromGroundBounce;
                }
                else if(isWallBouncing && myControlsScript.moveSetScript.basicMoves.airWallBounce.clip1 != null)
                {

                }
                else
                {
                    if(myControlsScript.currentSubState == PlayerSubState.stunned)
                    {
                        if(myControlsScript.moveSetScript.basicMoves.getHitKnockBack.clip1 != null
                            && Mathf.Abs(horizontalForce) > CSceneManager.Instance.comboOptions.knockBackMinForce
                            && CSceneManager.Instance.comboOptions.knockBackMinForce > 0)
                        {
                            airAnimation = myControlsScript.moveSetScript.basicMoves.getHitKnockBack;
                        }
                        else
                        {
                            airAnimation = myControlsScript.moveSetScript.basicMoves.getHitAir;
                            if(myControlsScript.moveSetScript.basicMoves.getHitAir.clip1 == null)
                                Debug.LogError("'getHitAir' animation clip not found. you have input animation at MoveSetScript -> BasicMoves -> F'getHitAir' clip1 ");
                        }
                    }
                    else if(myControlsScript.isAirRecovering && (myControlsScript.moveSetScript.basicMoves.airRecovery.clip1 != null))
                    {

                    }
                    else
                    {
                        // late add foward jump and back jump

                        if (myControlsScript.moveSetScript.basicMoves.fallStraight.clip1 == null)
                            Debug.LogError("'fallStraight' animation clip not found. you have input animation at MoveSetScript -> BasicMoves -> 'fallStraight' clip1 ");

                        airAnimation = myControlsScript.moveSetScript.basicMoves.fallStraight;
                    }
                }

                if (!isOverrideAirAnimation && !myControlsScript.moveSetScript.IsAnimationPlaying(airAnimation.name))
                {
                    myControlsScript.moveSetScript.PlayBasicMove(airAnimation);

                    if (airAnimation.isAutoSpeed)
                    {
                        myControlsScript.moveSetScript.SetAnimationNomalizedSpeed(airAnimation.name, (myControlsScript.moveSetScript.GetAnimationLength(airAnimation.name) / airTime));
                    }
                }
            }
        }
        if (horizontalForce == 0 && verticalForce == 0)
            moveDirection = 0;
    }

    public void Jump()
    {
        Jump(myControlsScript.myCharacterInfo.physics.jumpForce);
    }

    public void Jump(float jumpForce)
    {
        if (isTakingOff && currentAirJumps > 0) return;
        if (myControlsScript.currentMove != null) return;

        isTakingOff = false;
        isLanding = false;
        myControlsScript.storedMove = null;
        myControlsScript.potentialGuard = false;

        if (myControlsScript.currentState == PlayerState.down) return;
        if (myControlsScript.currentSubState == PlayerSubState.stunned || myControlsScript.currentSubState == PlayerSubState.Guarding) return;
        if (currentAirJumps >= myControlsScript.myCharacterInfo.physics.multiJumps) return;
        currentAirJumps++;

        if (moveDirection == 0 && myControlsScript.currentSubState != PlayerSubState.resting)
        {
            if (myControlsScript.currentSubState == PlayerSubState.movingBack)
                moveDirection = -1;
            else if (myControlsScript.currentSubState == PlayerSubState.movingForward)
                moveDirection = 1;
        }

        horizontalForce = myControlsScript.myCharacterInfo.physics.jumpDistance * moveDirection;

        verticalForce = jumpForce;
        setVerticalData(jumpForce);
        ApplyForces(myControlsScript.currentMove);
    }

    public bool IsGrounded()
    {
        if (Physics.RaycastAll(transform.position + new Vector3(0, 2f, 0), Vector3.down, 2.02f, groundMask).Length > 0)
        {
            if (transform.position.y != 0)
            {
                transform.Translate(new Vector3(0, -transform.position.y, 0));
            }
                
            return true;
        }
        return false;
    }

    void setVerticalData(float appliedForce)
    {
        float maxHeight = Mathf.Pow(appliedForce, 2) / (appliedGravity * 2);
        maxHeight += transform.position.y;
        airTime = Mathf.Sqrt(maxHeight * 2 / appliedGravity);
        verticalTotalForce = appliedGravity * airTime;
    }

    public float GetEnableAirTime(float appliedForce)
    {
        float maxHeight = Mathf.Pow(appliedForce, 2) / (appliedGravity * 2);
        maxHeight += transform.position.y;
        return Mathf.Sqrt(maxHeight * 2 / appliedGravity);
    }

    public bool IsJumping()
    {
        return (currentAirJumps > 0);
    }

    public bool IsMoving()
    {
        return (moveDirection != 0);
    }

    public void ResetForces(bool resetX, bool resetY)
    {
        if (resetX) horizontalForce = 0;
        if (resetY) verticalForce = 0;
    }

    public void ResetLanding()
    {
        isLanding = false;
    }

    IEnumerator CallResetLandingFunc(float delaytime)
    {
        yield return new WaitForSeconds(delaytime);

        ResetLanding();
    }

    public void ForceGrounded()
    {
        verticalForce = 0;
        horizontalForce = 0;
        setVerticalData(0);
        currentAirJumps = 0;
        isTakingOff = false;
        isLanding = false;
        if (transform.position.y != 0) transform.Translate(new Vector3(0, -transform.position.y, 0));
        myControlsScript.currentState = PlayerState.stand;
    }

    public void OnApplyNewWeight(float newWeight)
    {
        appliedGravity = newWeight * gravity_;
    }

    public void OnResetApplyWeight()
    {
        appliedGravity = myControlsScript.myCharacterInfo.physics.weight * airHitGravity;
    }
}

