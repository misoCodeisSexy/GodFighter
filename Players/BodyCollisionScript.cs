using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodFighter;

public class BodyCollisionScript : MonoBehaviour
{
    public CPlayerCtrl myScript;
    public ColliderInfo[] boxes;

    public float bodyMove = .5f;
    public float moveMirror;
    public bool onlyAnimation = false;
    public float distWalkValue;

    public float groundCollisionMass = 1; //캐릭터 사이 콜라이더 질량

    private void Start()
    {
        myScript = transform.parent.parent.gameObject.GetComponent<CPlayerCtrl>();
        boxes = transform.gameObject.GetComponentsInChildren<ColliderInfo>();

        for (int i = 0; i < boxes.Length; i++)
        {
            boxes[i].myScript = this.myScript;
        }

        if (myScript.mirror == -1) 
            moveMirror = -1;
        else
            moveMirror = 1;
    }

    public void FiexdUpdateGOF()
    {
        for (int i = 0; i < boxes.Length; i++)
        {
            boxes[i].FiexdUpdateGOF();
        }

        if (myScript != null)
        {
            float pushForce = CheckCollision(myScript.opBodyCollisionInfo.boxes);
        
            if (pushForce > 0)
            {
                if (myScript.transform.position.x < myScript.opPlayerScript.transform.position.x)
                {
                    myScript.transform.Translate(new Vector3(0, 0, 1.0f * pushForce * moveMirror));
                }
                else
                {
                    myScript.transform.Translate(new Vector3(0, 0, -1.0f * pushForce * moveMirror));
                }

                // wall
                if (myScript.opPlayerScript.transform.position.x == CGameManager.selectStateOption.rightBoundary
                    || myScript.opPlayerScript.transform.position.x == CGameManager.selectStateOption.leftBoundary)
                {
                    myScript.opPlayerScript.transform.Translate(new Vector3(0, 0, .1f * pushForce));
                }
            }

            pushForce = groundCollisionMass - Vector3.Distance(myScript.transform.position, myScript.opPlayerScript.transform.position);
            if (pushForce > 0) 
            {
                if (myScript.transform.position.x < myScript.opPlayerScript.transform.position.x)
                {
                    myScript.transform.Translate(new Vector3(0, 0, 1.0f * pushForce * moveMirror));
                }
                else
                {
                    myScript.transform.Translate(new Vector3(0, 0, -1.0f * pushForce * moveMirror));
                }

                // wall
                if (myScript.opPlayerScript.transform.position.x == CGameManager.selectStateOption.rightBoundary
                    || myScript.opPlayerScript.transform.position.x == CGameManager.selectStateOption.leftBoundary)
                {
                    myScript.opPlayerScript.transform.Translate(new Vector3(0, 0, .1f * pushForce));
                }       
            }
        }
    }

    public float CheckCollision(ColliderInfo[] opBodyBoxes)
    {
        float totalValue = 0;

        for(int i=0; i < boxes.Length; i++)
        {
            if (boxes[i].colliderType != ColliderType.bodyCollider)
                continue;

            for(int j=0; j < opBodyBoxes.Length; j++)
            {
                if (opBodyBoxes[j].colliderType != ColliderType.bodyCollider)
                    continue;

                Vector3 opBodyBoxPosition = opBodyBoxes[j].position.position;
                Vector3 bodyBoxPosition = boxes[i].position.position;

                float dist = Vector3.Distance(opBodyBoxPosition, bodyBoxPosition);

                //충돌ok
                if (dist <= opBodyBoxes[j].bodyRadius + boxes[i].bodyRadius)
                {
                    totalValue += (opBodyBoxes[j].bodyRadius + boxes[i].bodyRadius) - dist;
                }
            }
        }
        return totalValue;
    }
}
