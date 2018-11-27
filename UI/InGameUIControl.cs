using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GodFighter;
using DG.Tweening;

[System.Serializable]
public class CharacterImageDatas
{
    public Sprite agniChrSprite;
    public Sprite mihoChrSprite;
    public Sprite valkiriChrSprite;
    public Image player1ChrImg;
    public Image player2ChrImg;

    public Sprite agniNameSprite;
    public Sprite mihoNameSprite;
    public Sprite valkiriNameSprite;
    public Image player1NameImg;
    public Image player2NameImg;
}

[System.Serializable]
public class GaugeImageDatas
{
    // skill gauge 
    public RectTransform RectSkillGageP1;
    public RectTransform RectSkillGageP2;
    public Image ImgSkillFullP1;
    public Image ImgSkillFullP2;
    public float skillGaugeBarValueP1 = -85.2f;
    public float skillGaugeBarValueP2 = -85.2f;
    public float preSkillGaugeBarValueP1 = -85.2f;
    public float preSkillGaugeBarValueP2 = -85.2f;
    public float skillAlphaP1 = 0.0f;
    public float skillAlphaP2 = 0.0f;
    public bool  isSkillAlphaP1;
    public bool  isSkillAlphaP2;
    public bool  skillGaugeFullP1;
    public bool  skillGuageHalfP1;
    public bool  skillGaugeFullP2;
    public bool  skillGuageHalfP2;

    // guard gauge
    public Image ImgGuardGageP1;
    public Image ImgGuardGageP2;
    public Image ImgGuardFullP1;
    public Image ImgGuardFullP2;
    public float guardGaugeBarValueP1;
    public float guardGaugeBarValueP2;
    public float preGuardGaugeBarValueP1;
    public float preGuardGaugeBarValueP2;
    public float guardAlphaP1 = 0.0f;
    public float guardAlphaP2 = 0.0f;
    public bool  guardGaugeFullP1;
    public bool  guardGaugeFullP2;
    public bool  guardGaugeUseP1;
    public bool  guardGaugeUseP2;
    public bool  isGuardAlphaP1;
    public bool  isGuardAlphaP2;

    // stun gauge
    public Image ImgStunP1;
    public Image ImgStunP2;
    public Image ImgStunFullP1;
    public Image ImgStunFullP2;
    public float stunValueP1 = 0.0f;
    public float stunValueP2 = 0.0f;
    public float preStunValueP1 = 0.0f;
    public float preStunValueP2 = 0.0f;
    public float stunAlphaValueP1 = 0.0f;
    public float stunAlphaValueP2 = 0.0f;
    public bool  stunGaugeFullP1;
    public bool  stunGaugeFullP2;
    public bool  stunIncreaseP1;
    public bool  stunDecreaseP1;
    public bool  stunIncreaseP2;
    public bool  stunDecreaseP2;
    public bool  isStunAlphaMaxP1;
    public bool  isStunAlphaMaxP2;
}

[System.Serializable]
public class SkillDatas
{
    // half gauge skill prefabs
    public GameObject HalfGaugeSkillAgni;
    public GameObject HalfGaugeSkillMiho;
    public GameObject HalfGaugeSkilValkiri;
    public bool enableHalfSkillP1;
    public bool enableHalfSkillP2;

    // ultimate skill booleans
    public GameObject agniSpine;
    public GameObject mihoSpine;
    public GameObject valkiriSpine;
    public bool enableUltimateSkillP1 = false;
    public bool enableUltimateSkillP2 = false;
}

[System.Serializable]
public class HpImageDatas
{
    public Image ImgHealthBarP1;   
    public Image ImgHealthBarP2;
    public RectTransform shakePositionP1;   
    public RectTransform shakePositionP2;  
    public Sprite healthPointYellow;
    public Sprite healthPointRed;

    public float maxHpValueP1;               
    public float maxHpValueP2;
    public float currentHpValueP1 = 1.0f;
    public float currentHpValueP2 = 1.0f;
    public float preHPValueP1 = 1.0f;
    public float preHPValueP2 = 1.0f;
    public bool  hpIncreaseP1 = false;
    public bool  hpIncreaseP2 = false;
    public bool  hpDecreaseP1 = false;
    public bool  hpDecreaseP2 = false;
}

[System.Serializable]
public class StateOptionDatas
{
    public Sprite[] ComboSpriteNumbers;

    // round
    public GameObject Fight;
    public GameObject Round1;
    public GameObject Round2;
    public GameObject Round3;
    public GameObject Perfact;
    public GameObject KO;
    public GameObject TimeOver;

    public GameObject mihoWinP1;
    public GameObject mihoWinP2;
    public GameObject agniWinP1;
    public GameObject agniWinP2;
    public GameObject valkiriWinP1;
    public GameObject valkiriWinP2;

    // p1 state
    public GameObject   stateLineP1;
    public GameObject   FirstHitP1;
    public GameObject   aerialP1;
    public GameObject   counterP1;
    public GameObject   stunP1;
    public Image        ComboP1;
    public Image        ComboNumberP1;
    public Image        ComboNumber10P1;
    public GameObject[] WinPointsP1;

    // p2 state
    public GameObject   stateLineP2;
    public GameObject   FirstHitP2;
    public GameObject   aerialP2;
    public GameObject   counterP2;
    public GameObject   stunP2;
    public Image        ComboP2;
    public Image        ComboNumberP2;
    public Image        ComboNumber10P2;
    public GameObject[] WinPointsP2;
}

public class InGameUIControl : DefaultUIControl
{
    [SerializeField] private CharacterImageDatas characterImgData;
    [SerializeField] private StateOptionDatas roundStateData;
    [SerializeField] private HpImageDatas hpImgData;
    [SerializeField] private GaugeImageDatas gaugeImgData;
    [SerializeField] private SkillDatas skillData;

    [SerializeField] private TimeControlScript timeController;

    public override void OnStart()
    {
        base.OnStart();
        OnRoundInit();
        if (timeController != null)     // timer init
        {
            timeController.OnStart();
            timeController.Test_Time();
        }
    }

    public override void FixedUpdateGOF()
    {
        base.FixedUpdateGOF();

        // effects
        OnStunEffectMove();
        OnSkillEffectMove();
        OnGuardEffectMove();
        OnHpEffectMove();

        // time

        timeController.FixedUpdateGOF();
    }

    #region -- stun --
    private void OnStunEffectMove()
    {
        // increase
        if (gaugeImgData.stunIncreaseP1)
        {
            gaugeImgData.preStunValueP1 += Time.deltaTime * 0.3f;

            if (gaugeImgData.preStunValueP1 >= gaugeImgData.stunValueP1)
            {
                gaugeImgData.preStunValueP1 = gaugeImgData.stunValueP1;
                gaugeImgData.stunIncreaseP1 = false;
            }

            gaugeImgData.ImgStunP1.fillAmount = gaugeImgData.preStunValueP1;
        }
        else if (gaugeImgData.stunIncreaseP2)
        {
            gaugeImgData.preStunValueP2 += Time.deltaTime * 0.3f;

            if (gaugeImgData.preStunValueP2 >= gaugeImgData.stunValueP2)
            {
                gaugeImgData.preStunValueP2 = gaugeImgData.stunValueP2;
                gaugeImgData.stunIncreaseP2 = false;
            }

            gaugeImgData.ImgStunP2.fillAmount = gaugeImgData.preStunValueP2;
        }

        // decrease
        if (gaugeImgData.stunDecreaseP1)
        {
            gaugeImgData.preStunValueP1 -= Time.deltaTime * 0.05f;

            if (gaugeImgData.preStunValueP1 <= gaugeImgData.stunValueP1)
            {
                gaugeImgData.preStunValueP1 = gaugeImgData.stunValueP1;
                gaugeImgData.stunDecreaseP1 = false;
            }

            gaugeImgData.ImgStunP1.fillAmount = gaugeImgData.preStunValueP1;
        }
        else if (gaugeImgData.stunDecreaseP2)
        {
            gaugeImgData.preStunValueP2 -= Time.deltaTime * 0.05f;

            if (gaugeImgData.preStunValueP2 <= gaugeImgData.stunValueP2)
            {
                gaugeImgData.preStunValueP2 = gaugeImgData.stunValueP2;
                gaugeImgData.stunDecreaseP2 = false;
            }

            gaugeImgData.ImgStunP2.fillAmount = gaugeImgData.preStunValueP2;
        }

        // full stun image fade
        if(gaugeImgData.stunGaugeFullP1)
        {
            if (gaugeImgData.isStunAlphaMaxP1)
            {
                gaugeImgData.stunAlphaValueP1 -= Time.deltaTime;
                if (gaugeImgData.stunAlphaValueP1 <= 0.0f)
                    gaugeImgData.isStunAlphaMaxP1 = false;
            }
            else
            {
                gaugeImgData.stunAlphaValueP1 += Time.deltaTime;
                if (gaugeImgData.stunAlphaValueP1 >= 1.0f)
                    gaugeImgData.isStunAlphaMaxP1 = true;
            }
            gaugeImgData.ImgStunFullP1.color = new Color(gaugeImgData.ImgStunFullP1.color.r,
                                                                gaugeImgData.ImgStunFullP1.color.g,
                                                                gaugeImgData.ImgStunFullP1.color.b,
                                                                gaugeImgData.stunAlphaValueP1);
        }
        else if(gaugeImgData.stunGaugeFullP2)
        {
            if (gaugeImgData.isStunAlphaMaxP2)
            {
                gaugeImgData.stunAlphaValueP2 -= Time.deltaTime * 3.0f;
                if (gaugeImgData.stunAlphaValueP2 <= 0.0f)
                    gaugeImgData.isStunAlphaMaxP2 = false;
            }
            else
            {
                gaugeImgData.stunAlphaValueP2 += Time.deltaTime * 3.0f;
                if (gaugeImgData.stunAlphaValueP2 >= 1.0f)
                    gaugeImgData.isStunAlphaMaxP2 = true;
            }
            gaugeImgData.ImgStunFullP2.color = new Color(gaugeImgData.ImgStunFullP2.color.r,
                                                               gaugeImgData.ImgStunFullP2.color.g,
                                                               gaugeImgData.ImgStunFullP2.color.b,
                                                               gaugeImgData.stunAlphaValueP2);
        }
    }

    public override void OnStunImageReset(int playerNum)
    {
        if (playerNum == 1)
        {
            gaugeImgData.stunValueP1 = 0.0f;
            gaugeImgData.preStunValueP1 = 0.0f;
            gaugeImgData.ImgStunP1.fillAmount = gaugeImgData.stunValueP1;
            gaugeImgData.stunGaugeFullP1 = false;
            gaugeImgData.stunAlphaValueP1 = 0.0f;
            gaugeImgData.ImgStunFullP1.color = new Color(gaugeImgData.ImgStunFullP1.color.r,
                                                        gaugeImgData.ImgStunFullP1.color.g,
                                                        gaugeImgData.ImgStunFullP1.color.b,
                                                        gaugeImgData.stunAlphaValueP1);
        }
        else if (playerNum == 2)
        {
            gaugeImgData.stunValueP2 = 0.0f;
            gaugeImgData.preStunValueP2 = 0.0f;
            gaugeImgData.ImgStunP2.fillAmount = gaugeImgData.stunValueP2;
            gaugeImgData.stunGaugeFullP2 = false;
            gaugeImgData.stunAlphaValueP2 = 0.0f;
            gaugeImgData.ImgStunFullP2.color = new Color(gaugeImgData.ImgStunFullP2.color.r,
                                                         gaugeImgData.ImgStunFullP2.color.g,
                                                         gaugeImgData.ImgStunFullP2.color.b,
                                                         gaugeImgData.stunAlphaValueP2);
        }
    }

    public override void StunGageCtrl(int player, float gauge)
    {
        gauge = (gauge * 0.01f) * 1.0f;

        if (player == 1)
        {
            gaugeImgData.stunValueP1 = gauge;

            if (gaugeImgData.stunValueP1 >= 1.0f)
            {
                gaugeImgData.stunValueP1 = 1.0f;
                gaugeImgData.stunGaugeFullP1 = true;
            }
            else
            {
                gaugeImgData.stunGaugeFullP1 = false;
                gaugeImgData.stunAlphaValueP1 = 0.0f;
                gaugeImgData.ImgStunFullP1.color = new Color(gaugeImgData.ImgStunFullP1.color.r,
                                                             gaugeImgData.ImgStunFullP1.color.g,
                                                             gaugeImgData.ImgStunFullP1.color.b,
                                                             gaugeImgData.stunAlphaValueP1);
            }

            if (gaugeImgData.stunValueP1 > gaugeImgData.preStunValueP1)
                gaugeImgData.stunIncreaseP1 = true;
            if (gaugeImgData.stunValueP1 < gaugeImgData.preStunValueP1)
                gaugeImgData.stunDecreaseP1 = true;

            return;
        }

        if (player == 2)
        {
            gaugeImgData.stunValueP2 = gauge;

            if (gaugeImgData.stunValueP2 >= 1.0f)
            {
                gaugeImgData.stunGaugeFullP2 = true;
                gaugeImgData.stunValueP2 = 1.0f;
            }
            else
            {
                gaugeImgData.stunGaugeFullP2 = false;
                gaugeImgData.stunAlphaValueP2 = 0.0f;
                gaugeImgData.ImgStunFullP2.color = new Color(gaugeImgData.ImgStunFullP2.color.r,
                                                             gaugeImgData.ImgStunFullP2.color.g,
                                                             gaugeImgData.ImgStunFullP2.color.b,
                                                             gaugeImgData.stunAlphaValueP2);
            }

            if (gaugeImgData.stunValueP2 > gaugeImgData.preStunValueP2)
                gaugeImgData.stunIncreaseP2 = true;
            if (gaugeImgData.stunValueP2 < gaugeImgData.preStunValueP2)
                gaugeImgData.stunDecreaseP2 = true;
        }
    }

    #endregion

    #region -- skill --
    public override void IncreaseSkillGaugeBar(int Player, float damage)
    {
        damage = (damage * 0.01f) * 80.3f;

        if (Player == 1 && damage != 0)
        {
            gaugeImgData.skillGaugeBarValueP1 += damage;

            if (gaugeImgData.skillGaugeBarValueP1 >= 1.1f)
            {
                gaugeImgData.skillGaugeBarValueP1 = 1.1f;
                gaugeImgData.skillGaugeFullP1 = true;
            }
            else if (gaugeImgData.skillGaugeBarValueP1 >= -46.0f)
            {
                gaugeImgData.skillGuageHalfP1 = true;
            }
            return;
        }

        if (Player == 2 && damage != 0)
        {
            gaugeImgData.skillGaugeBarValueP2 += damage;

            if (gaugeImgData.skillGaugeBarValueP2 >= 1.1f)
            {
                gaugeImgData.skillGaugeBarValueP2 = 1.1f;
                gaugeImgData.skillGaugeFullP2 = true;
            }
            else if (gaugeImgData.skillGaugeBarValueP2 >= -46.0f)
            {
                gaugeImgData.skillGuageHalfP2 = true;
            }
        }
    }

    //skill=0 : 궁극기
    public override void DecreaseSkillGaugeBar(int Player, GaugeSkillType skill)
    {
        if (Player == 1)
        {
            if (skill == GaugeSkillType.Skill2 && gaugeImgData.skillGaugeFullP1)
            {
                gaugeImgData.skillGaugeBarValueP1 = -79.2f;
                
                gaugeImgData.skillGaugeFullP1 = false;
                skillData.enableHalfSkillP1 = false;
                skillData.enableUltimateSkillP1 = true;

                gaugeImgData.skillAlphaP1 = 0.0f;
                gaugeImgData.ImgSkillFullP1.color = new Color(gaugeImgData.ImgSkillFullP1.color.r, gaugeImgData.ImgSkillFullP1.color.g,
                                                              gaugeImgData.ImgSkillFullP1.color.b, gaugeImgData.skillAlphaP1);
            }
            else if (skill == GaugeSkillType.Skill1 && gaugeImgData.skillGuageHalfP1)
            {
                gaugeImgData.skillGaugeBarValueP1 = gaugeImgData.skillGaugeBarValueP1 - 40.15f;

                if (gaugeImgData.skillGaugeFullP1)
                {
                    gaugeImgData.skillGaugeBarValueP1 = -46.0f;

                    gaugeImgData.skillAlphaP1 = 0.0f;
                    gaugeImgData.ImgSkillFullP1.color = new Color(gaugeImgData.ImgSkillFullP1.color.r, gaugeImgData.ImgSkillFullP1.color.g,
                                                                  gaugeImgData.ImgSkillFullP1.color.b, gaugeImgData.skillAlphaP1);
                }
                else
                {
                    gaugeImgData.skillGuageHalfP1 = false;
                }
                
                gaugeImgData.skillGaugeFullP1 = false;
                skillData.enableHalfSkillP1 = true;
                skillData.enableUltimateSkillP1 = false;

            }
            return;
        }

        if (Player == 2)
        {
            if (skill == GaugeSkillType.Skill2 && gaugeImgData.skillGaugeFullP2)
            {
                gaugeImgData.skillGaugeBarValueP2 = -79.2f;

                gaugeImgData.skillGaugeFullP2 = false;
                skillData.enableHalfSkillP2 = false;
                skillData.enableUltimateSkillP2 = true;

                gaugeImgData.skillAlphaP2 = 0.0f;
                gaugeImgData.ImgSkillFullP2.color = new Color(gaugeImgData.ImgSkillFullP2.color.r, gaugeImgData.ImgSkillFullP2.color.g,
                                                              gaugeImgData.ImgSkillFullP2.color.b, gaugeImgData.skillAlphaP2);
            }
            else if (skill == GaugeSkillType.Skill1 && gaugeImgData.skillGuageHalfP2)
            {

                gaugeImgData.skillGaugeBarValueP2 = gaugeImgData.skillGaugeBarValueP2 - 40.15f;

                if (gaugeImgData.skillGaugeFullP2)
                {
                    gaugeImgData.skillGaugeBarValueP2 = -46.0f;

                    gaugeImgData.skillAlphaP2 = 0.0f;
                    gaugeImgData.ImgSkillFullP2.color = new Color(gaugeImgData.ImgSkillFullP2.color.r, gaugeImgData.ImgSkillFullP2.color.g,
                                                                  gaugeImgData.ImgSkillFullP2.color.b, gaugeImgData.skillAlphaP2);
                }
                else
                {
                    gaugeImgData.skillGuageHalfP2 = false;
                }

                gaugeImgData.skillGaugeFullP2 = false;
                skillData.enableHalfSkillP2 = true;
                skillData.enableUltimateSkillP2 = false;
            }
        }
    }

    private void OnSkillEffectMove()
    {
        // increase
        if (gaugeImgData.skillGaugeBarValueP1 >= gaugeImgData.preSkillGaugeBarValueP1)
        {
            if (gaugeImgData.preSkillGaugeBarValueP1 >= gaugeImgData.skillGaugeBarValueP1)
            {
                if (gaugeImgData.skillGaugeBarValueP1 >= 1.1f)
                    gaugeImgData.preSkillGaugeBarValueP1 = 1.1f;
                else
                    gaugeImgData.preSkillGaugeBarValueP1 = gaugeImgData.skillGaugeBarValueP1;
            }
            gaugeImgData.preSkillGaugeBarValueP1 += Time.deltaTime * 40.00f;
            gaugeImgData.RectSkillGageP1.localPosition = new Vector3(gaugeImgData.preSkillGaugeBarValueP1
                                                                    , gaugeImgData.RectSkillGageP1.localPosition.y
                                                                    ,gaugeImgData.RectSkillGageP1.localPosition.z);
        }

        if (gaugeImgData.skillGaugeBarValueP2 >= gaugeImgData.preSkillGaugeBarValueP2)
        {
            if (gaugeImgData.preSkillGaugeBarValueP2 >= gaugeImgData.skillGaugeBarValueP2)
            {
                if (gaugeImgData.skillGaugeBarValueP2 >= 1.1f)
                    gaugeImgData.preSkillGaugeBarValueP2 = 1.1f;
                else
                    gaugeImgData.preSkillGaugeBarValueP2 = gaugeImgData.skillGaugeBarValueP2;
            }
            gaugeImgData.preSkillGaugeBarValueP2 += Time.deltaTime * 40.00f;
            gaugeImgData.RectSkillGageP2.localPosition = new Vector3(gaugeImgData.preSkillGaugeBarValueP2
                                                                    , gaugeImgData.RectSkillGageP2.localPosition.y
                                                                    ,gaugeImgData.RectSkillGageP2.localPosition.z);
        }

        // decrease
        if (skillData.enableUltimateSkillP1 || skillData.enableHalfSkillP1)
        {
            gaugeImgData.preSkillGaugeBarValueP1 -= Time.deltaTime * 400.00f;

            if (gaugeImgData.preSkillGaugeBarValueP1 <= gaugeImgData.skillGaugeBarValueP1)
            {
                gaugeImgData.preSkillGaugeBarValueP1 = gaugeImgData.skillGaugeBarValueP1;
                skillData.enableUltimateSkillP1 = false;
                skillData.enableHalfSkillP1 = false;
            }
            gaugeImgData.RectSkillGageP1.localPosition = new Vector3(gaugeImgData.preSkillGaugeBarValueP1
                                                                    , gaugeImgData.RectSkillGageP1.localPosition.y
                                                                    , gaugeImgData.RectSkillGageP1.localPosition.z);
        }

        if (skillData.enableUltimateSkillP2 || skillData.enableHalfSkillP2)
        {
            gaugeImgData.preSkillGaugeBarValueP2 -= Time.deltaTime * 400.00f;

            if (gaugeImgData.preSkillGaugeBarValueP2 <= gaugeImgData.skillGaugeBarValueP2)
            {
                gaugeImgData.preSkillGaugeBarValueP2 = gaugeImgData.skillGaugeBarValueP2;
                skillData.enableUltimateSkillP2 = false;
                skillData.enableHalfSkillP2 = false;
            }
            gaugeImgData.RectSkillGageP2.localPosition = new Vector3(gaugeImgData.preSkillGaugeBarValueP2
                                                                    , gaugeImgData.RectSkillGageP2.localPosition.y
                                                                    , gaugeImgData.RectSkillGageP2.localPosition.z);
        }

        // full skill image fade
        if (gaugeImgData.skillGaugeFullP1)
        {
            gaugeImgData.skillGaugeBarValueP1 = 1.1f;
            if (gaugeImgData.isSkillAlphaP1)
            {
                gaugeImgData.skillAlphaP1 -= Time.deltaTime * 3.0f;
                if (gaugeImgData.skillAlphaP1 <= 0.0f)
                    gaugeImgData.isSkillAlphaP1 = false;
            }
            else
            {
                gaugeImgData.skillAlphaP1 += Time.deltaTime * 3.0f;
                if (gaugeImgData.skillAlphaP1 >= 1.0f)
                    gaugeImgData.isSkillAlphaP1 = true;
            }
            gaugeImgData.ImgSkillFullP1.color = new Color(gaugeImgData.ImgSkillFullP1.color.r, gaugeImgData.ImgSkillFullP1.color.g,
                                                          gaugeImgData.ImgSkillFullP1.color.b, gaugeImgData.skillAlphaP1);
        }
        if (gaugeImgData.skillGaugeFullP2)
        {
            gaugeImgData.skillGaugeBarValueP2 = 1.1f;
            if (gaugeImgData.isSkillAlphaP2)
            {
                gaugeImgData.skillAlphaP2 -= Time.deltaTime * 3.0f;
                if (gaugeImgData.skillAlphaP2 <= 0.0f)
                    gaugeImgData.isSkillAlphaP2 = false;
            }
            else 
            {
                gaugeImgData.skillAlphaP2 += Time.deltaTime * 3.0f;
                if (gaugeImgData.skillAlphaP2 >= 1.0f)
                    gaugeImgData.isSkillAlphaP2 = true;
            }
            gaugeImgData.ImgSkillFullP2.color = new Color(gaugeImgData.ImgSkillFullP2.color.r, gaugeImgData.ImgSkillFullP2.color.g,
                                                          gaugeImgData.ImgSkillFullP2.color.b, gaugeImgData.skillAlphaP2);
        }
    }

    public override void OnHalfSkillImageAnimation(CharacterNames fighterName, int pMirror)
    {
        GameObject playerHalfSkill = null;

        if (fighterName == CharacterNames.miho) playerHalfSkill = skillData.HalfGaugeSkillMiho;
        else if (fighterName == CharacterNames.agni) playerHalfSkill = skillData.HalfGaugeSkillAgni;
        else if (fighterName == CharacterNames.valkiri) playerHalfSkill = skillData.HalfGaugeSkilValkiri;

        if (playerHalfSkill == null) Debug.LogError("player name is NOT FIND ! Check Player 1 or 2 name");

        playerHalfSkill.transform.position
               = new Vector3(Screen.width * 2 * pMirror, playerHalfSkill.transform.position.y, playerHalfSkill.transform.position.z);

        Sequence playSKillSequence = DOTween.Sequence();
        playSKillSequence.Insert(0, playerHalfSkill.transform.DOMoveX(Screen.width * 2 * pMirror, 0.0f));
        playSKillSequence.Insert(0, playerHalfSkill.transform.DOMoveX(Screen.width / 2, 0.4f));
        playSKillSequence.Insert(1, playerHalfSkill.transform.DOMoveX(-Screen.width * 2 * pMirror, 0.2f));
    }
    #endregion

    #region -- guard --
    public override void IncreaseGuardGaugeBar(int player, float gauge)
    {
        gauge = (gauge * 0.01f) * 1.0f;
        if (player == 1)
        {
            gaugeImgData.guardGaugeBarValueP1 += gauge;

            if (gaugeImgData.guardGaugeBarValueP1 >= 1.0f)
            {
                gaugeImgData.guardGaugeBarValueP1 = 1.0f;
                gaugeImgData.guardGaugeFullP1 = true;
            }
        }

        if (player == 2)
        {
            gaugeImgData.guardGaugeBarValueP2 += gauge;

            if (gaugeImgData.guardGaugeBarValueP2 >= 1.0f)
            {
                gaugeImgData.guardGaugeBarValueP2 = 1.0f;
                gaugeImgData.guardGaugeFullP2 = true;
            }
        }
    }

    public override void DecreaseGuard(int player)
    {
        if (player == 1 && gaugeImgData.guardGaugeFullP1)
        {
            gaugeImgData.guardGaugeBarValueP1 = 0.0f;
            gaugeImgData.guardGaugeFullP1 = false;
            gaugeImgData.guardGaugeUseP1 = true;

            gaugeImgData.guardAlphaP1 = 0.0f;
            gaugeImgData.ImgGuardFullP1.color = new Color(gaugeImgData.ImgGuardFullP1.color.r, gaugeImgData.ImgGuardFullP1.color.g,
                                                          gaugeImgData.ImgGuardFullP1.color.b, gaugeImgData.guardAlphaP1);
        }

        if (player == 2 && gaugeImgData.guardGaugeFullP2)
        {
            gaugeImgData.guardGaugeBarValueP2 = 0.0f;
            gaugeImgData.guardGaugeFullP2 = false;
            gaugeImgData.guardGaugeUseP2 = true;

            gaugeImgData.guardAlphaP2 = 0.0f;
            gaugeImgData.ImgGuardFullP2.color = new Color(gaugeImgData.ImgGuardFullP2.color.r, gaugeImgData.ImgGuardFullP2.color.g,
                                                          gaugeImgData.ImgGuardFullP2.color.b, gaugeImgData.guardAlphaP2);
        }

    }

    void OnGuardEffectMove()
    {
        // Guard Increase Ani 
        if (gaugeImgData.guardGaugeBarValueP1 > gaugeImgData.preGuardGaugeBarValueP1)
        {
            gaugeImgData.preGuardGaugeBarValueP1 += Time.deltaTime * 0.3f;

            if (gaugeImgData.preGuardGaugeBarValueP1 >= gaugeImgData.guardGaugeBarValueP1)
                gaugeImgData.preGuardGaugeBarValueP1 = gaugeImgData.guardGaugeBarValueP1;

            gaugeImgData.ImgGuardGageP1.fillAmount = gaugeImgData.preGuardGaugeBarValueP1;
        }
        if (gaugeImgData.guardGaugeBarValueP2 > gaugeImgData.preGuardGaugeBarValueP2)
        {
            gaugeImgData.preGuardGaugeBarValueP2 += Time.deltaTime * 0.3f;

            if (gaugeImgData.preGuardGaugeBarValueP2 >= gaugeImgData.guardGaugeBarValueP2)
                gaugeImgData.preGuardGaugeBarValueP2 = gaugeImgData.guardGaugeBarValueP2;

            gaugeImgData.ImgGuardGageP2.fillAmount = gaugeImgData.preGuardGaugeBarValueP2;
        }

        //Guard Decrease Ani
        if (gaugeImgData.guardGaugeUseP1)
        {
            gaugeImgData.preGuardGaugeBarValueP1 -= Time.deltaTime * 1.00f;

            if (gaugeImgData.preGuardGaugeBarValueP1 <= gaugeImgData.guardGaugeBarValueP1)
            {
                gaugeImgData.preGuardGaugeBarValueP1 = gaugeImgData.guardGaugeBarValueP1;
                gaugeImgData.guardGaugeUseP1 = false;
            }

            gaugeImgData.ImgGuardGageP1.fillAmount = gaugeImgData.preGuardGaugeBarValueP1;
        }

        if (gaugeImgData.guardGaugeUseP2)
        {
            gaugeImgData.preGuardGaugeBarValueP2 -= Time.deltaTime * 1.00f;

            if (gaugeImgData.preGuardGaugeBarValueP2 <= gaugeImgData.guardGaugeBarValueP2)
            {
                gaugeImgData.preGuardGaugeBarValueP2 = gaugeImgData.guardGaugeBarValueP2;
                gaugeImgData.guardGaugeUseP2 = false;
            }

            gaugeImgData.ImgGuardGageP2.fillAmount = gaugeImgData.preGuardGaugeBarValueP2;
        }

        //FullGage
        if (gaugeImgData.guardGaugeFullP1)
        {
            gaugeImgData.guardGaugeBarValueP1 = 1.0f;
            if (gaugeImgData.isGuardAlphaP1)
            {
                gaugeImgData.guardAlphaP1 -= Time.deltaTime * 3.0f;
                if (gaugeImgData.guardAlphaP1 <= 0.0f)
                    gaugeImgData.isGuardAlphaP1 = false;
            }
            else
            {
                gaugeImgData.guardAlphaP1 += Time.deltaTime * 3.0f;
                if (gaugeImgData.guardAlphaP1 >= 1.0f)
                    gaugeImgData.isGuardAlphaP1 = true;
            }
            gaugeImgData.ImgGuardFullP1.color = new Color(gaugeImgData.ImgGuardFullP1.color.r,
                                                          gaugeImgData.ImgGuardFullP1.color.g,
                                                          gaugeImgData.ImgGuardFullP1.color.b,
                                                          gaugeImgData.guardAlphaP1);
        }

        if (gaugeImgData.guardGaugeFullP2)
        {
            gaugeImgData.guardGaugeBarValueP2 = 1.0f;
            if (gaugeImgData.isGuardAlphaP2)
            {
                gaugeImgData.guardAlphaP2 -= Time.deltaTime * 3.0f;
                if (gaugeImgData.guardAlphaP2 <= 0.0f)
                    gaugeImgData.isGuardAlphaP2 = false;
            }
            else
            {
                gaugeImgData.guardAlphaP2 += Time.deltaTime * 3.0f;
                if (gaugeImgData.guardAlphaP2 >= 1.0f)
                    gaugeImgData.isGuardAlphaP2 = true;
            }
            gaugeImgData.ImgGuardFullP2.color = new Color(gaugeImgData.ImgGuardFullP2.color.r,
                                                          gaugeImgData.ImgGuardFullP2.color.g,
                                                          gaugeImgData.ImgGuardFullP2.color.b,
                                                          gaugeImgData.guardAlphaP2);
        }
    }

    #endregion

    #region -- hp --
    public override void SetHP(int playerNum, float curLife, bool reset)
    {
        if (reset)
        {
            if (playerNum == 1)
            {
                hpImgData.currentHpValueP1 = 1.0f;
                hpImgData.preHPValueP1 = 1.0f;
                hpImgData.ImgHealthBarP1.sprite = hpImgData.healthPointYellow;
                hpImgData.ImgHealthBarP1.fillAmount = hpImgData.currentHpValueP1;
            }
            else if (playerNum == 2)
            {
                hpImgData.currentHpValueP2 = 1.0f;
                hpImgData.preHPValueP2 = 1.0f;
                hpImgData.ImgHealthBarP2.sprite = hpImgData.healthPointYellow;
                hpImgData.ImgHealthBarP2.fillAmount = hpImgData.currentHpValueP2;
            }
        }
        OnHpControl(playerNum, curLife);
    }

    public override void OnSetMaxHp(int player, float maxPoints)
    {
        if (player == 1)
            hpImgData.maxHpValueP1 = maxPoints;
        else
            hpImgData.maxHpValueP2 = maxPoints;
    }

    // 체력 관리
    private void OnHpControl(int player, float curLife)
    {
        if(player == 1)
        {
            hpImgData.currentHpValueP1 = curLife / hpImgData.maxHpValueP1;

            if (hpImgData.currentHpValueP1 < 0) return;

            //피통이 0.3f 이하일 때 체력 빨간색으로 변경
            if (hpImgData.currentHpValueP1 < 0.3f)
                hpImgData.ImgHealthBarP1.sprite = hpImgData.healthPointRed;
            else
                hpImgData.ImgHealthBarP1.sprite = hpImgData.healthPointYellow;

            if (hpImgData.currentHpValueP1 < hpImgData.preHPValueP1)
            {
                hpImgData.shakePositionP1.DOShakePosition(0.1f, 60.0f, 30);
                hpImgData.hpDecreaseP1 = true;
            }
            else if (hpImgData.currentHpValueP1 > hpImgData.preHPValueP1)
            {
                hpImgData.hpDecreaseP1 = true;
            }
        }
        else if(player == 2)
        {
            hpImgData.currentHpValueP2 = curLife / hpImgData.maxHpValueP2;

            if (hpImgData.currentHpValueP2 < 0) return;

            //피통이 0.3f 이하일 때 체력 빨간색으로 변경
            if (hpImgData.currentHpValueP2 < 0.3f)
                hpImgData.ImgHealthBarP2.sprite = hpImgData.healthPointRed;
            else
                hpImgData.ImgHealthBarP2.sprite = hpImgData.healthPointYellow;

            if (hpImgData.currentHpValueP2 < hpImgData.preHPValueP2)
            {
                hpImgData.shakePositionP2.DOShakePosition(0.1f, 60.0f, 30);
                hpImgData.hpDecreaseP2 = true;
            }
            else if (hpImgData.currentHpValueP2 > hpImgData.preHPValueP2)
            {
                hpImgData.hpDecreaseP2 = true;
            }
        }
    }

    private void OnHpEffectMove()
    {
        // increase
        if (hpImgData.hpIncreaseP1)
        {
            hpImgData.preHPValueP1 += Time.deltaTime * 0.8f;

            if (hpImgData.preHPValueP1 >= hpImgData.currentHpValueP1)
            {
                hpImgData.preHPValueP1 = hpImgData.currentHpValueP1;
                hpImgData.hpIncreaseP1 = false;
            }

            hpImgData.ImgHealthBarP1.fillAmount = hpImgData.preHPValueP1;
        }
        else if (hpImgData.hpIncreaseP2)
        {
            hpImgData.preHPValueP2 += Time.deltaTime * 0.8f;

            if (hpImgData.preHPValueP2 >= hpImgData.currentHpValueP2)
            {
                hpImgData.preHPValueP2 = hpImgData.currentHpValueP2;
                hpImgData.hpIncreaseP2 = false;
            }

            hpImgData.ImgHealthBarP2.fillAmount = hpImgData.preHPValueP2;
        }

        // decrease
        if (hpImgData.hpDecreaseP1)
        {
            hpImgData.preHPValueP1 -= Time.deltaTime * 0.8f;

            if (hpImgData.preHPValueP1 <= hpImgData.currentHpValueP1)
            {
                hpImgData.preHPValueP1 = hpImgData.currentHpValueP1;
                hpImgData.hpDecreaseP1 = false;
            }

            hpImgData.ImgHealthBarP1.fillAmount = hpImgData.preHPValueP1;
        }
        else if (hpImgData.hpDecreaseP2)
        {
            hpImgData.preHPValueP2 -= Time.deltaTime * 0.8f;

            if (hpImgData.preHPValueP2 <= hpImgData.currentHpValueP2)
            {
                hpImgData.preHPValueP2 = hpImgData.currentHpValueP2;
                hpImgData.hpDecreaseP2 = false;
            }

            hpImgData.ImgHealthBarP2.fillAmount = hpImgData.preHPValueP2;
        }

        ///////////////////////////////////////////////////
    }
    #endregion

    #region -- round state options --
    public override void OnRoundInit()
    {
        //값 초기화
        gaugeImgData.skillGaugeBarValueP1 = -85.2f;             gaugeImgData.skillGaugeBarValueP2 = -85.2f;
        gaugeImgData.preSkillGaugeBarValueP1 = -85.2f;          gaugeImgData.preSkillGaugeBarValueP2 = -85.2f;

        gaugeImgData.skillAlphaP1 = 0.0f;                       gaugeImgData.skillAlphaP2 = 0.0f;
        gaugeImgData.guardAlphaP1 = 0.0f;                       gaugeImgData.guardAlphaP2 = 0.0f;

        gaugeImgData.guardGaugeBarValueP1 = 0.0f;               gaugeImgData.guardGaugeBarValueP2 = 0.0f;
        gaugeImgData.preGuardGaugeBarValueP1 = 0.0f;            gaugeImgData.preGuardGaugeBarValueP2 = 0.0f;

        gaugeImgData.skillGaugeFullP1 = false;                  gaugeImgData.skillGaugeFullP2 = false;
        gaugeImgData.skillGuageHalfP1 = false;                  gaugeImgData.skillGuageHalfP2 = false;

        gaugeImgData.isSkillAlphaP1 = false;                    gaugeImgData.isSkillAlphaP2 = false;
        gaugeImgData.isGuardAlphaP1 = false;                    gaugeImgData.isGuardAlphaP2 = false;

        gaugeImgData.guardGaugeFullP1 = false;                  gaugeImgData.guardGaugeFullP2 = false;
        gaugeImgData.guardGaugeUseP1 = false;                   gaugeImgData.guardGaugeUseP2 = false;

        skillData.enableUltimateSkillP1 = false;                skillData.enableUltimateSkillP2 = false;
        skillData.enableHalfSkillP1 = false;                    skillData.enableHalfSkillP2 = false;

        //수치 초기화
        gaugeImgData.ImgGuardGageP1.fillAmount = 0.0f;          gaugeImgData.ImgGuardGageP2.fillAmount = 0.0f;

        gaugeImgData.RectSkillGageP1.localPosition = new Vector3(gaugeImgData.skillGaugeBarValueP1
                                                                , gaugeImgData.RectSkillGageP1.localPosition.y
                                                                , gaugeImgData.RectSkillGageP1.localPosition.z);

        gaugeImgData.RectSkillGageP2.localPosition = new Vector3(gaugeImgData.skillGaugeBarValueP2
                                                                , gaugeImgData.RectSkillGageP2.localPosition.y
                                                                , gaugeImgData.RectSkillGageP2.localPosition.z);

        gaugeImgData.ImgSkillFullP1.color = new Color(gaugeImgData.ImgSkillFullP1.color.r
                                                     , gaugeImgData.ImgSkillFullP1.color.g
                                                     , gaugeImgData.ImgSkillFullP1.color.b, 0);

        gaugeImgData.ImgSkillFullP2.color = new Color(gaugeImgData.ImgSkillFullP2.color.r
                                                     , gaugeImgData.ImgSkillFullP2.color.g
                                                     , gaugeImgData.ImgSkillFullP2.color.b, 0);

        gaugeImgData.ImgGuardFullP1.color = new Color(gaugeImgData.ImgGuardFullP1.color.r
                                                     , gaugeImgData.ImgGuardFullP1.color.g
                                                     , gaugeImgData.ImgGuardFullP1.color.b, 0);

        gaugeImgData.ImgGuardFullP2.color = new Color(gaugeImgData.ImgGuardFullP2.color.r
                                                     , gaugeImgData.ImgGuardFullP2.color.g
                                                     , gaugeImgData.ImgGuardFullP2.color.b, 0);
        roundStateData.TimeOver.SetActive(false);

        roundStateData.mihoWinP1.SetActive(false);
        roundStateData.mihoWinP2.SetActive(false);
        roundStateData.agniWinP1.SetActive(false);
        roundStateData.agniWinP2.SetActive(false);
        roundStateData.valkiriWinP1.SetActive(false);
        roundStateData.valkiriWinP2.SetActive(false);

        hpImgData.ImgHealthBarP1.fillAmount = 1.0f;
        hpImgData.ImgHealthBarP2.fillAmount = 1.0f;
    }

    public override void FightLogosInit()
    {
        roundStateData.Round1.SetActive(false);
        roundStateData.Round2.SetActive(false);
        roundStateData.Round3.SetActive(false);
        roundStateData.Fight.SetActive(true);
        CGameManager.PlaySoundFX(CSceneManager.Instance.announcerSounds.fight);
    }

    //Fight 출력 후 시간이 다시 흐르게 하는 함수
    public override void AwakeTimeRunning()
    {
        roundStateData.Fight.SetActive(false);
        OnSetTimeControlRoundEvent(false);
        OnSetRunning(true);
    }

    public override float OnGetCurrentPlayTime()
    {
        return timeController.Present_Time;
    }

    public override void OnSetTimeControlRoundEvent(bool value)
    {
        timeController.is_RoundEventEnd = value;
    }

    public override bool OnGetTimeControlRoundEvent()
    {
        return timeController.is_RoundEventEnd;
    }

    public override void ResetTimeDatas(float time)
    {
        timeController.ResetTimeDatas(time);
    }

    public override float GetTimePreTime()
    {
        return timeController.Present_Time;
    }

    public override void KOimgSet(bool on)
    {
        roundStateData.KO.SetActive(on);
    }

    public override void CharStateOption(int type, int player_num)
    {
        if (player_num == 1)
        {
            roundStateData.aerialP1.SetActive(false);
            roundStateData.counterP1.SetActive(false);
            roundStateData.stunP1.SetActive(false);
            roundStateData.stateLineP1.SetActive(false);
            roundStateData.FirstHitP1.SetActive(false);
        }
        else //if (player_num == 2)
        {
            roundStateData.aerialP2.SetActive(false);
            roundStateData.counterP2.SetActive(false);
            roundStateData.stunP2.SetActive(false);
            roundStateData.stateLineP2.SetActive(false);
            roundStateData.FirstHitP2.SetActive(false);
        }

        switch (type)
        {
            case 1:
                OnAerialView(player_num);
                break;
            case 2:
                OnCounterView(player_num);
                break;
            case 3:
                OnStunView(player_num);
                break;
            case 4:
                OnFirstHitView(player_num);
                break;
            default:
                Debug.LogError("UI Ctrl Charter State Option Error!");
                break;
        }
    }

    //ui 표시 시에 따라붙는 선
    void OnLineView(int playerNum)
    {
        if(playerNum == 1)
        {
            roundStateData.stateLineP1.SetActive(true);
            Sequence PlayLine_1 = DOTween.Sequence();
            PlayLine_1.Insert(0, roundStateData.stateLineP1.transform.DOMoveX(Screen.width * -0.1f, 0.0f));
            PlayLine_1.Insert(0, roundStateData.stateLineP1.GetComponent<Image>().DOFade(1.0f, 0.0f));
            PlayLine_1.Insert(0.1f, roundStateData.stateLineP1.transform.DOMoveX(Screen.width * 0.1f, 0.2f));
            PlayLine_1.Insert(1.5f, roundStateData.stateLineP1.GetComponent<Image>().DOFade(0.0f, 0.4f));
        }
        else
        {
            roundStateData.stateLineP2.SetActive(true);
            Sequence PlayLine_2 = DOTween.Sequence();
            PlayLine_2.Insert(0, roundStateData.stateLineP2.transform.DOMoveX(Screen.width * 1.1f, 0.0f));
            PlayLine_2.Insert(0, roundStateData.stateLineP2.GetComponent<Image>().DOFade(1.0f, 0.0f));
            PlayLine_2.Insert(0.1f, roundStateData.stateLineP2.transform.DOMoveX(Screen.width * 0.9f, 0.2f));
            PlayLine_2.Insert(1.5f, roundStateData.stateLineP2.GetComponent<Image>().DOFade(0.0f, 0.4f));
        }
    }

    void OnFirstHitView(int playerNum)
    {
        OnLineView(playerNum);
        if (playerNum == 1)
        {
            roundStateData.FirstHitP1.SetActive(true);
            Sequence PlayFirstHit_1p = DOTween.Sequence();
            PlayFirstHit_1p.Insert(0, roundStateData.FirstHitP1.transform.DOMoveX(Screen.width * -0.1f, 0.0f));
            PlayFirstHit_1p.Insert(0, roundStateData.FirstHitP1.GetComponent<Image>().DOFade(1.0f, 0.0f));
            PlayFirstHit_1p.Insert(0.1f, roundStateData.FirstHitP1.transform.DOMoveX(Screen.width * 0.1f, 0.2f));
            PlayFirstHit_1p.Insert(1.5f, roundStateData.FirstHitP1.GetComponent<Image>().DOFade(0.0f, 0.4f));
        }
        else //if (playerNum == 2)
        {
            roundStateData.FirstHitP2.SetActive(true);
            Sequence PlayFirstHit_2p = DOTween.Sequence();
            PlayFirstHit_2p.Insert(0, roundStateData.FirstHitP2.transform.DOMoveX(Screen.width * 1.1f, 0.0f));
            PlayFirstHit_2p.Insert(0, roundStateData.FirstHitP2.GetComponent<Image>().DOFade(1.0f, 0.0f));
            PlayFirstHit_2p.Insert(0.1f, roundStateData.FirstHitP2.transform.DOMoveX(Screen.width * 0.9f, 0.2f));
            PlayFirstHit_2p.Insert(1.5f, roundStateData.FirstHitP2.GetComponent<Image>().DOFade(0.0f, 0.4f));
        }
    }

    void OnStunView(int playerNum)
    {
        OnLineView(playerNum);
        if (playerNum == 1)
        {
            roundStateData.stunP1.SetActive(true);
            Sequence PlayStun_1p = DOTween.Sequence();
            PlayStun_1p.Insert(0, roundStateData.stunP1.transform.DOMoveX(Screen.width * -0.1f, 0.0f));
            PlayStun_1p.Insert(0, roundStateData.stunP1.GetComponent<Image>().DOFade(1.0f, 0.0f));
            PlayStun_1p.Insert(0.1f, roundStateData.stunP1.transform.DOMoveX(Screen.width * 0.1f, 0.2f));
            PlayStun_1p.Insert(1.5f, roundStateData.stunP1.GetComponent<Image>().DOFade(0.0f, 0.4f));
        }
        else //if (playerNum == 2)
        {
            roundStateData.stunP2.SetActive(true);
            Sequence PlayStun_2p = DOTween.Sequence();
            PlayStun_2p.Insert(0, roundStateData.stunP2.transform.DOMoveX(Screen.width * 1.1f, 0.0f));
            PlayStun_2p.Insert(0, roundStateData.stunP2.GetComponent<Image>().DOFade(1.0f, 0.0f));
            PlayStun_2p.Insert(0.1f, roundStateData.stunP2.transform.DOMoveX(Screen.width * 0.9f, 0.2f));
            PlayStun_2p.Insert(1.5f, roundStateData.stunP2.GetComponent<Image>().DOFade(0.0f, 0.4f));
        }
    }

    void OnAerialView(int playerNum)
    {
        OnLineView(playerNum);
        if (playerNum == 1)
        {
            roundStateData.aerialP1.SetActive(true);
            Sequence PlayAerial_1p = DOTween.Sequence();
            PlayAerial_1p.Insert(0, roundStateData.aerialP1.transform.DOMoveX(Screen.width * -0.1f, 0.0f));
            PlayAerial_1p.Insert(0, roundStateData.aerialP1.GetComponent<Image>().DOFade(1.0f, 0.0f));
            PlayAerial_1p.Insert(0.1f, roundStateData.aerialP1.transform.DOMoveX(Screen.width * 0.1f, 0.2f));
            PlayAerial_1p.Insert(1.5f, roundStateData.aerialP1.GetComponent<Image>().DOFade(0.0f, 0.4f));

        }   
        else //if (playerNum == 2)
        {
            roundStateData.aerialP2.SetActive(true);
            Sequence PlayAerial_2p = DOTween.Sequence();
            PlayAerial_2p.Insert(0, roundStateData.aerialP2.transform.DOMoveX(Screen.width * 1.1f, 0.0f));
            PlayAerial_2p.Insert(0, roundStateData.aerialP2.GetComponent<Image>().DOFade(1.0f, 0.0f));
            PlayAerial_2p.Insert(0.1f, roundStateData.aerialP2.transform.DOMoveX(Screen.width * 0.9f, 0.2f));
            PlayAerial_2p.Insert(1.5f, roundStateData.aerialP2.GetComponent<Image>().DOFade(0.0f, 0.4f));
        }
    }

    void OnCounterView(int playerNum)
    {
        OnLineView(playerNum);
        if (playerNum == 1)
        {
            roundStateData.counterP1.SetActive(true);
            Sequence PlayCounter_1p = DOTween.Sequence();
            PlayCounter_1p.Insert(0, roundStateData.counterP1.transform.DOMoveX(Screen.width * -0.1f, 0.0f));
            PlayCounter_1p.Insert(0, roundStateData.counterP1.GetComponent<Image>().DOFade(1.0f, 0.0f));
            PlayCounter_1p.Insert(0.1f, roundStateData.counterP1.transform.DOMoveX(Screen.width * 0.1f, 0.2f));
            PlayCounter_1p.Insert(1.5f, roundStateData.counterP1.GetComponent<Image>().DOFade(0.0f, 0.4f));
        }

        if (playerNum == 2)
        {
            roundStateData.counterP2.SetActive(true);
            Sequence PlayCounter_2p = DOTween.Sequence();
            PlayCounter_2p.Insert(0, roundStateData.counterP2.transform.DOMoveX(Screen.width * 1.1f, 0.0f));
            PlayCounter_2p.Insert(0, roundStateData.counterP2.GetComponent<Image>().DOFade(1.0f, 0.0f));
            PlayCounter_2p.Insert(0.1f, roundStateData.counterP2.transform.DOMoveX(Screen.width * 0.9f, 0.2f));
            PlayCounter_2p.Insert(1.5f, roundStateData.counterP2.GetComponent<Image>().DOFade(0.0f, 0.4f));

        }
    }

    public override void OnTimeOverView(bool on)
    {
        roundStateData.TimeOver.SetActive(on);
    }

    public override bool GetTimeOverActive()
    {
        return roundStateData.TimeOver.activeSelf;
    }

    public override void OnPerfactKOView(bool on)
    {
        roundStateData.Perfact.SetActive(on);
    }

    public override void OnVictoryView(int playerNum, int roundWinIdx)
    {
        if (playerNum == 1)
            roundStateData.WinPointsP1[roundWinIdx - 1].SetActive(true);
        else if (playerNum == 2)
            roundStateData.WinPointsP2[roundWinIdx - 1].SetActive(true);
    }

    public override void OnWinView(int playernum, CharacterNames fighterName, bool view)
    {
        if (playernum == 1)
        {
            if (fighterName == CharacterNames.miho)
                roundStateData.mihoWinP1.SetActive(view);
            else if (fighterName == CharacterNames.agni)
                roundStateData.agniWinP1.SetActive(view);
            else if (fighterName == CharacterNames.valkiri)
                roundStateData.valkiriWinP1.SetActive(view);
        }
        else //if (playernum == 2)
        {
            if (fighterName == CharacterNames.miho)
                roundStateData.mihoWinP2.SetActive(view);
            else if (fighterName == CharacterNames.agni)
                roundStateData.agniWinP2.SetActive(view);
            else if (fighterName == CharacterNames.valkiri)
                roundStateData.valkiriWinP2.SetActive(view);
        }
    }

    public override void OnComboView(int playerNum, int ComboNum)
    {
        int ComboNum_10 = (int)ComboNum / 10;
        int ComboNum_1 = (int)ComboNum - (ComboNum_10 * 10);

        Sequence playerSequence = DOTween.Sequence();
        Image    playerComboImg = null;
        Image    playerComboNumberImg = null;
        Image    playerComboNumberImg10 = null;

        if (playerNum == 1)
        {
            roundStateData.ComboNumberP1.sprite = roundStateData.ComboSpriteNumbers[ComboNum_1];
            roundStateData.ComboNumber10P1.sprite = roundStateData.ComboSpriteNumbers[ComboNum_10];
            playerComboImg = roundStateData.ComboP1;
            playerComboNumberImg = roundStateData.ComboNumberP1;
            playerComboNumberImg10 = roundStateData.ComboNumber10P1;
        }
        else //if (playerNum == 2)
        {
            roundStateData.ComboNumberP2.sprite = roundStateData.ComboSpriteNumbers[ComboNum_1];
            roundStateData.ComboNumber10P2.sprite = roundStateData.ComboSpriteNumbers[ComboNum_10];
            playerComboImg = roundStateData.ComboP2;
            playerComboNumberImg = roundStateData.ComboNumberP2;
            playerComboNumberImg10 = roundStateData.ComboNumber10P2;
        }

        if (ComboNum_10 > 0)
        {
            playerSequence.Insert(0, playerComboNumberImg10.DOFade(1.0f, 0.2f));
            playerSequence.Insert(0, playerComboNumberImg10.transform.DOShakeScale(0.1f, 1.5f, 10, 90, false));
            playerSequence.Insert(0.5f, playerComboNumberImg10.DOFade(0.0f, 0.2f));
        }

        playerSequence.Insert(0, playerComboImg.DOFade(1.0f, 0.2f));
        playerSequence.Insert(0, playerComboNumberImg.DOFade(1.0f, 0.2f));

        playerSequence.Insert(0, playerComboImg.transform.DOShakeScale(0.1f, 1.5f, 10, 90, false));
        playerSequence.Insert(0, playerComboNumberImg.transform.DOShakeScale(0.1f, 1.5f, 10, 90, false));

        playerSequence.Insert(0.5f, playerComboImg.DOFade(0.0f, 0.2f));
        playerSequence.Insert(0.5f, playerComboNumberImg.DOFade(0.0f, 0.2f));
    }

    public override void OnRoundView(int currentRound)
    {
        if (currentRound == 0)
        {
            Debug.LogError("Round Data Null");
            return;
        }

        switch (currentRound)
        {
            case 1:
                {
                    roundStateData.Round1.SetActive(true);
                    CGameManager.PlaySoundFX(CSceneManager.Instance.announcerSounds.round1);
                }
                break;
            case 2:
                {
                    roundStateData.Round2.SetActive(true);
                    CGameManager.PlaySoundFX(CSceneManager.Instance.announcerSounds.round2);
                }
                break;
            case 3:
                {
                    roundStateData.Round3.SetActive(true);
                    CGameManager.PlaySoundFX(CSceneManager.Instance.announcerSounds.round3);
                }
                break;
        }
    }

    public override void OnMakeClone(CharacterNames name)
    {
        if (name == CharacterNames.agni)
        {
            GameObject temp = Instantiate(skillData.agniSpine) as GameObject;
        }
        else if (name == CharacterNames.miho)
        {
            GameObject temp = Instantiate(skillData.mihoSpine) as GameObject;
        }
        else if (name == CharacterNames.valkiri)
        {
            GameObject temp = Instantiate(skillData.valkiriSpine) as GameObject;
        }
    }

    public override void SetPlayerUiImages(int player, CharacterNames name)
    {
        if(player == 1)
        {
            switch (name)
            {
                case CharacterNames.agni:
                    {
                        characterImgData.player1ChrImg.sprite = characterImgData.agniChrSprite;
                        characterImgData.player1NameImg.sprite = characterImgData.agniNameSprite;
                    }
                    break;
                case CharacterNames.miho:
                    {
                        characterImgData.player1ChrImg.sprite = characterImgData.mihoChrSprite;
                        characterImgData.player1NameImg.sprite = characterImgData.mihoNameSprite;
                    }
                    break;
                case CharacterNames.valkiri:
                    {
                        characterImgData.player1ChrImg.sprite = characterImgData.valkiriChrSprite;
                        characterImgData.player1NameImg.sprite = characterImgData.valkiriNameSprite;
                    }
                    break;
            }
        }
        else
        {
            switch (name)
            {
                case CharacterNames.agni:
                    {
                        characterImgData.player2ChrImg.sprite = characterImgData.agniChrSprite;
                        characterImgData.player2NameImg.sprite = characterImgData.agniNameSprite;
                    }
                    break;
                case CharacterNames.miho:
                    {
                        characterImgData.player2ChrImg.sprite = characterImgData.mihoChrSprite;
                        characterImgData.player2NameImg.sprite = characterImgData.mihoNameSprite;
                    }
                    break;
                case CharacterNames.valkiri:
                    {
                        characterImgData.player2ChrImg.sprite = characterImgData.valkiriChrSprite;
                        characterImgData.player2NameImg.sprite = characterImgData.valkiriNameSprite;
                    }
                    break;
            }
        }
    }
    #endregion
}
