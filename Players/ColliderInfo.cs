using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodFighter;

// 공격 > 타입
public enum HurtType
{
    none,
    launcher, //발사체
    pureNormalAttack,
    pureNormalAttackAir,
    skill,
    highKnockDown,
    midKnockDown
}

// 공격 > 약, 걍 구분
public enum AttackLevel
{
    none,
    week,
    mid,
    strong
}

public enum AttackRange
{
    none,
    top,
    mid,
    low
}

public enum CounterMoveType
{
    MoveFilter,
    SpecialMove
}

public enum HitEffectSpawnPoint
{
    StrikeHurtBox, // 때린 기준
    StrikeHitBox, // 맞은 기준
    InBetween
}

[System.Serializable]
public class AttackHitOptions
{
    public GameObject hitParticle;
    public AudioClip hitSound;
    public float killTime;
    public HitEffectSpawnPoint spawnPoint = HitEffectSpawnPoint.StrikeHitBox;
    public float freezingTime;
    public float animationSpeed = .1f;
    public bool shakeCharacterOnHit = true;
    public bool shakeCameraOnHit = true;
    public float shakeDestiny = .8f;
}

[System.Serializable]
public class HurtBoxes
{
    public string colliderStr;
    public bool casted;
}

public class ColliderInfo : MonoBehaviour
{
    public CPlayerCtrl myScript;

    [SerializeField] private float xPosCheckValue = -2.2f;
    public bool defaultVisible = true;
    public bool istrigger = true;
    public bool isDrawGizmo = false;
    public bool isYposUpper = false;
    public bool enableGuard = true;
    public bool resetHitAnimations = true;
    public bool isGuardableAreaEnter;
    public bool damageScaling;
    public bool doesntKill;

    public bool resetPreviousHorizontalPush = true;
    public bool resetPreviousVerticalPush = true;
    public bool applyDifferentAirForce;

    public bool overrideHitAcceleration = true;
    public bool overrideNormalHitAnimation;
    public bool overrideUltimateHitAnimation;
    public int  ultimateHitAnimationIdx;
    public bool overrideHitEffects;

    public float damage;
    public float damageOnGuard;
    public float camShakeTime;
    public float offset;
    public float upeerYpos;
    public float bodyRadius;
    public int hitStunOnHit = 20;
    public float hitStunWhenBlock;

    public float gainGaugeOnHit;
    public float opGainGaugeOnHit;
    public float gainGaugeOnGuard;
    public float opGainStrongStun;

    public Rect rect;
    public Vector3 Center;
    public Transform position;

    public BodyPart bodypart;
    public ColliderShape sphere;
    public ColliderType colliderType;
    public SphereCollider[] hitBoxes;
    [HideInInspector] public HealingDatas healData;

    #region hurt box datas
    public HurtType hurtType;
    public AttackLevel attackLevel;
    public AttackRange attackRange;
    public HurtOptionDatas hurtEffects;
    
    public bool groundBounce = true;
    public bool cornerPush = true;
    public bool isContinueHit;
    public bool curHitEnabled { get; set; }

    public bool resetPreviousHorizontal = true;
    public bool resetPreviousVertical = true;

    public string hurtTypeName;
    public Vector2 appliedForce;
    public Vector2 pushForce;
    public Vector2 pushForceAir;
    public AttackHitOptions hitEffects;
    public int activeFrameBegin;
    public int activeFrameEnd;
    public HurtBoxes[] hurtBoxes;
    #endregion

    public bool disabled { get; set; }

    public void Start()
    {
        if (colliderType == ColliderType.bodyCollider)
        {
            isDrawGizmo = true;
            return;
        }
        else if(colliderType == ColliderType.hitCollider)
        {
            hitBoxes = new SphereCollider[1];
            hitBoxes[0] = transform.GetComponent<SphereCollider>();
            hitBoxes[0].radius = this.bodyRadius;
        }
    }

    public void FiexdUpdateGOF()
    {
        if (isYposUpper)
        {
            transform.transform.position = new Vector3(position.position.x, position.position.y + upeerYpos, position.position.z);
        }
        else
        {
            transform.transform.position = new Vector3(position.position.x, position.position.y, position.position.z);
        }

        transform.Translate(Vector3.right * offset);

        if (transform.localPosition.x > xPosCheckValue)
            transform.localPosition = new Vector3(xPosCheckValue, transform.localPosition.y, transform.localPosition.z);
    }

    public void OnDrawGizmos()
    {
        if ( isDrawGizmo )
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, bodyRadius);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (colliderType == ColliderType.hitCollider && myScript != null && !myScript.myHitScript.isHit)
        {
            if (other.tag == "HurtBox")
            {
                ColliderInfo opHurtInfo =
                    other.transform.GetComponent<ColliderInfo>();

                if (opHurtInfo == null)
                {
                    Debug.LogError("trigger collider not has ColiderInfo script");
                    return;
                }

                if (myScript.mirror == opHurtInfo.myScript.mirror) return;

                if (myScript.currentSubState != PlayerSubState.stunned
                    && myScript.currentMove == null
                    && myScript.potentialGuard
                    && myScript.CheckGuardRange(opHurtInfo.attackRange, opHurtInfo.hurtType)
                    && opHurtInfo.enableGuard) /// 가드 피격
                {
                    myScript.GetHitGuarding(opHurtInfo, transform.position);
                    return;
                }
                else if (!myScript.isGuarding) /// 기본 피격
                {
                    myScript.GetHit(opHurtInfo, transform.position);

                    if (hurtTypeName == null)
                        Debug.LogError("hurtTypeName is null!! input attack name..");
                    else
                        myScript.prevAttackingName = opHurtInfo.hurtTypeName;
                }
            }
            else if (other.tag == "GuardArea") /// 가드 자세 취하기
            {
                if (myScript.potentialGuard)
                    isGuardableAreaEnter = true;
            }
        }
    }
}
