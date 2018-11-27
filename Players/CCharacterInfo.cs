using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GodFighter;


public class CCharacterInfo : MonoBehaviour
{
    public PhysicsData physics;

    public float executionTiming = .3f; 
    public int possibleAirMoves = 1; 
    public float blendingTime = .05f; 

    public float maxLifePoints = 1000;
    public float maxGaugePoints = 100;
    public float maxGuardGaugePoints = 100;
    public float maxStrongStunPoints = 100;
    public float currentLifePoints { get; set; }
    public float currentGaugePoints { get; set; }
    public float currentGuardGaugePoints { get; set; }
    public float currentStrongStunPoints { get; set; }
}

[System.Serializable]
public class PhysicsData
{
    public float moveForwardSpeed = 4f; 
    public float moveBackSpeed = 3.5f; 
    public bool highMovingFriction = true; 
    public float friction = 30f; 

    public bool canCrouch = true;
    public int crouchDelay = 2;
    public int standingDelay = 2;

    public bool canJump = true;
    public float jumpForce = 15f; 
    public float minJumpForce = 20f;
    public int minJumpDelay = 4;
    public float jumpDistance = 5f; 
    public bool cumulativeForce = true; 
    public int multiJumps = 1; 
    public float weight = 120;
    public int jumpDelay = 3;
    public int landingDelay = 7;
    public float groundCollisionMass = 2;
}

[System.Serializable]
public class ParticleInfo
{
    public GameObject prefab;
    public BodyPart bodyPart;
    public Vector3 positionOffSet;
}

#region //  script add 2017.07.11 kimmiso  //

[System.Serializable]
public class BasicMoveInfo
{
    public AnimationClip clip1;
    public AnimationClip clip2;
    public AnimationClip clip3;

    public float soundPlayRandomValue;
    public AudioClip soundEffects;
    public ParticleInfo hitEffect;

    public WrapMode wrapMode;
    public float animationSpeed = 1;
    public float restingClipInterval = 6f;
    public bool isAutoSpeed = true;
    public bool continueSound;
    public bool downClip;

    public string name;
    public BasicMoveReference reference;
}

[System.Serializable]
public class BasicMoves
{
    public BasicMoveInfo idle = new BasicMoveInfo();
    public BasicMoveInfo walkForward = new BasicMoveInfo();
    public BasicMoveInfo walkBack = new BasicMoveInfo();
    public BasicMoveInfo takeOff = new BasicMoveInfo();
    public BasicMoveInfo jumpStraight = new BasicMoveInfo();
    public BasicMoveInfo fallStraight = new BasicMoveInfo();
    public BasicMoveInfo landing = new BasicMoveInfo();
    public BasicMoveInfo crouching = new BasicMoveInfo();
    public BasicMoveInfo blockingCrouchingPose = new BasicMoveInfo(); // 앉아서 가드
    public BasicMoveInfo blockingCrouchingPoseHit = new BasicMoveInfo();
    public BasicMoveInfo blockingStandingPose = new BasicMoveInfo(); // 서서 가드
    public BasicMoveInfo blockingStandingPoseHit = new BasicMoveInfo();
    public BasicMoveInfo getHitCrouchingMid = new BasicMoveInfo();
    public BasicMoveInfo getHitCrouchingLow = new BasicMoveInfo();
    public BasicMoveInfo getHitStandingHigh = new BasicMoveInfo();
    public BasicMoveInfo getHitStandingHighSkill1 = new BasicMoveInfo();
    public BasicMoveInfo getHitStandingMid = new BasicMoveInfo();
    public BasicMoveInfo getHitStandingLow = new BasicMoveInfo();
    public BasicMoveInfo getHitAir = new BasicMoveInfo();
    public BasicMoveInfo getHitKnockBack = new BasicMoveInfo();
    public BasicMoveInfo fallDownAirHit = new BasicMoveInfo();
    public BasicMoveInfo standUp = new BasicMoveInfo();
    public BasicMoveInfo airRecovery = new BasicMoveInfo();
    public BasicMoveInfo fallingFromGroundBounce = new BasicMoveInfo();
    public BasicMoveInfo airWallBounce = new BasicMoveInfo();
    public BasicMoveInfo strongStunned = new BasicMoveInfo();

    public bool moveEnable = true;
    public bool jumpEnable = true;
    public bool crouchEnable = true;
    public bool blockEnable = true;
}

#endregion