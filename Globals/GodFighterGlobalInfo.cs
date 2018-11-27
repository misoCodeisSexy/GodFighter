using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GodFighter
{
    public enum GameMode
    {
        none,
        BattleRoom,
        TrainingRoom,
        EndGame
    }

    public enum LifeBarTrainingMode
    {
        refill,
        infinite,
        normal
    }

    public enum PlayerState
    {
        none,
        stand,
        crouch,
        down,
        neutralJump, //기본점프
        forwardJump,
        backJump
    }

    public enum PlayerSubState
    {
        resting,
        movingForward,
        movingBack,
        Guarding,
        stunned
    }

    public enum CharacterNames
    {
        agni,
        miho,
        valkiri,
        random
    }

    public enum StageNames
    {
        MihoBaseMap
    }

    public enum ButtonPress
    {
        none,
        Forward,
        Back,
        Up,
        Down,
        SmallPunch,
        BigPunch,
        SmallKick,
        BigKick
    }

    public enum InputType
    {
        none,
        HorizontalAxis,
        VerticalAxis,
        Button
    }

    public enum BodyPart
    {
        none,
        head,
        highTorso,
        lowTorso,

        leftShoulder,
        leftElbow,
        leftHand,
        leftPelvis, // 골반
        leftCalf, // 무릎
        leftFoot,

        rightShoulder,
        rightElbow,
        rightHand,
        rightPelvis,
        rightCalf,
        rightFoot,

        footSteps,

        root,

        custom1,
        custom2,
        custom3,
        custom4,
        custom5
    }

    public enum ColliderShape
    {
        sphere,
        rectangle,
        box
    }

    public enum ColliderType
    {
        bodyCollider,
        hitCollider, // 맞는거
        HurtCollider,
        throwCollider // 잡히는거
    }

    public enum HitType
    {
        none,
        Guard,
        Strike
    }

    public enum BasicMoveReference
    {
        idle,
        walkForward,
        walkBack,
        takeOff,
        jumpStraight,
        fallStraight,
        landing,
        crouching,
        blockingCrouchingPose,
        blockingCrouchingPoseHit,
        blockingStandingPose,
        blockingStandingPoseHit,
        blockingAirPose,
        getHitCrouchingMid,
        getHitCrouchingLow,
        getHitStandingHigh,
        getHitStandingMid,
        getHitStandingLow,
        getHitAir,
        getHitKnockBack,
        fallDownfromAir,
        stageGroundBounce,
        standUp,
        strongStunned,
        getHitStandingHigh2,
        getHitSkill2E0,
        getHitSkill2E1,
        getHitSkill2E2,
        getHitSkill2E3,
        getHitSkill2E4,
        getHitSkill2E5
    }

    /////////////////////////////////////

    public enum ExecutionBufferType
    {
        OnlyMoveLinks,
        AnyMove,
        NoBuffer
    }

    public enum Sizes
    {
        none,
        small,
        medium,
        high
    }

    public enum AirJuggleDeteriorationType
    {
        ComboHits,
        AirHits
    }
}
