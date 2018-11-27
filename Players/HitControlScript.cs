using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodFighter;

public class HitControlScript : MonoBehaviour {

    public CPlayerCtrl myScript;
    public ColliderInfo[] hitBoxes;
    
    public HitType hitType;
    public bool overrideHitAcceleration = true;

    public bool isHit;

    private void Start()
    {
        myScript = transform.parent.parent.GetComponent<CPlayerCtrl>();
        hitBoxes = transform.gameObject.GetComponentsInChildren<ColliderInfo>();

        for (int i = 0; i < hitBoxes.Length; i++)
        {
            hitBoxes[i].myScript = this.myScript;
        }
    }

    public void FiexdUpdateGOF()
    {
        for(int i = 0; i < hitBoxes.Length; i++)
        {
            hitBoxes[i].FiexdUpdateGOF();
        }
    }

    public void ResetHit()
    {
        isHit = false;
    }

    public bool GetHitBoxesGuardableAreaHit()
    {
        for (int i = 0; i < hitBoxes.Length; i++)
        {
            if (hitBoxes[i].isGuardableAreaEnter)
                return true;
        }
        return false;
    }

    public void SetAllHitBoxes(bool flag)
    {
        for (int i = 0; i < hitBoxes.Length; i++)
            hitBoxes[i].isGuardableAreaEnter = flag;
    }

    public Vector3 GetPosition(BodyPart bodyPart)
    {
        foreach(ColliderInfo box in hitBoxes)
        {
            if (box.bodypart == bodyPart) return box.position.position;
        }

        return Vector3.zero;
    }

    public Transform GetTransform(BodyPart bodyPart)
    {
        foreach (ColliderInfo box in hitBoxes)
        {
            if (box.bodypart == bodyPart) return box.transform;
        }

        return null;
    }
}
