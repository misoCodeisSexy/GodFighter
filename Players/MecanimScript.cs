using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MecanimAnimationData
{
    public AnimationClip clip;
    public string clipName;
    [HideInInspector] public float speed = 1;
    public float transitionDuration = -1;
    public WrapMode wrapMode;
    public bool applyRootMition;
    public float length = 1;

    public float originalSpeed = 1;
    [HideInInspector] public int timesPlayed = 0;
    [HideInInspector] public float secondsPlayed = 0;
    [HideInInspector] public float nomalizedSpeed = 1;
    [HideInInspector] public float nomalizedTime = 1;
    [HideInInspector] public string stateName;
}

[RequireComponent(typeof(Animator))]
public class MecanimScript : MonoBehaviour
{
    public MecanimAnimationData defaultAnimation = new MecanimAnimationData();
    public MecanimAnimationData[] allAnimations = new MecanimAnimationData[0];  
    public bool overrideAnimationUpdate = false;
    public bool alwaysPlay = false;
    public float defaultTransitionDuration = .15f;
    public WrapMode defaultWrapMode = WrapMode.Loop;

    public Animator animator; 

    [SerializeField]
    private RuntimeAnimatorController controller; 
    public AnimatorOverrideController overrideController; 

    public MecanimAnimationData currentAnimationData = null; 
    public bool currentMirror;

    public float currentNomalizedTime;
    public float currentSpeed;
    public string currentState;


    private void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
        animator.logWarnings = false;
        controller = null;
        overrideController = null;
        controller = (RuntimeAnimatorController)Instantiate(Resources.Load("MasterController"));

        foreach (MecanimAnimationData animData in allAnimations)
        {
            if (animData.clip == null) continue;

            if (animData.wrapMode == WrapMode.Default)
                animData.wrapMode = defaultWrapMode;

            animData.clip.wrapMode = animData.wrapMode;
        }
    }

    public void FixedUpdateGOF()
    {
        if(overrideAnimationUpdate)
        {
            animator.enabled = false;
            animator.Update(Time.fixedDeltaTime);
        }

        if (currentAnimationData == null || currentAnimationData.clip == null)
            return;

        currentAnimationData.secondsPlayed += (Time.fixedDeltaTime * GetSpeed());

        if(currentAnimationData.secondsPlayed > currentAnimationData.length)
        {
            currentAnimationData.secondsPlayed = currentAnimationData.length;
        }

        currentAnimationData.nomalizedTime = currentAnimationData.secondsPlayed / currentAnimationData.length;

        // 애니메이션 길이와 같으면
        if(currentAnimationData.secondsPlayed == currentAnimationData.length)
        {
            if(currentAnimationData.clip.wrapMode == WrapMode.Loop || currentAnimationData.clip.wrapMode == WrapMode.PingPong)
            {
                currentAnimationData.timesPlayed++;

                if(currentAnimationData.clip.wrapMode == WrapMode.Loop)
                {
                    SetCurrentClipPosition(0);
                }
            }
            else if(currentAnimationData.timesPlayed == 0)
            {
                currentAnimationData.timesPlayed = 1;

                if((currentAnimationData.clip.wrapMode == WrapMode.Once || currentAnimationData.clip.wrapMode == WrapMode.Clamp)
                    && alwaysPlay)
                {
                    Play(defaultAnimation, currentMirror);
                }
                else if( !alwaysPlay )
                {
                    SetSpeed(0);
                }
            }
        }
    }

    public void OnPlayAnimation(MecanimAnimationData targetAnimationData, float blendingTime, float nomalizedTime, bool mirror)
    {
        if (targetAnimationData == null || targetAnimationData.clip == null)
            return;

        bool prevMirror = currentMirror;
        currentMirror = mirror;

        float animSpeed = targetAnimationData.originalSpeed * (targetAnimationData.originalSpeed < 0 ? -1 : 1);

        currentNomalizedTime = GetCurrentClipPosition();
        currentState = "State1";

        if( !mirror )
        {
            if(targetAnimationData.originalSpeed >= 0)
            {
                currentState = "State1";
            }
            else
            {
                currentState = "State2";
            }
        }
        else
        {
            if(targetAnimationData.originalSpeed >= 0)
            {
                currentState = "State3";
            }
            else
            {
                currentState = "State4";
            }
        }

        overrideController = new AnimatorOverrideController();
        overrideController.runtimeAnimatorController = null;
        overrideController.runtimeAnimatorController = controller;

        if (currentAnimationData != null && currentAnimationData.clip != null)
        {
            overrideController["Default"] = null;
            overrideController["Default"] = currentAnimationData.clip;
        }

        overrideController[currentState] = null;
        overrideController[currentState] = targetAnimationData.clip;

        if (blendingTime == -1)
            blendingTime = defaultTransitionDuration;

        if (blendingTime <= 0 || currentAnimationData == null)
        {
            animator.runtimeAnimatorController = null;
            animator.runtimeAnimatorController = overrideController;
            animator.Play(currentState, 0, nomalizedTime);
        }
        else
        {
            animator.runtimeAnimatorController = null;
            animator.runtimeAnimatorController = overrideController;

            currentAnimationData.stateName = "Default";
            SetCurrentClipPosition(currentNomalizedTime);

            animator.Play("Default", 0, nomalizedTime);
            animator.CrossFade(currentState, (blendingTime / animSpeed), 0, nomalizedTime);
        }

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if(info.IsName("Default"))
        {
            if(animator.GetBool("Mirror") != prevMirror)
            {
                animator.SetBool("Mirror", prevMirror);
            }
        }
        animator.Update(0);

        targetAnimationData.timesPlayed = 0;
        targetAnimationData.secondsPlayed = (nomalizedTime * targetAnimationData.length) / animSpeed;
        targetAnimationData.nomalizedTime = nomalizedTime;
        targetAnimationData.speed = targetAnimationData.originalSpeed;

        SetSpeed(targetAnimationData.originalSpeed);

        if(currentAnimationData != null)
        {
            currentAnimationData.speed = currentAnimationData.originalSpeed;
            currentAnimationData.nomalizedTime = 1;
            currentAnimationData.timesPlayed = 0;
        }

        currentAnimationData = targetAnimationData;
        currentAnimationData.stateName = currentState;
    }

    public void SetMirror(bool toggle)
    {
        SetMirror(toggle, 0, false);
    }

    public void SetMirror(bool toggle, float blendingTime, bool forceMirror)
    {
        //////////////////////////////////////////////////
    }

    public void RestoreSpeed()
    {
        SetSpeed(currentAnimationData.speed);
    }

    public float GetSpeed()
    {
        return animator.speed;
    }

    public string GetCurrentClipName()
    {
        return currentAnimationData.clipName;
    }

    public void SetCurrentClipPosition(float nomalizedTime)
    {
        SetCurrentClipPosition(nomalizedTime, false);
    }

    public void SetCurrentClipPosition(float nomalizedTime, bool pause)
    {
        nomalizedTime = Mathf.Clamp01(nomalizedTime);
        currentAnimationData.secondsPlayed = nomalizedTime * currentAnimationData.length;
        currentAnimationData.nomalizedTime = nomalizedTime;

        animator.Play(currentAnimationData.stateName, 0, nomalizedTime);
        animator.Update(0);

        if (pause)
            Pause();
    }
    public float GetCurrentClipPosition()
    {
        if (currentAnimationData == null)
            return 0;

        return currentAnimationData.secondsPlayed / currentAnimationData.length;
    }

    public void Pause()
    {
        SetSpeed(0);
    }

    public void SetSpeed(float speed)
    {
        animator.speed = Mathf.Abs(speed);
        currentSpeed = speed;
    }

    public void SetSpeed(string clipName, float speed)
    {
        SetSpeed(GetAnimationData(clipName), speed);
    }

    public void SetSpeed(MecanimAnimationData animData, float speed)
    {
        if(animData != null)
        {
            animData.nomalizedSpeed = speed / animData.originalSpeed;
            animData.speed = speed;

            if (IsPlaying(animData))
                SetSpeed(speed);
        }
    }

    public void SetNomalizedSpeed(string clipName, float nomalizedSpeed)
    {
        SetNomalizedSpeed(GetAnimationData(clipName), nomalizedSpeed);
    }

    public void SetNomalizedSpeed(MecanimAnimationData animData, float nomalizedSpeed)
    {
        if (animData == null) return;

        animData.nomalizedSpeed = nomalizedSpeed;
        animData.speed = animData.originalSpeed * animData.nomalizedSpeed;

        if (IsPlaying(animData))
            SetSpeed(animData.speed);
    }

    public void AddClip(AnimationClip clip, string newName, float speed, WrapMode wrapMode, float length)
    {
        if (GetAnimationData(newName) != null)
            Debug.LogError("this animation already exists... " + newName);

        MecanimAnimationData animData = new MecanimAnimationData();
        animData.clip = (AnimationClip)Instantiate(clip);

        if (wrapMode == WrapMode.Default)
            wrapMode = defaultWrapMode;

        animData.clip.wrapMode = wrapMode;
        animData.clip.name = newName;
        animData.clipName = newName;
        animData.speed = speed;
        animData.originalSpeed = speed;
        animData.length = length;
        animData.wrapMode = wrapMode;

        List<MecanimAnimationData> animationDataList = new List<MecanimAnimationData>(allAnimations);
        animationDataList.Add(animData);
        allAnimations = animationDataList.ToArray();
    }

    public void Play(MecanimAnimationData animationData, bool mirror)
    {
        OnPlayAnimation(animationData, animationData.transitionDuration, 0, mirror);
    }

    public void Play(string clipName, float blendingTime, float nomalizedTime, bool mirror)
    {
        OnPlayAnimation(GetAnimationData(clipName), blendingTime, nomalizedTime, mirror);
    }

    public MecanimAnimationData GetAnimationData(string clipName)
    {
        foreach (MecanimAnimationData animData in allAnimations)
        {
            if (animData.clipName == clipName)
                return animData;
        }
            
        if (clipName == defaultAnimation.clipName)
            return defaultAnimation;

        return null;
    }

    public void SetDefaultClip(AnimationClip clip, string name, float speed, WrapMode wrapMode)
    {
        defaultAnimation.clip = (AnimationClip)Instantiate(clip);
        defaultAnimation.clip.wrapMode = wrapMode;
        defaultAnimation.clipName = name;
        defaultAnimation.speed = speed;
        defaultAnimation.originalSpeed = speed;
        defaultAnimation.transitionDuration = -1;
        defaultAnimation.wrapMode = wrapMode;
    }

    public bool IsPlaying(string clipName, float weight)
    {
        return IsPlaying(GetAnimationData(clipName), weight);
    }

    public bool IsPlaying(MecanimAnimationData animData)
    {
        return (currentAnimationData == animData);
    }

    public bool IsPlaying(MecanimAnimationData animData, float weight)
    {
        if (animData == null) return false;
        if (currentAnimationData == null) return false;
        if (currentAnimationData == animData && animData.wrapMode == WrapMode.Once && animData.timesPlayed > 0) return false;
        if (currentAnimationData == animData && animData.wrapMode == WrapMode.Clamp && animData.timesPlayed > 0) return false;
        if (currentAnimationData == animData && animData.wrapMode == WrapMode.ClampForever) return true;
        if (currentAnimationData == animData) return true;

        AnimatorClipInfo[] animationInfoArray = animator.GetCurrentAnimatorClipInfo(0);
        foreach(AnimatorClipInfo animInfo in animationInfoArray)
        {
            if (animData.clip == animInfo.clip && animInfo.weight >= weight)
                return true;
        }
        return false;
    }
}
