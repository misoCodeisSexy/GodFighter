using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;
using GodFighter;


public enum LinkType
{
    HitConfirm,
    CounterMove, // 맞았을 때 카운터 모션으로 넘어가는지
    NoConditions
}

public enum GaugeSkillType
{
    none,
    Guard,
    Skill1,
    Skill2
}

public enum CounterFrameData
{
    none,
    Attack,
    Dash
}

// 애니메이션 도중 다른 애니메이션으로 링크하기위한 조건 클래스
[System.Serializable]
public class MoveLink
{
    public LinkType linkType = LinkType.NoConditions;
    public bool onStrike = true; //부딪혔을 경우
    public bool onGuard = true;
    public int activeFramesBegins; // 캔슬 가능한 프레임 위치
    public int activeFramesEnds;    // 캔슬 불가능한 프레임 위치

    public bool cancelable { get; set; }
}

[System.Serializable]
public class ApplyForce
{
    public int castingFrame;
    public bool resetPreviousVertical = true;
    public bool resetPreviousHorizontal = true;
    public Vector2 force;

    public bool casted { get; set; }
}

[System.Serializable]
public class CameraMovement
{
    public bool inSkillUseCineCamMove;
    public Vector3 position;
    public Vector3 rotation;
    public float fieldOfView;
    public float camSpeed = 2;
    public float duration;
    public int castingFrame;
    public float myAnimationSpeed = 100;
    public float opAnimationSpeed = 100;
    public bool freezePhysics;

    public bool casted { get; set; }
    public bool over { get; set; }
    public float time { get; set; }
}

[System.Serializable]
public class SoundEffects
{
    public int castingFrame;
    public float playRandomValue;
    public AudioClip sound;

    public bool casted { get; set; }
}

[System.Serializable]
public class MoveParticleEffect
{
    public int castingFrame;
    public ParticleInfo particleEffect;
    public float duration;

    public bool isAttachMesh;
    public bool casted { get; set; }
}

[System.Serializable]
public class HealingDatas
{
    public bool enableHealing;
    public float healValue;
}

[System.Serializable]
public class DistanceParticleEffect
{
    public int castingFrame;
    public int castingEndFrame;
    public bool isMine;
    public float duration;
    public GameObject prefab;
    [HideInInspector] public GameObject distroyClone;

    public Vector3 positionOffSet;
    public HealingDatas healingData;

    public bool casted { get; set; }
}

[System.Serializable]
public class Projectile
{
    public int castingFrame = 1;
    public GameObject projectilePrefab;

    public BodyPart bodyPart;
    public Vector3 castingOffSet;
    public int speed = 20;
    public float duration;

    public bool casted { get; set; }
}

[CreateAssetMenu(menuName = "Character Move/Character Move Create")]
public class MoveInfo : ScriptableObject
{
    public string moveName;
    public string InputName;
    public ButtonPress axisPress;
    public string description;
    public float fps = 60;

    public bool ignoreGravity;
    public bool invertRotationLeft;
    public bool invertRotationRight;
    public int totalFrames = 15;
    public bool overrideBlendingIn = true;
    public bool overrideBlendingOut = false;
    public float blendingIn = 0;
    public bool hasGuardableArea = true;
    public bool chargeMove;
    public float chargeTiming;

    public bool voiceRandom;
    [HideInInspector] public int voiceIdx = -1;

    public AnimationClip animationClip;
    public WrapMode wrapMode;
    public InputType inputType;
    public GaugeSkillType gaugeSkillType;
    public ButtonPress attackButtonPress;
    public PlayerState attackStateType;
    public CounterFrameData counterFrameData;

    public bool fixedSpeed = true;
    public bool cancelMoveWheLanding;
    public float animationSpeed = 1;

    public SoundEffects[] soundEffects = new SoundEffects[0];
    public MoveParticleEffect[] particleEffects = new MoveParticleEffect[0];

    public bool hitAnimationOverride { get; set; }
    public int currentFrame { get; set; }
    public float currentTick { get; set; }
    public bool cancelable { get; set; }
    public int overrideStartUpFrame { get; set; }
    public float animationSpeedTemp { get; set; }
    public bool hitConfirmOnStrike { get; set; }
    public bool hitConfirmOnGuard { get; set; }

    public ButtonPress[] axisSequence = new ButtonPress[0];
    public ButtonPress[] buttonExecution = new ButtonPress[0];

    public MoveLink[] frameLinks = new MoveLink[0];
    public ApplyForce[] applyForces = new ApplyForce[0];
    public CameraMovement[] cameraMovements = new CameraMovement[0];
    public ColliderInfo[] hurtInfo = new ColliderInfo[0];
    public Projectile[] projectile = new Projectile[0];
    public DistanceParticleEffect[] distanceEffects = new DistanceParticleEffect[0];
}
