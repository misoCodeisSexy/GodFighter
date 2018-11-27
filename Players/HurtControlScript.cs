using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodFighter;

public class HurtControlScript : MonoBehaviour
{
    public CPlayerCtrl myScript;
    public ColliderInfo[] hurtBoxes; // equal enum AttackIdx number sort.
    public GameObject hurtBoxObj;

    private void Start()
    {
        myScript = transform.parent.parent.GetComponent<CPlayerCtrl>();

        hurtBoxes = transform.gameObject.GetComponentsInChildren<ColliderInfo>();

        for (int i = 0; i < hurtBoxes.Length; i++)
        {
            hurtBoxes[i].myScript = this.myScript;
            hurtBoxes[i].gameObject.SetActive(false);
        }
    }

    public void AttackColliderOn(string objStr)
    {
        for (int i = 0; i < hurtBoxes.Length; i++)
        {
            if (hurtBoxes[i].transform.name == objStr)
            {
                hurtBoxObj = hurtBoxes[i].gameObject;
                break;
            }
        }

        if (!hurtBoxObj.activeSelf)
            hurtBoxObj.SetActive(true);
    }

    public void AttackColliderOff()
    {
        if (hurtBoxObj != null && hurtBoxObj.activeSelf)
        {
            hurtBoxObj.SetActive(false);
            hurtBoxObj = null;
        }
    }
}

