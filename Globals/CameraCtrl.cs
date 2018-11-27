using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GodFighter;

////////////////////////////////////////////////////////////////////

///  CCameraCtrl :
///  1. 두 캐릭터 사이의 Center 위치 고정
///  2. 캐릭터 점프 시, 카메라 Y축 이동

////////////////////////////////////////////////////////////////////


public class CameraCtrl : MonoBehaviour
{
    [HideInInspector] public bool cinematicFreeze;
    [HideInInspector] public bool killCamMove;

    public CPlayerCtrl leftTarget;
    public CPlayerCtrl rightTarget;

    /// 캐릭터 사이의 카메라 줌아웃 최대값
    public float maxDistance = 6.0f;
   
    public float moveSpeed;
    public float rotSpeed;

    public Vector3 curLookAtPos;
    public Vector3 rotOffSet;

    public float maxZoom;
    public float minZoom;
    public float initFieldOfView = 25;

    private Vector3 initPosition = new Vector3(0, 2.2f, -4.5f);
    private Vector3 initRotation = new Vector3(0, 0, 0);

    private Transform leftTr;
    private Transform rightTr;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float standardDist;
    private float curDistance;
    private float targetFieldOfView;
    private float freeCameraSpeed;
    private string lastOwner;

    void Start ()
    {
        leftTr = leftTarget.transform;
        rightTr = rightTarget.transform;

        ResetCam();

        standardDist = Vector3.Distance(leftTr.position, rightTr.position);
    }

    public void FixedUpdateGOF()
    {
        if (killCamMove) return;
        if(CSceneManager.freeCamera)
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFieldOfView, Time.fixedDeltaTime * freeCameraSpeed * 1.8f);
            Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, targetPosition, Time.fixedDeltaTime * freeCameraSpeed * 1.08f);
            Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, targetRotation, Time.fixedDeltaTime * freeCameraSpeed * 1.8f);
        }
        else
        {
            curDistance = Mathf.Abs(leftTr.position.x - rightTr.position.x);

            if (curDistance >= maxDistance)
            {
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, initFieldOfView, Time.fixedDeltaTime * moveSpeed);
            }
            else
            {
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, initFieldOfView, Time.fixedDeltaTime * moveSpeed);
            }

            Vector3 newPosition = ((leftTr.position + rightTr.position) / 2) + initPosition;

            newPosition.x = Mathf.Clamp(newPosition.x, CGameManager.selectStateOption.leftBoundary,
                                                       CGameManager.selectStateOption.rightBoundary);

            newPosition.z = initPosition.z - Vector3.Distance(leftTr.position, rightTr.position) + standardDist;
            newPosition.z = Mathf.Clamp(newPosition.z, -maxZoom, -minZoom);


            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newPosition, Time.fixedDeltaTime * moveSpeed);

            Quaternion a = Quaternion.Slerp(Camera.main.transform.localRotation,
                                                    Quaternion.Euler(initRotation), Time.fixedDeltaTime * moveSpeed);
            Camera.main.transform.localRotation = a;

            curLookAtPos = Vector3.Lerp(curLookAtPos, ((leftTr.position + rightTr.position) / 2) + rotOffSet,
                                        Time.fixedDeltaTime * rotSpeed);

            Camera.main.transform.LookAt(curLookAtPos, Vector3.up);
        }
    }

    public void ResetCam()
    {
        Camera.main.transform.localPosition = initPosition;
        Camera.main.transform.position = initPosition;
        Camera.main.transform.localRotation = Quaternion.Euler(initRotation);
        Camera.main.fieldOfView = 25;
    }

    public void MoveCameraToLocation(string owner)
    {
        CSceneManager.freeCamera = true;
        lastOwner = owner;
    }

    public void MoveCameraToLocation(Vector3 targetPos, Vector3 targetRot, float targetFOV, float speed, string owner)
    {
        targetFieldOfView = targetFOV;
        targetPosition = targetPos;
        targetRotation = Quaternion.Euler(targetRot);
        freeCameraSpeed = speed;
        CSceneManager.freeCamera = true;
        lastOwner = owner;
    }

    public string GetCameraOwner()
    {
        return lastOwner;
    }

    public void ReleaseCam()
    {
        Camera.main.enabled = true;
        cinematicFreeze = false;
        CSceneManager.freeCamera = false;
        lastOwner = "";

        targetFieldOfView = 0;
        freeCameraSpeed = 0;
        targetPosition = Vector3.zero;
        targetRotation = Quaternion.identity;
    }

    private void OnDrawGizmos()
    {
        Vector3 cameraLeftBounds = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z));
        Vector3 cameraRightBounds = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, -Camera.main.transform.position.z));

        cameraLeftBounds.x = Camera.main.transform.position.x - (maxDistance / 2);
        cameraRightBounds.x = Camera.main.transform.position.x + (maxDistance / 2);

        Gizmos.DrawLine(cameraLeftBounds, cameraLeftBounds + new Vector3(0, 15, 0));
        Gizmos.DrawLine(cameraRightBounds, cameraRightBounds + new Vector3(0, 15, 0));
    }
}
